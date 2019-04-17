using PokerWebApplication.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerWebApplication.Model
{
    public class PlayerInfo
    {
        public string Name { get; set; }
        public int TablePosition { get; set; }
        public int Chips { get; set; }

        public bool Dealer { get; set; }
        public bool BigBlind { get; set; }
        public bool SmallBlind { get; set; }
        public bool PlayersTurn { get; set; }

        public Card CardOne { get; set; }
      
        public Card CardTwo { get; set; }

        public string Action { get; set; }


    }
}
