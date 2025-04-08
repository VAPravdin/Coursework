using Coursework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Abstractions.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderEntity>> GetAllOrdersAsync();
        Task<OrderEntity> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderEntity>> GetOrdersByUserIdAsync(int userId);
        Task CreateOrderAsync(OrderEntity order);
        Task UpdateOrderAsync(OrderEntity order);
        Task DeleteOrderAsync(int id);
    }
}
