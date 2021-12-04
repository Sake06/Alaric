using System;
using System.Text;
using Alaric.DB;
using Alaric.DB.Auditor;
using Alaric.Messaging.RabbitMq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Alaric.Messaging
{
   
    public class RabbitmqPublisher : IPublisher
    {
        private   string _hostname;
        private   string _password;
        private   string _queueName;
        private   string _username;
        private   int _port; 
        private IConnection _connection;

        public IOptions<RabbitMqConfiguration> rabbitMqOptions { get; }
        public Dispatcher dispatcher { get; }
        public ILogger<RabbitmqPublisher> logger { get; }

        public RabbitmqPublisher(IOptions<RabbitMqConfiguration> rabbitMqOptions, Dispatcher dispatcher, ILogger<RabbitmqPublisher> logger)
        {
            this.rabbitMqOptions = rabbitMqOptions;
            this.dispatcher = dispatcher;
            this.logger = logger;
            logger.LogDebug("Publisher Started");

        }

        private void onLogGenerated(object arg1, AuditLogGeneratedEventArgs arg2)
        {
            this.Send(new QueueModel()
            {
                AuditLog = arg2.Log 
            });
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password,
                    Port = _port
                };
                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create connection: {ex.Message}");
            }
        }

        private bool ConnectionExists()
        {
            if (_connection != null)
            {
                return true;
            }

            CreateConnection();

            return _connection != null;
        }

        public void Send(QueueModel model)
        {
            if (ConnectionExists())
            {
                using (var channel = _connection.CreateModel())
                {
                    channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                    var json = JsonConvert.SerializeObject(model);
                    var body = Encoding.UTF8.GetBytes(json);
                    logger.LogDebug("Sent To Queue :" + JsonConvert.SerializeObject(model.AuditLog));

                    channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
                }
            }
        }

        public void Init()
        {
            _queueName = this.rabbitMqOptions.Value.PublishQueueName;
            _hostname = rabbitMqOptions.Value.Hostname;
            _username = rabbitMqOptions.Value.UserName;
            _password = rabbitMqOptions.Value.Password;
            _port = rabbitMqOptions.Value.Port;

            dispatcher.OnAuditLogGenerated += onLogGenerated;
            CreateConnection();
        }
    }
}
