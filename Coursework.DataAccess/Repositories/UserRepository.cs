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
    public class UserRepository : IUserRepository
    {
        private readonly CourseworkDbContext _context;
        public UserRepository(CourseworkDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<UserEntity>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<UserEntity> GetByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }
        public async Task<UserEntity?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<UserEntity?> GetWithOrdersAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<UserEntity?> GetWithServicesAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Services)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<IEnumerable<UserEntity>> GetByRoleAsync(string role)
        {
            if (Enum.TryParse<UserRole>(role, out var userRole))
            {
                return await _context.Users
                    .Where(user => user.Role == userRole)
                    .ToListAsync();
            }
            else
            {
                return Enumerable.Empty<UserEntity>();
            }
        }
        public async Task AddAsync(UserEntity user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserEntity user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}
