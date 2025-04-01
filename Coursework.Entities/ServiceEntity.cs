using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Entities
{
    public class ServiceEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal DefaultPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public int UserId { get; set; }
        public UserEntity User { get; set; }
        public ICollection<OrderServiceEntity>? OrderServices { get; set; }
    }
}
