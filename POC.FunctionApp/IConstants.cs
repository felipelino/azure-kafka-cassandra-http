namespace FunctionApp
{
    public interface IConstants
    {
        const string Broker = "HOST:9092";
        const string Topic = "person";
        const string ConsumerGroup = "functionGroup01";
    }
}