namespace DTShop.PaymentService.RabbitMQ.Dtos
{
    public class PayForOrderDto
    {
        public int OrderId { get; set; }
        public long PaymentId { get; set; }
        public string Status { get; set; }
    }
}
