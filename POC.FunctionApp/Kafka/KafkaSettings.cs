namespace FunctionApp.Kafka
{
    using Microsoft.Extensions.Configuration;

    public class KafkaSettings
    {
        public string Broker { get; private set; }
        public string ConsumerGroup { get; private set; }
        public string Topic { get; private set; }

        public KafkaSettings(IConfiguration configuration)
        {
            Broker = configuration["KAFKA_BROKER"];
            ConsumerGroup = configuration["KAFKA_CONSUMER_GROUP"];
            Topic = configuration["KAFKA_TOPIC"];
        }
    }
}