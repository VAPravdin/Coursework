using Coursework.Abstractions.Repositories;
using Coursework.Abstractions.Services;
using Coursework.Business.Validators;
using Coursework.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Business.Services
{
    public class OrderServiceService : IOrderServiceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderServiceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ServiceEntity>> GetServicesByOrderIdAsync(int orderId)
            => await _unitOfWork.OrderServiceRepository.GetServicesByOrderIdAsync(orderId);

        public async Task<IEnumerable<OrderEntity>> GetOrdersByServiceIdAsync(int serviceId)
            => await _unitOfWork.OrderServiceRepository.GetOrdersByServiceIdAsync(serviceId);

        public async Task<int> GetTotalSalesByServiceAsync(int serviceId)
            => await _unitOfWork.OrderServiceRepository.GetTotalSalesByServiceAsync(serviceId);

        public async Task<decimal> GetTotalRevenueByServiceAsync(int serviceId)
            => await _unitOfWork.OrderServiceRepository.GetTotalRevenueByServiceAsync(serviceId);

        public async Task AddServiceToOrderAsync(int orderId, int serviceId, decimal priceAtPurchase)
        {
            var orderServiceEntity = new OrderServiceEntity
            {
                OrderId = orderId,
                ServiceId = serviceId,
                PriceAtPurchase = priceAtPurchase
            };
            ValidateOrderService(orderServiceEntity);
            await _unitOfWork.OrderServiceRepository.AddServiceToOrderAsync(orderId, serviceId, priceAtPurchase);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveServiceFromOrderAsync(int orderId, int serviceId)
        {
            await _unitOfWork.OrderServiceRepository.RemoveServiceFromOrderAsync(orderId, serviceId);
            await _unitOfWork.SaveChangesAsync();
        }
        private void ValidateOrderService(OrderServiceEntity orderServiceEntity)
        {
            var validator = new OrderServiceValidator();
            var result = validator.Validate(orderServiceEntity);

            if (!result.IsValid)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errors);
            }
        }
    }
}
