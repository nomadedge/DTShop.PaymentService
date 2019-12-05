using DTShop.PaymentService.Core.Models;
using DTShop.PaymentService.Data.Entities;
using DTShop.PaymentService.Data.Repositories;
using DTShop.PaymentService.RabbitMQ;
using DTShop.PaymentService.RabbitMQ.Consumers;
using DTShop.PaymentService.RabbitMQ.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace DTShop.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ILogger<PaymentsController> _logger;
        private readonly IRabbitManager _rabbitManager;
        private readonly IRpcClient _rpcClient;
        private readonly IPaymentRepository _paymentRepository;

        public PaymentsController(
            ILogger<PaymentsController> logger,
            IRabbitManager rabbitManager,
            IRpcClient rpcClient,
            IPaymentRepository paymentRepository)
        {
            _logger = logger;
            _rabbitManager = rabbitManager;
            _rpcClient = rpcClient;
            _paymentRepository = paymentRepository;
        }

        [HttpPut("{orderId}")]
        public async Task<ActionResult<OrderModel>> PayForOrderAsync(int orderId, UserDetailsModel userDetails)
        {
            try
            {
                _logger.LogInformation("{Username} has started payment for the order with OrderId {OrderId}.",
                    userDetails.Username, orderId);

                if (userDetails.CardAuthorizationInfo.ToLower() != "authorized" &&
                    userDetails.CardAuthorizationInfo.ToLower() != "unauthorized")
                {
                    throw new ArgumentException("CardAuthorizationInfo is not valid.");
                }


                var orderRequestDto = new OrderRequestDto
                {
                    OrderId = orderId
                };
                
                var response = _rpcClient.Call(orderRequestDto);
                _rpcClient.Close();

                var orderResponseDto = JsonConvert.DeserializeObject<OrderModel>(response);

                if (userDetails.Username != orderResponseDto.Username)
                {
                    throw new ArgumentException("Usernames in order and user details should be equal.");
                }

                if (orderResponseDto.Status.ToLower() != "collecting")
                {
                    throw new InvalidOperationException("Order status should be \"Collecting\".");
                }

                switch (userDetails.CardAuthorizationInfo.ToLower())
                {
                    case "authorized":
                        orderResponseDto.PaymentId = DateTime.Now.Ticks;
                        orderResponseDto.Status = "Paid";
                        break;
                    case "unauthorized":
                        orderResponseDto.PaymentId = DateTime.Now.Ticks;
                        orderResponseDto.Status = "Failed";
                        break;
                }

                var payment = new Payment
                {
                    PaymentId = orderResponseDto.PaymentId.Value,
                    OrderId = orderResponseDto.OrderId,
                    Username = orderResponseDto.Username,
                    TotalCost = orderResponseDto.TotalCost,
                    IsPassed = orderResponseDto.Status == "Paid" ? true : false
                };

                await _paymentRepository.AddPaymentAsync(payment);

                var payForOrderDto = new PayForOrderDto
                {
                    OrderId = orderResponseDto.OrderId,
                    PaymentId = orderResponseDto.PaymentId.Value,
                    Status = orderResponseDto.Status.ToString()
                };
                _rabbitManager.Publish(payForOrderDto, "PaymentService_OrderExchange", "direct", "PayForOrder");

                _logger.LogInformation("{Username} has finished payment for the order with OrderId {OrderId} with status {Status}.",
                    orderResponseDto.Username, orderResponseDto.OrderId, orderResponseDto.Status);

                return orderResponseDto;
            }
            catch (Exception e)
            {
                _logger.LogInformation("{Username} has failed to perform payment for the order with OrderId {OrderId}.",
                    userDetails.Username, orderId);

                return BadRequest(e.Message);
            }
        }
    }
}
