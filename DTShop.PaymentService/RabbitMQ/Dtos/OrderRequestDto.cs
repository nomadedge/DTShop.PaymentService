namespace DTShop.PaymentService.RabbitMQ.Dtos
{
    public class OrderRequestDto
    {
        public int OrderId { get; set; }
        public string Username { get; set; }
        public string CardAuthorizationInfo { get; set; }
    }
}
