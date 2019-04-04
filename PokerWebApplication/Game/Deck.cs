using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PokerWebApplication.Game
{
    public class Deck
    {
        List<Card> deck = new List<Card>();

        string[] cardSuit = { "hearts", "diamonds", "clubs", "spades" };
        int[] cardRank = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };

        public Deck()
        {
            FillDeck();

        }

        public Card DrawCard()
        {
            /*  Timer t = new Timer();
              t.Interval = 3000;
              t.Enabled = true;*/

            
            Random rnd = new Random();
            int cardNumber = rnd.Next(deck.Count);
            Card c = deck[cardNumber];
            deck.RemoveAt(cardNumber);
            return c;

        }

        public void ResetDeck()
        {
            deck.Clear();
            FillDeck();
        }

        public void FillDeck()
        {

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    deck.Add(new Card(cardSuit[i], cardRank[j]));
                }
            }
        }

    }
}
