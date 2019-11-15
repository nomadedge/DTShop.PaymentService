using DTShop.PaymentService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DTShop.PaymentService.Data.Repositories
{
    public class SqlPaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _paymentDbContext;

        public SqlPaymentRepository(PaymentDbContext paymentDbContext)
        {
            _paymentDbContext = paymentDbContext;
        }

        public async Task<Payment> AddPayment(Payment payment)
        {
            _paymentDbContext.Add(payment);
            if (!await SaveChangesAsync())
            {
                throw new DbUpdateException("Database failure");
            }
            return payment;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _paymentDbContext.SaveChangesAsync()) > 0;
        }
    }
}
