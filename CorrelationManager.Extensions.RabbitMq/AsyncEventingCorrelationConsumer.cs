using System;
using System.Threading.Tasks;
using CorrelationManager.Core.Interfaces;
using CorrelationManager.Core.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CorrelationManager.RabbitMq
{
    public class AsyncEventingCorrelationConsumer: AsyncEventingBasicConsumer
    {
        private readonly ICorrelationManager _correlationManager;
        
        public AsyncEventingCorrelationConsumer(IModel model, ICorrelationManagerFactory factory) : base(model)
        {
            _correlationManager = factory.CreateCorrelationManager();
        }

        public override Task HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey,
            IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            _correlationManager.CorrelationId = !string.IsNullOrWhiteSpace(properties.CorrelationId) ?
                properties.CorrelationId : Guid.NewGuid().ToString();

            using (_correlationManager.CreateScope())
            {
                return base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            }
        }
    }
}