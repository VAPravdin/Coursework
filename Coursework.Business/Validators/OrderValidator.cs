using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coursework.Entities;
using FluentValidation;

namespace Coursework.Business.Validators
{
    public class OrderValidator : AbstractValidator<OrderEntity>
    {
        public OrderValidator()
        {
            RuleFor(o => o.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative.");

            RuleFor(o => o.OrderDate)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Order date cannot be in the future.");

            RuleFor(o => o.UserId)
                .GreaterThan(0).WithMessage("UserId must be a positive number.");
        }
    }
}
