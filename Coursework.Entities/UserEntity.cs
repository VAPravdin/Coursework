using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coursework.Entities
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public string Email { get; set; }
        public ICollection<ServiceEntity>? Services { get; set; }
        public ICollection<OrderEntity>? Orders { get; set; }
    }
    public enum UserRole
    {
        Customer,
        Seller,
        Admin
    }
}
