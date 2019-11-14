using System.Collections.Generic;
using System.Linq;

namespace DTShop.PaymentService.Core.Models
{
    public class OrderModel
    {
        public int OrderId { get; set; }
        public string Username { get; set; }
        public long? PaymentId { get; set; }
        public string Status { get; set; }
        public decimal TotalCost => OrderItems.Sum(i => i.Amount * i.Item.Price);
        public int TotalAmount => OrderItems.Sum(i => i.Amount);
        public ICollection<OrderItemModel> OrderItems { get; set; }
    }
}
