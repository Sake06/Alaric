using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alaric.DB;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Alaric.Messaging.RabbitMq
{
    public class RabbitMqConsumer : BackgroundService, IConsumer
    {
        private IModel _channel;
        private IConnection _connection; 
        private readonly Orchestrator orchestrator;
        private readonly ILogger<RabbitMqConsumer> logger;
        private readonly string _hostname;
        private readonly string _queueName;
        private readonly string _username;
        private readonly int  _port;
        private readonly string _password;
        public RabbitMqConsumer( IOptions<RabbitMqConfiguration> rabbitMqOptions, Orchestrator orchestrator, ILogger<RabbitMqConsumer> Logger)
        {
            _hostname = rabbitMqOptions.Value.Hostname;
            _queueName = rabbitMqOptions.Value.ConsumeQueueName;
            _username = rabbitMqOptions.Value.UserName;
            _password = rabbitMqOptions.Value.Password;
            _port = rabbitMqOptions.Value.Port;
            this.orchestrator = orchestrator;
            logger = Logger;
            InitializeRabbitMqListener();
        }

        private void InitializeRabbitMqListener()
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password,
                Port = _port
            };
            factory.DispatchConsumersAsync = true;
            _connection = factory.CreateConnection();
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            logger.LogDebug("Consumer Started");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
 
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());

                try
                {
                    var trademodel = JsonConvert.DeserializeObject<QueueModel>(content);

                    await HandleMessageAsync(trademodel);


                }
                catch (Exception ex)
                {
                    logger.LogError("Consume Message :" + ex.Message);


                }


                _channel.BasicAck(ea.DeliveryTag, false);
                await Task.Yield();

            };
            consumer.Shutdown +=   OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerCancelled;

            _channel.BasicConsume(_queueName, false, consumer);

            return Task.CompletedTask;
        }

        public async Task HandleMessageAsync(QueueModel model)
        {
           await orchestrator.MergeAsync(model);
 
        }

        private async Task OnConsumerCancelled(object sender, ConsumerEventArgs e)
        {
        }

        private async Task OnConsumerUnregistered(object sender, ConsumerEventArgs e)
        {
        }

        private async Task OnConsumerRegistered(object sender, ConsumerEventArgs e)
        {
        }

        private async Task OnConsumerShutdown(object sender, ShutdownEventArgs e)
        {
        }

        private void  RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

        
    }
}