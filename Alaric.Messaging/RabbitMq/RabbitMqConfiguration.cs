using System;
namespace Alaric.Messaging.RabbitMq
{
    public class RabbitMqConfiguration
    {
        public string Hostname { get; set; }

        public string ConsumeQueueName { get; set; }

        public string PublishQueueName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int  Port { get; set; }
    }
}
