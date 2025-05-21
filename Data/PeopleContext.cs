using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DOTNETWorkspace.Models;

namespace DOTNETWorkspace.Data
{
    public class PeopleContext : DbContext
    {
        public PeopleContext (DbContextOptions<PeopleContext> options)
            : base(options)
        {
        }

        public DbSet<DOTNETWorkspace.Models.Person> Person { get; set; } = default!;
    }
}
