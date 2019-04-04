using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerWebApplication.Game
{
    public class Card
    {
        public string Suit { get; set; }
        public int Rank { get; set; }

        public Card(string s, int r)
        {
            Suit = s;
            Rank = r;
        }

       
    }
}
