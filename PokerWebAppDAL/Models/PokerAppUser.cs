using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PokerWebAppDAL.Models
{
    // Add profile data for application users by adding properties to the PokerAppUser class
    public class PokerAppUser : IdentityUser
    {
        public string ProfilePicturePath { get; set; }
        public DateTime AccountCreated { get; set; }
        public int Money { get; set; }
        public int WinsCurrentWeek { get; set; }

        public virtual Statistics statistics { get; set; }
    }

    public class Statistics
    {
        [Key]
        public Guid StatisticsID { get; set; }

        public int TotalGamesPlayed { get; set; }
        public int TotalWins { get; set; }
        public int TotalMoneyEarned { get; set; }
        public int BestWeeklyRank { get; set; }
        public DateTime BestWeeklyRankDate { get; set; }
        
        
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual PokerAppUser User { get; set; }

    }
}
