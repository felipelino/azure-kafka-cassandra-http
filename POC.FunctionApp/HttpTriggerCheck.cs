
namespace FunctionApp
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    using System.Threading;
    using Confluent.Kafka;
    using FunctionApp.Dtos;
    using FunctionApp.Kafka;
    using FunctionApp.Repository;
    using Microsoft.Extensions.Configuration;

    public static class HttpTriggerCheck
    {
        private static IProducer<int, string> _producer = null;
        private static KafkaSettings _kafkaSettings = null;
        private static int _timeoutInSeconds = 60;
        
        [FunctionName("HttpTrigger")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous,   "get","post", Route = null)]
            HttpRequest req, ILogger log)
        { 
            if (_producer == null)
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
                _timeoutInSeconds = configuration.GetValue<int>("TIMEOUT_SECONDS", 60);
                _kafkaSettings = new KafkaSettings(configuration);
                var producerConfig = _kafkaSettings.SSlEnabled
                    ? new ProducerConfig
                    {
                        BootstrapServers = _kafkaSettings.Broker,
                        SaslMechanism = SaslMechanism.Plain,
                        SaslUsername = _kafkaSettings.Username,
                        SaslPassword = _kafkaSettings.Password,
                        SecurityProtocol = SecurityProtocol.SaslPlaintext,
                    }
                    : new ProducerConfig
                    {
                        BootstrapServers = _kafkaSettings.Broker,
                    };
                
                _producer = new ProducerBuilder<int, string>(producerConfig)
                    .Build();
            }
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Person data = JsonConvert.DeserializeObject<Person>(requestBody);
            var result = "None";
            if (data != null)
            { 
                try
                {
                    string json = JsonConvert.SerializeObject(data);
                    var task = _producer.ProduceAsync(
                        IConstants.Topic, new Message<int, string> { Key = data.Id, Value = json });
                    var timeout = _timeoutInSeconds * 1000;
                    if (await Task.WhenAny(task, Task.Delay(timeout, new CancellationToken())) == task)
                    {
                        // Task completed within timeout.
                        // Consider that the task may have faulted or been canceled.
                        // We re-await the task so that any exceptions/cancellation is rethrown.
                        var deliveryReport =  await task;
                        result = $"Data:[{data}] sent to Kafka - delivered to: {deliveryReport.TopicPartitionOffset}";
                    }
                    else
                    {
                        // timeout/cancellation logic
                        result = $"Timeout when trying to publish message data:[{data}] to kafka. SSL Enabled?:[{_kafkaSettings.SSlEnabled}]";
                    }
                }
                catch (Exception e)
                {
                    result = $"Fail to publish message [{data}] to Kafka - Exception:[{e}]";
                }
            }

            return (ActionResult) new OkObjectResult(result);

        }
    }
}