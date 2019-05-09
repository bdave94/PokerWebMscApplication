using PokerWebApplication.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerWebApplication.Model
{
    public class TableInfo
    {
        public List<Card> TableHand { get; set; }
        public List<bool> TableHandHighlight { get; set; }
        public int Pot { get; set; }
        public string VictoryText { get; set; }

    }
}
