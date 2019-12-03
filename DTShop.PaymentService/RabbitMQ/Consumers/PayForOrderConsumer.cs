using DTShop.PaymentService.Data.Entities;
using DTShop.PaymentService.Data.Repositories;
using DTShop.PaymentService.RabbitMQ.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DTShop.PaymentService.RabbitMQ.Consumers
{
    public class PayForOrderConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitManager _rabbitManager;
        private readonly ILogger<PayForOrderConsumer> _logger;

        public PayForOrderConsumer(
            IServiceScopeFactory scopeFactory,
            IRabbitManager rabbitManager,
            ILogger<PayForOrderConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _rabbitManager = rabbitManager;
            _logger = logger;
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("OrderResponse", ExchangeType.Direct, true, false, null);
            _channel.QueueDeclare("PaymentService_GetOrderResponse", true, false, false, null);
            _channel.QueueBind("PaymentService_GetOrderResponse", "OrderResponse", "GetOrderResponse", null);
            _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body);
                HandleMessage(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume("PaymentService_GetOrderResponse", false, consumer);
            return Task.CompletedTask;
        }

        private async void HandleMessage(string content)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                try
                {
                    var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
                    var orderResponseDto = JsonConvert.DeserializeObject<OrderResponseDto>(content);

                    if (orderResponseDto.Order.Status.ToLower() != "collecting")
                    {
                        throw new InvalidOperationException("Order status should be \"Collecting\".");
                    }

                    switch (orderResponseDto.CardAuthorizationInfo.ToLower())
                    {
                        case "authorized":
                            orderResponseDto.Order.PaymentId = DateTime.Now.Ticks;
                            orderResponseDto.Order.Status = "Paid";
                            break;
                        case "unauthorized":
                            orderResponseDto.Order.PaymentId = DateTime.Now.Ticks;
                            orderResponseDto.Order.Status = "Failed";
                            break;
                    }

                    var payment = new Payment
                    {
                        PaymentId = orderResponseDto.Order.PaymentId.Value,
                        OrderId = orderResponseDto.Order.OrderId,
                        Username = orderResponseDto.Order.Username,
                        TotalCost = orderResponseDto.Order.TotalCost,
                        IsPassed = orderResponseDto.Order.Status == "Paid" ? true : false
                    };

                    await paymentRepository.AddPaymentAsync(payment);

                    var payForOrderDto = new PayForOrderDto
                    {
                        OrderId = orderResponseDto.Order.OrderId,
                        PaymentId = orderResponseDto.Order.PaymentId.Value,
                        Status = orderResponseDto.Order.Status.ToString()
                    };
                    _rabbitManager.Publish(payForOrderDto, "OrderRequest", "direct", "PayForOrder");

                    _logger.LogInformation("{Username} has finished payment for the order with OrderId {OrderId} with status {Status}.",
                        orderResponseDto.Order.Username, orderResponseDto.Order.OrderId, orderResponseDto.Order.Status);
                }
                catch (Exception e)
                {

                }
            }
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
