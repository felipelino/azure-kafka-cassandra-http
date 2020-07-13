namespace FunctionApp
{
    using System;
    using FunctionApp.Dtos;
    using FunctionApp.Kafka;
    using FunctionApp.Repository;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;

    using Microsoft.Azure.WebJobs.Extensions.Kafka;
    using Microsoft.Extensions.Configuration;
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
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
                var cassandraSettings = new CassandraSettings(configuration);
                var cassandraConnectionFactory = new CassandraConnectionFactory(logger, cassandraSettings);
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