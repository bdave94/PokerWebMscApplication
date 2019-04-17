using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PokerWebApplication.Game
{
    public class Player
    {
        public string Name { get; set; }
        public int TablePosition { get; set; }
        public int Chips { get; set; }

        public List<Card> Hand { get; set; }

        public List<Card> PublicHand { get; set; }

        public int CallValue  { get; set; }

        public string Action { get; set; }

        public Player()
        {
            Hand = new List<Card>();

            PublicHand = new List<Card>();
            PublicHand.Add(new Card("back", 0));
            PublicHand.Add(new Card("back", 0));


            CallValue = -1;

            Dealer = false;
            BigBlind = false;
            SmallBlind = false;
            PlayersTurn = false;
            Action = "";
        }

        public bool Dealer { get; set; }
        public bool BigBlind { get; set; }
        public bool SmallBlind { get; set; }
        public bool PlayersTurn { get; set; }
        public string ConnectionId { get; internal set; }

        

        


       
    }
}
