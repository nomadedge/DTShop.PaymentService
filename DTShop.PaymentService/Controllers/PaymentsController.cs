using DTShop.PaymentService.Core.Enums;
using DTShop.PaymentService.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DTShop.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        [HttpPut("{orderId}")]
        public async Task<ActionResult<OrderModel>> PayForOrder(int orderId, UserDetailsModel userDetails)
        {
            //Should get an order from OrderService
            var order = new OrderModel
            {
                OrderId = orderId,
                Username = "misha",
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

            int paymentId = 0;
            //This happens on external resource
            switch (userDetails.CardAuthorizationInfo)
            {
                case CardAuthorizationInfo.Authorized:
                    //Perform payment
                    paymentId = 1;
                    break;
                case CardAuthorizationInfo.Unauthorized:
                    //Authorize card and perform payment
                    paymentId = 1;
                    break;
            }

            //Call OrderService's method and pass orderId and paymentId to it
            //This method will return OrderModel instance with actual data
            var orderAfterPayment = order;
            orderAfterPayment.PaymentId = paymentId;
            orderAfterPayment.Status = "Paid";

            return orderAfterPayment;
        }
    }
}
