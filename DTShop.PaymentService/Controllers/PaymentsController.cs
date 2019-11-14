using DTShop.PaymentService.Core.Enums;
using DTShop.PaymentService.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace DTShop.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        [HttpPut("{orderId}")]
        public ActionResult<OrderModel> PayForOrder(int orderId, UserDetailsModel userDetails)
        {
            CardAuthorizationInfo cardAuthorizationInfo;
            switch (userDetails.CardAuthorizationInfo.ToLower())
            {
                case "authorized":
                    cardAuthorizationInfo = CardAuthorizationInfo.Authorized;
                    break;
                case "unauthorized":
                    cardAuthorizationInfo = CardAuthorizationInfo.Unauthorized;
                    break;
                default:
                    return BadRequest("CardAuthorizationInfo is not valid.");
            }

            //Should get an order from OrderService
            var order = new OrderModel
            {
                OrderId = orderId,
                Username = "Misha",
                Status = "Collecting",
                OrderItems = new List<OrderItemModel> { new OrderItemModel
                {
                    Item = new ItemModel
                    {
                        ItemId = 1,
                        Name = "Bioshock Infinite",
                        Price = 10m
                    },
                    Amount = 3
                } }
            };

            if (order.Username != userDetails.Username)
            {
                return BadRequest("Usernames in order and user details should be equal.");
            }

            if (order.Status.ToLower() != "collecting")
            {
                return BadRequest("Order status should be \"Collecting\".");
            }

            var orderAfterPayment = order;
            switch (cardAuthorizationInfo)
            {
                case CardAuthorizationInfo.Authorized:
                    orderAfterPayment.PaymentId = DateTime.Now.Ticks;
                    orderAfterPayment.Status = "Paid";
                    break;
                case CardAuthorizationInfo.Unauthorized:
                    orderAfterPayment.PaymentId = 0;
                    orderAfterPayment.Status = "Failed";
                    break;
            }

            //Call OrderService's method and pass orderId and paymentId to it
            //This method will return OrderModel instance with actual data

            return orderAfterPayment;
        }
    }
}
