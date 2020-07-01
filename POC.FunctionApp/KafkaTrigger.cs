namespace FunctionApp
{
    using FunctionApp.Dtos;
    using FunctionApp.Repository;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;

    using Microsoft.Azure.WebJobs.Extensions.Kafka;
    using Newtonsoft.Json;

    public static class KafkaTrigger
    {
        private static PersonRepository _personRepository;
        
        [FunctionName("KafkaTriggerCheck")]
        public static void StringTopic(
            [KafkaTrigger(brokerList: IConstants.Broker, topic: IConstants.Topic, ConsumerGroup = IConstants.ConsumerGroup)] KafkaEventData<string>[] kafkaEvents,
            ILogger logger)
        {
            if (_personRepository == null)
            {
                CassandraSettings settings = new CassandraSettings
                {
                    ContactPoints = "HOST",
                    Port = 9042,
                    UserName = "USER",
                    Password = "PASSWORD",
                    KeySpace = "app",
                };
                var cassandraConnectionFactory = new CassandraConnectionFactory(logger, settings);
                _personRepository = new PersonRepository(cassandraConnectionFactory.Session);
            }
            
            foreach (var kafkaEvent in kafkaEvents)
            {
                Person person = JsonConvert.DeserializeObject<Person>(kafkaEvent.Value);
                _personRepository.Save(person);
                logger.LogInformation($"Person:[{person}] persisted with success");
            }
        }
    }
}