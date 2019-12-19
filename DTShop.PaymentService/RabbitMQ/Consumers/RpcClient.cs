using DTShop.PaymentService.Core.Models;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace DTShop.PaymentService.RabbitMQ.Consumers
{
    public class RpcClient : IRpcClient
    {
        private readonly DefaultObjectPool<IModel> _objectPool;
        private readonly BlockingCollection<string> _respQueue = new BlockingCollection<string>();
        private IModel _channel;
        private string _replyQueueName;
        private EventingBasicConsumer _consumer;
        private IBasicProperties _props;
        private ManualResetEvent _signal;

        public RpcClient(IPooledObjectPolicy<IModel> objectPolicy)
        {
            _objectPool = new DefaultObjectPool<IModel>(objectPolicy, Environment.ProcessorCount * 2);
        }

        public void Open()
        {
            _channel = _objectPool.Get();
            _replyQueueName = _channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(_channel);

            _props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            _props.CorrelationId = correlationId;
            _props.ReplyTo = _replyQueueName;

            _signal = new ManualResetEvent(false);

            _consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    _signal.Set();
                    _respQueue.Add(response);
                }
            };
        }

        public OrderModel Call<T>(T message) where T : class
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

            bool timeout = !_signal.WaitOne(TimeSpan.FromSeconds(5));

            if (timeout)
            {
                throw new TimeoutException("Order Service is now unreachable. Try again later.");
            }

            return JsonConvert.DeserializeObject<OrderModel>(_respQueue.Take());
        }

        public void Close()
        {
            _objectPool.Return(_channel);
        }
    }
}
