using DTShop.PaymentService.Core.Models;

namespace DTShop.PaymentService.RabbitMQ.Consumers
{
    public interface IRpcClient
    {
        void Open();
        OrderModel Call<T>(T message) where T : class;
        void Close();
    }
}