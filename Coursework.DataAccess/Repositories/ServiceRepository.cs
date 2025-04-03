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
    public class ServiceRepository : IServiceRepository
    {
        private readonly CourseworkDbContext _context;
        public ServiceRepository(CourseworkDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ServiceEntity>> GetAllAsync()
        {
            return await _context.Services.ToListAsync();
        }
        public async Task<ServiceEntity> GetByIdAsync(int serviceId)
        {
            return await _context.Services.FindAsync(serviceId);
        }
        public async Task<IEnumerable<ServiceEntity>> GetByUserIdAsync(int userId)
        {
            return await _context.Services
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }
        public async Task<IEnumerable<ServiceEntity>> SearchAsync(string query)
        {
            return await _context.Services
                .Where(s => s.Name.Contains(query) || s.Description.Contains(query))
                .ToListAsync();
        }
        public async Task<IEnumerable<ServiceEntity>> GetPopularServicesAsync(int count)
        {
            return await _context.Services
                .OrderByDescending(s => s.OrderServices.Count) 
                .Take(count)
                .ToListAsync();
        }
        public async Task AddAsync(ServiceEntity service)
        {
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ServiceEntity service)
        {
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int serviceId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }
    }
}
