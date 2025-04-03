using Coursework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Abstractions.Repositories
{
    public interface IOrderServiceRepository
    {
        Task<IEnumerable<ServiceEntity>> GetServicesByOrderIdAsync(int orderId);
        Task<IEnumerable<OrderEntity>> GetOrdersByServiceIdAsync(int serviceId);
        Task<int> GetTotalSalesByServiceAsync(int serviceId);
        Task<decimal> GetTotalRevenueByServiceAsync(int serviceId);
        Task AddServiceToOrderAsync(int orderId, int serviceId, decimal priceAtPurchase);
        Task RemoveServiceFromOrderAsync(int orderId, int serviceId);
    }
}
