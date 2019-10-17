namespace DTShop.PaymentService.Core.Models
{
    public class OrderItemModel
    {
        public ItemModel Item { get; set; }
        public int Amount { get; set; }
    }
}
