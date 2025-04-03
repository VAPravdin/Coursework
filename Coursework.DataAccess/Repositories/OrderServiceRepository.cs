using Coursework.Abstractions.Repositories;
using Coursework.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.DataAccess.Repositories
{
    public class OrderServiceRepository : IOrderServiceRepository
    {
        private readonly CourseworkDbContext _context;
        public OrderServiceRepository(CourseworkDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ServiceEntity>> GetServicesByOrderIdAsync(int orderId)
        {
            return await _context.OrderServices
                .Where(os => os.OrderId == orderId)
                .Select(os => os.Service)
                .ToListAsync();
        }
        public async Task<IEnumerable<OrderEntity>> GetOrdersByServiceIdAsync(int serviceId)
        {
            return await _context.OrderServices
                .Where(os => os.ServiceId == serviceId)
                .Select(os => os.Order)
                .ToListAsync();
        }
        public async Task<int> GetTotalSalesByServiceAsync(int serviceId)
        {
            return await _context.OrderServices
                .CountAsync(os => os.ServiceId == serviceId);
        }
        public async Task<decimal> GetTotalRevenueByServiceAsync(int serviceId)
        {
            return await _context.OrderServices
                .Where(os => os.ServiceId == serviceId)
                .SumAsync(os => os.PriceAtPurchase);
        }
        public async Task AddServiceToOrderAsync(int orderId, int serviceId, decimal priceAtPurchase)
        {
            var orderService = new OrderServiceEntity
            {
                OrderId = orderId,
                ServiceId = serviceId,
                PriceAtPurchase = priceAtPurchase
            };

            _context.OrderServices.Add(orderService);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveServiceFromOrderAsync(int orderId, int serviceId)
        {
            var orderService = await _context.OrderServices
                .FirstOrDefaultAsync(os => os.OrderId == orderId && os.ServiceId == serviceId);

            if (orderService != null)
            {
                _context.OrderServices.Remove(orderService);
                await _context.SaveChangesAsync();
            }
        }
    }
}
