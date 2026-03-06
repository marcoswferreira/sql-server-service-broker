namespace ServiceBroker.ConsoleApplication.Configurations;

public class ServiceBrokerOptions
{
    public const string SectionName = "ServiceBroker";

    public string Queue { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Contract { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
}
