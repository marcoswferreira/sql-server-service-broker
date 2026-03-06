using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceBroker.ConsoleApplication.Configurations;
using ServiceBroker.ConsoleApplication.Interfaces;

namespace ServiceBroker.ConsoleApplication.Services;

public class SubscriberService : ISubscribeService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMessageHandler _messageHandler;
    private readonly ServiceBrokerOptions _options;
    private readonly ILogger<SubscriberService> _logger;

    public SubscriberService(
        IDbConnectionFactory connectionFactory,
        IMessageHandler messageHandler,
        IOptions<ServiceBrokerOptions> options,
        ILogger<SubscriberService> logger)
    {
        _connectionFactory = connectionFactory;
        _messageHandler = messageHandler;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SubscribeMessageAsync(CancellationToken cancellationToken = default)
    {
        using SqlConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using SqlCommand command = connection.CreateCommand();
        
        try
        {
            command.Transaction = connection.BeginTransaction();
            command.CommandText = GetQuery();
            command.CommandTimeout = 60; // 60 seconds

            var dialogHandlerParam = new SqlParameter("@dialog_handle", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };

            var messageParam = new SqlParameter("@message", SqlDbType.NVarChar, -1)
            {
                Direction = ParameterDirection.Output
            };

            command.Parameters.Add(dialogHandlerParam);
            command.Parameters.Add(messageParam);

            // Execute waiting query
            await command.ExecuteNonQueryAsync(cancellationToken);

            if (messageParam.Value != DBNull.Value && messageParam.Value is string messageContent)
            {
                if (dialogHandlerParam.Value is Guid dialogId && dialogId != Guid.Empty)
                {
                    command.CommandText = "END CONVERSATION @dialog_handle";
                    command.Parameters.Clear();
                    command.Parameters.Add(new SqlParameter("@dialog_handle", SqlDbType.UniqueIdentifier) { Value = dialogId });
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                // Process message via decoupled handler
                await _messageHandler.HandleMessageAsync(messageContent, cancellationToken);
            }

            await command.Transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error occurred while subscribing to messages.");
            if (command.Transaction != null)
            {
                await command.Transaction.RollbackAsync(cancellationToken);
            }
        }
    }

    public string GetQuery()
    {
        return $"""
                WAITFOR (
                    RECEIVE TOP(1) 
                        @dialog_handle = conversation_handle, 
                        @message = message_body 
                    FROM dbo.[{_options.Queue}]
                ) TIMEOUT 60000
                """;
    }
}
