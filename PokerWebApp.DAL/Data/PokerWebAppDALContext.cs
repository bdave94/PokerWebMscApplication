using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PokerWebApp.DAL.Models;

namespace PokerWebApp.DAL.Data
{
    public class PokerWebAppDALContext : IdentityDbContext<PokerAppUser>
    {
        

        public PokerWebAppDALContext(DbContextOptions<PokerWebAppDALContext> options )
            : base(options)
        {
        }


        
        public DbSet<Statistics> Statistics { get; set; }

    }
}
