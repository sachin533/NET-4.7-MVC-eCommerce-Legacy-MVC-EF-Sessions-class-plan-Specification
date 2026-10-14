using System;
using System.Collections.Generic;

namespace Ecommerce.Core.Entities
{
    /// <summary>
    /// Customer profile. UserId links to ASP.NET Identity (AspNetUsers.Id) but is stored as a
    /// plain scalar: the Identity tables are managed by the code-first Identity context, not the EDMX.
    /// </summary>
    public class Customer
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedUtc { get; set; }

        public virtual ICollection<Address> Addresses { get; set; }
        public virtual ICollection<Order> Orders { get; set; }

        public Customer()
        {
            Addresses = new List<Address>();
            Orders = new List<Order>();
        }
    }
}
