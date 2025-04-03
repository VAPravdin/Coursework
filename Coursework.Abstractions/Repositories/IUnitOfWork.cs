using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Abstractions.Repositories
{
    public interface IUnitOfWork
    {
        IOrderRepository OrderRepository { get; }
        IOrderServiceRepository OrderServiceRepository { get; }
        IServiceRepository ServiceRepository { get; }
        IUserRepository UserRepository { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
