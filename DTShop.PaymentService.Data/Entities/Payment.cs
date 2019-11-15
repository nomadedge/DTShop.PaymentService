using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DTShop.PaymentService.Data.Entities
{
    public class Payment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PaymentId { get; set; }
        public int OrderId { get; set; }
        [Required]
        public string Username { get; set; }
        public decimal TotalCost { get; set; }
        public bool IsPassed { get; set; }
    }
}
