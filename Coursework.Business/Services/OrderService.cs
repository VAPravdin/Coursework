using Coursework.Abstractions.Repositories;
using Coursework.Abstractions.Services;
using Coursework.Entities;
using System;
using System.Collections.Generic;
using FluentValidation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coursework.Business.Validators;

namespace Coursework.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<OrderEntity>> GetAllOrdersAsync()
        {
            return await _unitOfWork.OrderRepository.GetAllAsync();
        }

        public async Task<OrderEntity> GetOrderByIdAsync(int id)
        {
            return await _unitOfWork.OrderRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<OrderEntity>> GetOrdersByUserIdAsync(int userId)
        {
            return await _unitOfWork.OrderRepository.GetByUserIdAsync(userId);
        }

        public async Task CreateOrderAsync(OrderEntity order)
        {
            ValidateOrder(order);
            await _unitOfWork.OrderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(OrderEntity order)
        {
            ValidateOrder(order);
            await _unitOfWork.OrderRepository.UpdateAsync(order);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(int id)
        {
            await _unitOfWork.OrderRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
        private void ValidateOrder(OrderEntity order)
        {
            var validator = new OrderValidator();
            var result = validator.Validate(order);

            if (!result.IsValid)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errors);
            }
        }
    }

}
