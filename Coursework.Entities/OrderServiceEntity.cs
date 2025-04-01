using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Entities
{
    public class OrderServiceEntity
    {
        public int OrderId { get; set; }
        public OrderEntity Order { get; set; }
        public int ServiceId { get; set; }
        public ServiceEntity Service { get; set; }
        public decimal PriceAtPurchase { get; set; }
    }
}
