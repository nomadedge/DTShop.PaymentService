using DTShop.PaymentService.Core.Models;
using DTShop.PaymentService.RabbitMQ;
using DTShop.PaymentService.RabbitMQ.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace DTShop.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ILogger<PaymentsController> _logger;
        private readonly IRabbitManager _rabbitManager;

        public PaymentsController(
            ILogger<PaymentsController> logger,
            IRabbitManager rabbitManager)
        {
            _logger = logger;
            _rabbitManager = rabbitManager;
        }

        [HttpPut("{orderId}")]
        public ActionResult<OrderRequestDto> PayForOrder(int orderId, UserDetailsModel userDetails)
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
                    OrderId = orderId,
                    Username = userDetails.Username,
                    CardAuthorizationInfo = userDetails.CardAuthorizationInfo
                };
                _rabbitManager.Publish(orderRequestDto, "OrderRequest", "direct", "GetOrderRequest");

                return orderRequestDto;
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
