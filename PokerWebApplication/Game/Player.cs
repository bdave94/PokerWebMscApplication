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

        public int CallValue  { get; set; }

        public Player()
        {
            Hand = new List<Card>();
            CallValue = -1;

            Dealer = false;
            BigBlind = false;
            SmallBlind = false;
            PlayersTurn = false;
        }

        public bool Dealer { get; set; }
        public bool BigBlind { get; set; }
        public bool SmallBlind { get; set; }
        public bool PlayersTurn { get; set; }
        public string ConnectionId { get; internal set; }

        public void ClearHand()
        {
            Hand.Clear();
        }

        public void AddCardHand(Card c)
        {
            Hand.Add(c);
        }


        public bool Call()
        {
            bool decision = true;
            while (CallValue == -1)
            {
               
            }

            if (CallValue == 0)
                decision = false;

            if (CallValue == 1)
                decision = true;

            CallValue = -1;
            return decision;

        }
    }
}
