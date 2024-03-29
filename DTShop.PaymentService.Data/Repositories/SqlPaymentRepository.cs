﻿using DTShop.PaymentService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<Payment> AddPaymentAsync(Payment payment)
        {
            await _paymentDbContext.AddAsync(payment);
            if (!await SaveChangesAsync())
            {
                throw new DbUpdateException("Database failure");
            }
            return payment;
        }

        public IEnumerable<Payment> GetPaymentsByUsername(string username)
        {
            var payments = _paymentDbContext.Payments.Where(p => p.Username == username);
            return payments;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _paymentDbContext.SaveChangesAsync()) > 0;
        }
    }
}
