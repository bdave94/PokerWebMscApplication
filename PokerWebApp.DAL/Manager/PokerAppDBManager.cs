using Microsoft.EntityFrameworkCore;
using PokerWebApp.DAL.Data;
using PokerWebApp.DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PokerWebApp.DAL.Manager
{
    public class PokerAppDBManager
    {


        public async Task InsertNewUserAsync(PokerAppUser  user)
        {
            DbContextOptionsBuilder<PokerWebAppDALContext> builder = new DbContextOptionsBuilder<PokerWebAppDALContext>();

            PokerWebAppDALContext appDbContext = new PokerWebAppDALContext(builder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=PokerWebAppDAL;Trusted_Connection=True;MultipleActiveResultSets=true").Options);


            Statistics s = new Statistics { PokerAppUserId = user.Id, TotalGamesPlayed = 0, TotalWins = 0 };
           
            await appDbContext.Statistics.AddAsync(s);
            await appDbContext.SaveChangesAsync();
      
        }


        public async Task<Statistics> GetStatisticsAsync(PokerAppUser user)
        {
            DbContextOptionsBuilder<PokerWebAppDALContext> builder = new DbContextOptionsBuilder<PokerWebAppDALContext>();


            PokerWebAppDALContext appDbContext = new PokerWebAppDALContext(builder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=PokerWebAppDAL;Trusted_Connection=True;MultipleActiveResultSets=true").Options);

            return await appDbContext.Statistics.FirstOrDefaultAsync(s => s.PokerAppUserId.Equals(user.Id) );
        }


        public async Task IncrementNumberOfWinsAsync(string userName)
        {
            DbContextOptionsBuilder<PokerWebAppDALContext> builder = new DbContextOptionsBuilder<PokerWebAppDALContext>();


            PokerWebAppDALContext appDbContext = new PokerWebAppDALContext(builder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=PokerWebAppDAL;Trusted_Connection=True;MultipleActiveResultSets=true").Options);
            PokerAppUser pokerappuser = await appDbContext.Users.FirstOrDefaultAsync(user => user.UserName.Equals(userName));
            Statistics st = await appDbContext.Statistics.FirstOrDefaultAsync(s => s.PokerAppUserId.Equals(pokerappuser.Id));

            int numberOfWins = st.TotalWins;
            numberOfWins = numberOfWins + 1;

            st.TotalWins = numberOfWins;
            
            appDbContext.Statistics.Update(st);
            await appDbContext.SaveChangesAsync();
        }


        public async Task IncrementNumberOfGamesAsync(string userName)
        {
            DbContextOptionsBuilder<PokerWebAppDALContext> builder = new DbContextOptionsBuilder<PokerWebAppDALContext>();


            PokerWebAppDALContext appDbContext = new PokerWebAppDALContext(builder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=PokerWebAppDAL;Trusted_Connection=True;MultipleActiveResultSets=true").Options);
            PokerAppUser pokerappuser = await appDbContext.Users.FirstOrDefaultAsync(user => user.UserName.Equals(userName));
            Statistics st = await appDbContext.Statistics.FirstOrDefaultAsync(s => s.PokerAppUserId.Equals(pokerappuser.Id));

            int numberOfGames = st.TotalGamesPlayed;

            numberOfGames = numberOfGames + 1;

            st.TotalGamesPlayed = numberOfGames;

            appDbContext.Statistics.Update(st);
            await appDbContext.SaveChangesAsync();
        }

        public async Task AddMoneyAsync(string userName, int value)
        {
            DbContextOptionsBuilder<PokerWebAppDALContext> builder = new DbContextOptionsBuilder<PokerWebAppDALContext>();


            PokerWebAppDALContext appDbContext = new PokerWebAppDALContext(builder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=PokerWebAppDAL;Trusted_Connection=True;MultipleActiveResultSets=true").Options);
            PokerAppUser pokerappuser = await appDbContext.Users.FirstOrDefaultAsync(user => user.UserName.Equals(userName));


            pokerappuser.Money += value;



            appDbContext.Users.Update(pokerappuser);
            await appDbContext.SaveChangesAsync();
        }

    }
}
