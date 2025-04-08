using Coursework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Abstractions.Services
{
    public interface IUserService
    {
        Task<string> GenerateJwtToken(UserEntity user);
        Task<string> AuthenticateAsync(string email, string password);
        bool VerifyPassword(string password, string storedHash);
        Task<UserEntity> GetByIdAsync(int id);
        Task<IEnumerable<UserEntity>> GetAllAsync();
        Task<UserEntity> GetByEmailAsync(string email);
        Task<IEnumerable<UserEntity>> GetByRoleAsync(string role);
        Task AddAsync(UserEntity user);
        Task UpdateAsync(UserEntity user);
        Task DeleteAsync(int id);
    }
}
