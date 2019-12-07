using AutoMapper;
using DTShop.PaymentService.Core.Models;
using DTShop.PaymentService.RabbitMQ.Dtos;

namespace DTShop.PaymentService.AutoMapper
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {

            CreateMap<OrderModel, PayForOrderDto>()
                .ForMember(om => om.PaymentId, opt => opt.MapFrom<OrderModelToPayForOrderPaymentId>());
        }
    }
}
