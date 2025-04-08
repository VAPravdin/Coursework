using Coursework.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Business.Validators
{
    public class OrderServiceValidator : AbstractValidator<OrderServiceEntity>
    {
        public OrderServiceValidator()
        {
            RuleFor(os => os.OrderId)
                .GreaterThan(0).WithMessage("OrderId must be a positive number.");

            RuleFor(os => os.ServiceId)
                .GreaterThan(0).WithMessage("ServiceId must be a positive number.");

            RuleFor(os => os.PriceAtPurchase)
                .GreaterThanOrEqualTo(0).WithMessage("Price at purchase must be non-negative.");
        }
    }
}
