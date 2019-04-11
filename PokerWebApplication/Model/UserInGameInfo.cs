using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerWebApplication.Model
{
    public class UserInGameInfo
    {
        public string ConnectionID { get; set; }
        public int TableId { get; set; }
        public int TablePosition { get; set; }
 
    }
}
