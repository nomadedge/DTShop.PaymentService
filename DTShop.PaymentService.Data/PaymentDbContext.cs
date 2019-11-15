using DTShop.PaymentService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DTShop.PaymentService.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options) { }

        DbSet<Payment> Payments { get; set; }
    }
}
