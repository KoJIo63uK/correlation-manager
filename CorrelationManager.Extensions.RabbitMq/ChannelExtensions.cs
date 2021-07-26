using System;
using CorrelationManager.Core.Services;
using RabbitMQ.Client;

namespace CorrelationManager.RabbitMq
{
    public static class ChannelExtensions
    {
        /// <summary>
        /// (Extension method) Convenience overload of BasicPublish.
        /// </summary>
        /// <remarks>
        /// The publication occurs with mandatory=false and correlation properties
        /// </remarks>
        public static void BasicPublish(
            this IModel model, 
            ICorrelationManagerFactory correlationManagerFactory, 
            string exchange, string routingKey, 
            IBasicProperties basicProperties, 
            ReadOnlyMemory<byte> body)
        {
            basicProperties ??= model.CreateBasicProperties();

            var correlationManager = correlationManagerFactory.CreateCorrelationManager();
            basicProperties.CorrelationId = correlationManager.CorrelationId;
            
            model.BasicPublish(exchange, routingKey, false, basicProperties, body);
        }
    }
}