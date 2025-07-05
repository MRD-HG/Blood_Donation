using Blood_Donation.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Blood_Donation.Data
{
    public class DonorContext : DbContext
    {
        public DonorContext(DbContextOptions<DonorContext> options) : base(options) { }

        public DbSet<Donor> Donors { get; set; }
    }
}
