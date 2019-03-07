using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerWebApplication.Model
{
    public class ProfileInfo
    {
        public string UserName;
        public int Money { get; set; }
        public int GamesPlayed { get; set; }
        public int TotalWins { get; set; }

    }
}
