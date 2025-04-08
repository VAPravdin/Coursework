using Coursework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Abstractions.Services
{
    public interface IServiceService
    {
        Task<IEnumerable<ServiceEntity>> GetAllAsync();
        Task<ServiceEntity> GetByIdAsync(int serviceId);
        Task<IEnumerable<ServiceEntity>> GetByUserIdAsync(int userId);
        Task<IEnumerable<ServiceEntity>> SearchAsync(string query);
        Task<IEnumerable<ServiceEntity>> GetPopularServicesAsync(int count);
        Task AddAsync(ServiceEntity service);
        Task UpdateAsync(ServiceEntity service);
        Task DeleteAsync(int serviceId);
    }
}
