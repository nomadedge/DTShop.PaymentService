using DTShop.PaymentService.Data.Entities;
using System.Threading.Tasks;

namespace DTShop.PaymentService.Data.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> AddPayment(Payment payment);
        Task<bool> SaveChangesAsync();
    }
}