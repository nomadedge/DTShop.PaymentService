using AutoMapper;
using DTShop.PaymentService.Core.Models;
using DTShop.PaymentService.RabbitMQ.Dtos;

namespace DTShop.PaymentService.AutoMapper
{
    public class OrderModelToPayForOrderPaymentId : IValueResolver<OrderModel, PayForOrderDto, long>
    {
        public long Resolve(OrderModel source, PayForOrderDto destination, long destMember, ResolutionContext context)
        {
            return source.PaymentId.Value;
        }
    }
}
