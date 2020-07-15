namespace FunctionApp.Kafka
{
    using Microsoft.Extensions.Configuration;

    public class KafkaSettings
    {
        public string Broker { get; private set; }
        
        public string ConsumerGroup { get; private set; }
        
        public string Topic { get; private set; }

        public string Username { get; private set; }
        
        public string Password { get; private set; }
        
        public bool SSlEnabled { get; private set; }

        public KafkaSettings(IConfiguration configuration)
        {
            ConsumerGroup = configuration["KAFKA_CONSUMER_GROUP"];
            SSlEnabled = configuration.GetValue<bool>("KAFKA_SSL_ENABLED", false);
            if (SSlEnabled)
            {
                Topic = configuration["KAFKA_TOPIC"];
                Username = configuration["KAFKA_USER"];
                Password = configuration["KAFKA_PASSWORD"];
                Broker = configuration["KAFKA_BROKER_AUTH"];
            }
            else
            {
                Broker = configuration["KAFKA_BROKER"];
            }
        }
    }
}