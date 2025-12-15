using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TodoApi.Models;
using RestSharp;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace TodoApi.Controllers
{
    [Route("")]
    [ApiController]
    public class RootController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly IDistributedCache _cache;
        private readonly IProducer<string, string> _producer;
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger<RootController> _logger;

        public RootController(TodoContext context, IDistributedCache cache, ILogger<RootController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = "kafka:9092",
            };
            var producer = new ProducerBuilder<string, string>(producerConfig).Build();
            _producer = producer;

            var consumerConfig = new ConsumerConfig
            {
                // User-specific properties that you must set
                BootstrapServers = "kafka:9092",
                GroupId = "foo",
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };
            var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
            _consumer = consumer;
        }

        [HttpGet]
        public ActionResult<string> Get()
        {
            _logger.LogInformation("hello request hit");
            return "Hello";
        }

        [HttpGet("param/{param}")]
        public ActionResult<string> GetParam(string param)
        {
            _logger.LogInformation("param called");
            return "Got param " + param;
        }

        [HttpGet("exception")]
        public ActionResult<string> GetException()
        {
            _logger.LogWarning("exception occur");
            throw new Exception("Sample Exception");
        }

        [HttpGet("api")]
        public async Task<ActionResult<string>> GetAPI()
        {
            _logger.LogInformation("Calling external API");
            var client = new RestClient();
            var request = new RestRequest("http://localhost:8080/", Method.Get);
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("External API success");
                return "Got from API: " + response.Content;
            }
            else
            {
                _logger.LogWarning("External API failed with status {StatusCode}", response.StatusCode);
                return response.StatusCode.ToString();
            }

        }

        [HttpGet("mysql")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            _logger.LogInformation("Fetching TodoItems from MySQL");
            return await _context.TodoItems.ToListAsync();
        }

        [HttpGet("redis")]
        public async Task<ActionResult<string>> GetRedis()
        {
            _logger.LogInformation("Setting value in Redis");
            var value = System.Text.Encoding.UTF8.GetBytes("my-value");
            var options = new DistributedCacheEntryOptions();
            _cache.Set("my-key", value, options);

            var res = await _cache.GetAsync("my-key");
            if (res != null)
            {
                _logger.LogInformation("Redis key retrieved");
                return Encoding.UTF8.GetString(res);
            }
            else
            {
                _logger.LogWarning("Redis key not found");
                return "Not Found";
            }
        }

        [HttpGet("kafka/produce")]
        public ActionResult<string> GetKafkaProduce()
        {
            _producer.Produce("sample_topic", new Message<string, string> { Key = "user", Value = "item1" });
            _producer.Flush(TimeSpan.FromSeconds(10));
            _logger.LogInformation("Kafka message produced");
            return "Kafka Produced";
        }

        [HttpGet("kafka/consume")]
        public ActionResult<string> GetKafkaConsume()
        {
            _consumer.Subscribe("sample_topic");
            var res = _consumer.Consume();
             _logger.LogInformation(
            "Kafka message consumed. Topic={Topic}, Value={Value}",
            res.Topic,
            res.Message.Value
            );
            return "Kafka Consumed - " + res.Message.Value;
        }
    }
}
