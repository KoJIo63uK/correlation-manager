using System;
using CorrelationManager.Core.Interfaces;
using CorrelationManager.Core.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CorrelationManager.RabbitMq
{
    public class EventingCorrelationConsumer: EventingBasicConsumer
    {
        private readonly ICorrelationManager _correlationManager;
        
        public EventingCorrelationConsumer(IModel model, ICorrelationManagerFactory factory) : base(model)
        {
            _correlationManager = factory.CreateCorrelationManager();
        }
        
        ///<summary>
        /// Invoked when a delivery arrives for the consumer.
        /// </summary>
        /// <remarks>
        /// Handlers must copy or fully use delivery body before returning.
        /// Accessing the body at a later point is unsafe as its memory can
        /// be already released.
        /// </remarks>
        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            if (!string.IsNullOrWhiteSpace(properties.CorrelationId))
            {
                _correlationManager.CorrelationId = properties.CorrelationId;
            }
            else
            {
                _correlationManager.CorrelationId = Guid.NewGuid().ToString();
            }
            
            using (_correlationManager.CreateScope())
            {
                base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            }
        }
    }
}