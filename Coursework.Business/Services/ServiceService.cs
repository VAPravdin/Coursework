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
    public class ServiceService : IServiceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ServiceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ServiceEntity>> GetAllAsync()
            => await _unitOfWork.ServiceRepository.GetAllAsync();

        public async Task<ServiceEntity> GetByIdAsync(int serviceId)
            => await _unitOfWork.ServiceRepository.GetByIdAsync(serviceId);

        public async Task<IEnumerable<ServiceEntity>> GetByUserIdAsync(int userId)
            => await _unitOfWork.ServiceRepository.GetByUserIdAsync(userId);

        public async Task<IEnumerable<ServiceEntity>> SearchAsync(string query)
            => await _unitOfWork.ServiceRepository.SearchAsync(query);

        public async Task<IEnumerable<ServiceEntity>> GetPopularServicesAsync(int count)
            => await _unitOfWork.ServiceRepository.GetPopularServicesAsync(count);

        public async Task AddAsync(ServiceEntity service)
        {
            ValidateService(service);
            await _unitOfWork.ServiceRepository.AddAsync(service);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(ServiceEntity service)
        {
            ValidateService(service);
            await _unitOfWork.ServiceRepository.UpdateAsync(service);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int serviceId)
        {
            await _unitOfWork.ServiceRepository.DeleteAsync(serviceId);
            await _unitOfWork.SaveChangesAsync();
        }
        private void ValidateService(ServiceEntity service)
        {
            var validator = new ServiceValidator();
            var result = validator.Validate(service);

            if (!result.IsValid)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException(errors);
            }
        }
    }
}
