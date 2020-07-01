
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

    public static class HttpTriggerCheck
    {
        static ProducerConfig _config = new ProducerConfig { BootstrapServers = IConstants.Broker };
        private static IProducer<int, string> _producer = null;
        
        [FunctionName("HttpTriggerCheck")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous,   "get","post", Route = null)]
            HttpRequest req, ILogger log)
        { 
            if (_producer == null)
            {
                  _producer = new ProducerBuilder<int, string>(_config)
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
                    var timeout = 60000;
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
                        result = $"Timeout when trying to publish message data:[{data}] to kafka";
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