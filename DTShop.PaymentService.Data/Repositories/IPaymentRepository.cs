using DTShop.PaymentService.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DTShop.PaymentService.Data.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> AddPaymentAsync(Payment payment);
        Task<bool> SaveChangesAsync();
        IEnumerable<Payment> GetPaymentsByUsername(string username);
    }
}