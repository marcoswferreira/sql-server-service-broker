using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceBroker.ConsoleApplication.Configurations;
using ServiceBroker.ConsoleApplication.Interfaces;

namespace ServiceBroker.ConsoleApplication.Services;

public class PublisherService : IPublisherService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ServiceBrokerOptions _options;
    private readonly ILogger<PublisherService> _logger;

    public PublisherService(
        IDbConnectionFactory connectionFactory, 
        IOptions<ServiceBrokerOptions> options, 
        ILogger<PublisherService> logger)
    {
        _connectionFactory = connectionFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishMessageAsync(string message = "Hello World!", CancellationToken cancellationToken = default)
    {
        using SqlConnection connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using SqlCommand command = connection.CreateCommand();

        try
        {
            command.CommandText = GetQuery();
            command.Transaction = connection.BeginTransaction();

            command.Parameters.Add(new SqlParameter("@dialog_handle", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            });
            
            command.Parameters.Add(new SqlParameter("@message_body", SqlDbType.NVarChar, -1) { Value = message });
            command.Parameters.Add(new SqlParameter("@service_name", SqlDbType.NVarChar, 128) { Value = _options.Queue });

            await command.ExecuteNonQueryAsync(cancellationToken);
            await command.Transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while publishing message.");
            if (command.Transaction != null)
            {
                await command.Transaction.RollbackAsync(cancellationToken);
            }
        }
    }

    private string GetQuery()
    {
        return $"""
                BEGIN DIALOG @dialog_handle
                    FROM SERVICE {_options.Service}
                    TO SERVICE '{_options.Service}', 'CURRENT DATABASE'
                    ON CONTRACT {_options.Contract}
                    WITH ENCRYPTION = OFF;
                SEND ON CONVERSATION @dialog_handle
                    MESSAGE TYPE {_options.MessageType}(@message_body);
                """;
    }
}