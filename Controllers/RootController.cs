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

        public RootController(TodoContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;

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
            return "Hello";
        }

        [HttpGet("param/{param}")]
        public ActionResult<string> GetParam(string param)
        {
            return "Got param " + param;
        }

        [HttpGet("exception")]
        public ActionResult<string> GetException()
        {
            throw new Exception("Sample Exception");
        }

        [HttpGet("api")]
        public async Task<ActionResult<string>> GetAPI()
        {
            var client = new RestClient();
            var request = new RestRequest("http://localhost:8080/", Method.Get);
            var response = await client.ExecuteAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return "Got from API: " + response.Content;
            }
            else
            {
                return response.StatusCode.ToString();
            }

        }

        [HttpGet("mysql")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            return await _context.TodoItems.ToListAsync();
        }

        [HttpGet("redis")]
        public async Task<ActionResult<string>> GetRedis()
        {
            var value = System.Text.Encoding.UTF8.GetBytes("my-value");
            var options = new DistributedCacheEntryOptions();
            _cache.Set("my-key", value, options);

            var res = await _cache.GetAsync("my-key");
            if (res != null)
            {
                return Encoding.UTF8.GetString(res);
            }
            else
            {
                return "Not Found";
            }
        }

        [HttpGet("kafka/produce")]
        public ActionResult<string> GetKafkaProduce()
        {
            _producer.Produce("sample_topic", new Message<string, string> { Key = "user", Value = "item1" });
            _producer.Flush(TimeSpan.FromSeconds(10));
            return "Kafka Produced";
        }

        [HttpGet("kafka/consume")]
        public ActionResult<string> GetKafkaConsume()
        {
            _consumer.Subscribe("sample_topic");
            var res = _consumer.Consume();
            return "Kafka Consumed - " + res.Message.Value;
        }
    }
}
