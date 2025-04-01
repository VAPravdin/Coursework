using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Entities
{
    public class OrderEntity
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public DateTime OrderDate { get; set; }
        public int UserId { get; set; }
        public UserEntity User { get; set; }
        public ICollection<OrderServiceEntity>? OrderServices { get; set; }
    }
}
