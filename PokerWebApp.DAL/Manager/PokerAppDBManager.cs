using Microsoft.EntityFrameworkCore;
using PokerWebApp.DAL.Data;
using PokerWebApp.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokerWebApp.DAL.Manager
{
    public class PokerAppDBManager
    {


        public async System.Threading.Tasks.Task InsertNewUserAsync(PokerAppUser  user)
        {
            DbContextOptionsBuilder<PokerWebAppDALContext> builder = new DbContextOptionsBuilder<PokerWebAppDALContext>();

            PokerWebAppDALContext appDbContext = new PokerWebAppDALContext(builder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=PokerWebAppDAL;Trusted_Connection=True;MultipleActiveResultSets=true").Options);


            Statistics s = new Statistics { PokerAppUserId = user.Id, TotalGamesPlayed = 0, TotalWins = 0 };
           
            await appDbContext.Statistics.AddAsync(s);
            await appDbContext.SaveChangesAsync();
      
        }


        public async System.Threading.Tasks.Task<Statistics> GetStatisticsAsync(PokerAppUser user)
        {
            DbContextOptionsBuilder<PokerWebAppDALContext> builder = new DbContextOptionsBuilder<PokerWebAppDALContext>();


            PokerWebAppDALContext appDbContext = new PokerWebAppDALContext(builder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=PokerWebAppDAL;Trusted_Connection=True;MultipleActiveResultSets=true").Options);

            return await appDbContext.Statistics.FirstOrDefaultAsync(s => s.PokerAppUserId.Equals(user.Id) );
        }

    }
}
