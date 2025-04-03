using Coursework.Abstractions.Repositories;
using Coursework.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CourseworkDbContext _context;
        public UnitOfWork(CourseworkDbContext context,
            IOrderRepository orderRepository,
            IOrderServiceRepository orderServiceRepository,
            IServiceRepository serviceRepository,
            IUserRepository userRepository)
        {
            _context = context;
            OrderRepository = orderRepository;
            OrderServiceRepository = orderServiceRepository;
            ServiceRepository = serviceRepository;
            UserRepository = userRepository;
        }
        public IOrderRepository OrderRepository { get; }
        public IOrderServiceRepository OrderServiceRepository { get; }
        public IServiceRepository ServiceRepository { get; }
        public IUserRepository UserRepository { get; }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task BeginTransactionAsync()
        {
            await _context.Database.BeginTransactionAsync();
        }
        public async Task CommitAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }
        public async Task RollbackAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
