using DTShop.PaymentService.Core.Models;

namespace DTShop.PaymentService.RabbitMQ.Dtos
{
    public class OrderResponseDto
    {
        public OrderModel Order { get; set; }
        public string CardAuthorizationInfo { get; set; }
    }
}
