using Coursework.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Abstractions.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserEntity>> GetAllAsync();
        Task<UserEntity> GetByIdAsync(int userId);

        Task<UserEntity?> GetByUsernameAsync(string username);
        Task<UserEntity?> GetByEmailAsync(string email);
        Task<UserEntity?> GetWithOrdersAsync(int userId);
        Task<UserEntity?> GetWithServicesAsync(int userId);
        Task<IEnumerable<UserEntity>> GetByRoleAsync(string role);
        Task AddAsync(UserEntity user);
        Task UpdateAsync(UserEntity user);
        Task DeleteAsync(int id);
    }

}
