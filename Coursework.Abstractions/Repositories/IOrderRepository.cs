using Coursework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Abstractions.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderEntity>> GetAllAsync();
        Task<OrderEntity> GetByIdAsync(int orderId);
        Task<IEnumerable<OrderEntity>> GetByUserIdAsync(int userId);
        Task AddAsync(OrderEntity order);
        Task UpdateAsync(OrderEntity order);
        Task DeleteAsync(int orderId);
    }
}
