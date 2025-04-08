using Coursework.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Business.Validators
{
    public class ServiceValidator : AbstractValidator<ServiceEntity>
    {
        public ServiceValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

            RuleFor(s => s.Description)
                .MaximumLength(1000).WithMessage("Description must be at most 1000 characters.");

            RuleFor(s => s.DefaultPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Default price must be non-negative.");

            RuleFor(s => s.DiscountPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Discount price must be non-negative.")
                .LessThanOrEqualTo(s => s.DefaultPrice)
                .WithMessage("Discount price must be less than or equal to default price.");

            RuleFor(s => s.UserId)
                .GreaterThan(0).WithMessage("UserId must be a positive number.");
        }
    }
}
