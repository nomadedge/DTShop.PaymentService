using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace DTShop.PaymentService.RabbitMQ.Consumers
{
    public class RpcClient : IRpcClient
    {
        private readonly DefaultObjectPool<IModel> _objectPool;
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;
        private readonly BlockingCollection<string> _respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties _props;

        public RpcClient(IPooledObjectPolicy<IModel> objectPolicy)
        {
            _objectPool = new DefaultObjectPool<IModel>(objectPolicy, Environment.ProcessorCount * 2);
            _channel = _objectPool.Get();
            _replyQueueName = _channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(_channel);

            _props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            _props.CorrelationId = correlationId;
            _props.ReplyTo = _replyQueueName;

            _consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    _respQueue.Add(response);
                }
            };
        }

        public string Call<T>(T message) where T : class
        {
            var content = JsonConvert.SerializeObject(message);
            var messageBytes = Encoding.UTF8.GetBytes(content);
            _channel.BasicPublish(
                exchange: "PaymentService_OrderExchange",
                routingKey: "GetOrderRequest",
                basicProperties: _props,
                body: messageBytes);

            _channel.BasicConsume(
                consumer: _consumer,
                queue: _replyQueueName,
                autoAck: true);

            return _respQueue.Take();
        }

        public void Close()
        {
            _objectPool.Return(_channel);
        }
    }
}
