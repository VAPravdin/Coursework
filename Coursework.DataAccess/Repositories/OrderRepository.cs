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
    public class OrderRepository : IOrderRepository
    {
        private readonly CourseworkDbContext _context;
        public OrderRepository(CourseworkDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<OrderEntity>> GetAllAsync()
        {
            return await _context.Orders.ToListAsync();
        }
        public async Task<OrderEntity> GetByIdAsync(int orderId)
        {
            return await _context.Orders.FindAsync(orderId);
        }
        public async Task<IEnumerable<OrderEntity>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Where(x => x.UserId == userId)
                .Include(x => x.OrderServices)
                .ThenInclude(x => x.Service).ToListAsync();
        }
        public async Task AddAsync(OrderEntity order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(OrderEntity order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }
    }
}
