using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PokerWebApp.DAL.Models
{
    // Add profile data for application users by adding properties to the PokerAppUser class
    public class PokerAppUser : IdentityUser
    {
        public string ProfilePicturePath { get; set; }
        public DateTime AccountCreated { get; set; }
        public int Money { get; set; }
        public int WinsCurrentWeek { get; set; }

        [ForeignKey("Statistics")]
        public int? StatisticsId { get; set; }

        public virtual Statistics Statistics { get; set; }
    }

    public class Statistics
    {
       
        public int StatisticsId { get; set; }

        public int TotalGamesPlayed { get; set; }
        public int TotalWins { get; set; }
        public int TotalMoneyEarned { get; set; }
        public int BestWeeklyRank { get; set; }
        public DateTime BestWeeklyRankDate { get; set; }

        [ForeignKey("PokerAppUser")]
        public string PokerAppUserId { get; set; }

        public virtual PokerAppUser PokerAppUser { get; set; }

    }
}
