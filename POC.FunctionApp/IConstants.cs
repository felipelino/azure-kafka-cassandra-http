namespace FunctionApp
{
    public interface IConstants
    {
        const string Broker = "kafka04.northeurope.cloudapp.azure.com:9092";
        const string Topic = "person";
        const string ConsumerGroup = "functionGroup01";
    }
}