using Coursework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Abstractions.Repositories
{
    public interface IServiceRepository
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
