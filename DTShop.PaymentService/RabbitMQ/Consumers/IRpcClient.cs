namespace DTShop.PaymentService.RabbitMQ.Consumers
{
    public interface IRpcClient
    {
        string Call<T>(T message) where T : class;
        void Close();
    }
}