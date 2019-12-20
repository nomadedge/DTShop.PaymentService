using AutoMapper;
using DTShop.PaymentService.Core.Models;
using DTShop.PaymentService.Data.Entities;
using DTShop.PaymentService.Data.Repositories;
using DTShop.PaymentService.RabbitMQ;
using DTShop.PaymentService.RabbitMQ.Consumers;
using DTShop.PaymentService.RabbitMQ.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IMapper _mapper;

        public PaymentsController(
            ILogger<PaymentsController> logger,
            IRabbitManager rabbitManager,
            IRpcClient rpcClient,
            IPaymentRepository paymentRepository,
            IMapper mapper)
        {
            _logger = logger;
            _rabbitManager = rabbitManager;
            _rpcClient = rpcClient;
            _paymentRepository = paymentRepository;
            _mapper = mapper;
        }

        [HttpGet("filter/{username}")]
        public ActionResult<List<Payment>> GetPaymentsByUsername(string username)
        {
            try
            {
                _logger.LogInformation("Getting orders by username");

                var payments = _paymentRepository.GetPaymentsByUsername(username).ToList();

                return payments;
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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


                var orderRequestDto = new OrderRequestDto { OrderId = orderId };
                _rpcClient.Open();
                var order = _rpcClient.Call(orderRequestDto);
                _rpcClient.Close();


                if (order.OrderId == 0)
                {
                    throw new ArgumentException("No order with such id.");
                }

                if (userDetails.Username != order.Username)
                {
                    throw new ArgumentException("Usernames in order and user details should be equal.");
                }

                if (order.Status.ToLower() != "collecting")
                {
                    throw new InvalidOperationException("Order status should be \"Collecting\".");
                }

                switch (userDetails.CardAuthorizationInfo.ToLower())
                {
                    case "authorized":
                        order.PaymentId = DateTime.Now.Ticks;
                        order.Status = "Paid";
                        break;
                    case "unauthorized":
                        order.PaymentId = DateTime.Now.Ticks;
                        order.Status = "Failed";
                        break;
                }

                var payment = new Payment
                {
                    PaymentId = order.PaymentId.Value,
                    OrderId = order.OrderId,
                    Username = order.Username,
                    TotalCost = order.TotalCost,
                    IsPassed = order.Status == "Paid" ? true : false
                };

                await _paymentRepository.AddPaymentAsync(payment);

                var payForOrderDto = _mapper.Map<PayForOrderDto>(order);

                _rabbitManager.Publish(payForOrderDto, "PaymentService_OrderExchange", ExchangeType.Direct, "PayForOrder");

                _logger.LogInformation("{Username} has finished payment for the order with OrderId {OrderId} with status {Status}.",
                    order.Username, order.OrderId, order.Status);

                return order;
            }
            catch (TimeoutException e)
            {
                _logger.LogInformation("{Username} has failed to perform payment for the order with OrderId {OrderId}.",
                    userDetails.Username, orderId);

                return StatusCode(408, e.Message);
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
