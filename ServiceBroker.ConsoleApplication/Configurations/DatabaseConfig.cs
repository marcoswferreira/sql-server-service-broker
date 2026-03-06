using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using ServiceBroker.ConsoleApplication.Interfaces;

namespace ServiceBroker.ConsoleApplication.Configurations;

public static class DatabaseConfig
{
    public static void StartQueue(IDbConnectionFactory connectionFactory, IOptions<ServiceBrokerOptions> options)
    {
        var settings = options.Value;
        using SqlConnection connection = connectionFactory.CreateConnection();

        CreateQueue(connection, settings.Queue);
        CreateService(connection, settings.Service, settings.Queue);
        CreateMessageType(connection, settings.MessageType);
        CreateContract(connection, settings.Contract, settings.MessageType);
    }

    public static void CreateMessageType(SqlConnection connection, string messageTypeName)
    {
        if (!ExistsInDatabase(connection, "sys.service_message_types", "name", messageTypeName))
        {
            string createMessageTypeQuery = $"CREATE MESSAGE TYPE {messageTypeName} VALIDATION = NONE;";
            ExecuteNonQuery(connection, createMessageTypeQuery);
        }
    }

    public static void CreateContract(SqlConnection connection, string contractName, string messageTypeName)
    {
        if (!ExistsInDatabase(connection, "sys.service_contracts", "name", contractName))
        {
            string createContractQuery = $"CREATE CONTRACT {contractName} ([{messageTypeName}] SENT BY ANY);";
            ExecuteNonQuery(connection, createContractQuery);
        }
    }

    public static void CreateService(SqlConnection connection, string serviceName, string queueName)
    {
        if (!ExistsInDatabase(connection, "sys.services", "name", serviceName))
        {
            string createServiceQuery = $"CREATE SERVICE {serviceName} ON QUEUE {queueName} ([DEFAULT]);";
            ExecuteNonQuery(connection, createServiceQuery);
        }
    }

    public static bool ExistsInDatabase(SqlConnection connection, string tableName, string columnName, string value)
    {
        string query = $"SELECT COUNT(*) FROM {tableName} WHERE {columnName} = @Value";

        using SqlCommand command = connection.CreateCommand();
        command.CommandText = query;

        command.Parameters.Add("@Value", SqlDbType.NVarChar).Value = value;
        int count = (int)command.ExecuteScalar();

        return count > 0;
    }

    public static void ExecuteNonQuery(SqlConnection connection, string query)
    {
        using SqlCommand command = connection.CreateCommand();
        command.CommandText = query;
        command.ExecuteNonQuery();
    }

    public static void CreateQueue(SqlConnection connection, string queueName)
    {
        string query = "SELECT COUNT(*) FROM sys.service_queues WHERE name = @QueueName";

        using SqlCommand command = connection.CreateCommand();
        command.CommandText = query;

        command.Parameters.Add("@QueueName", SqlDbType.NVarChar).Value = queueName;
        int count = (int)command.ExecuteScalar();

        if (count == 0)
        {
            // Note: Enabling broker requires exclusive lock in some environments, but this works for demo.
            string enableQueueQuery = "ALTER DATABASE CURRENT SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE;";

            command.CommandText = enableQueueQuery;
            try { command.ExecuteNonQuery(); } catch { /* Ignore if already enabled or failed */ }

            string createQueueQuery = $@"CREATE QUEUE {queueName} 
                                        WITH STATUS = ON, 
                                        RETENTION = ON, 
                                        ACTIVATION (
                                            MAX_QUEUE_READERS = 2, 
                                            EXECUTE AS SELF
                                        ) ON [DEFAULT];";

            command.CommandText = createQueueQuery;
            command.ExecuteNonQuery();
        }
    }
}
