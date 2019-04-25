using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerWebApplication.Game
{
    public class PokerHand
    {
        public bool StraightFlush { get; set; }
        public int StraightFlushBigest { get; set; }

        public bool Poker { get; set; }
        public int PokerStrength { get; set; }
        public int PokerHighestCard { get; set; }

        public bool FullHouse { get; set; }
        public int FullHouseThreeStr { get; set; }
        public int FullHouseTwoStr { get; set; }

        public bool Flush { get; set; }
        public int FlushHighest { get; set; }

        public bool Straight { get; set; }
        public int StraightHighest { get; set; }

        public bool ThreeOfaKind { get; set; }
        public int ThreeOfaKindHighest { get; set; }
        public int ThreeOfaKindHighestCard { get; set; }
        public int ThreeOfaKindSecondHighestCard { get; set; }

        public bool TwoPair { get; set; }
        public int PairOneStr { get; set; }
        public int PairTwoStr { get; set; }
        public int TwoPairHighestCard { get; set; }

        public bool Pair { get; set; }
        public int PairStr { get; set; }
        public int PairHighestCard { get; set; }
        public int PairHighestSecondCard { get; set; }

        public int HighcardFirst { get; set; }
        public int HighcardSecond { get; set; }
        public int HighcardThird { get; set; }
        public int HighcardFourth { get; set; }
        public int HighcardFifth { get; set; }

        public int StraightLength { get; set; }
        public int FlushLength { get; set; }

        public PokerHand()
        {
            StraightFlush = false;
            Poker = false;
            FullHouse = false;
            Flush = false;
            Straight = false;
            ThreeOfaKind = false;
            TwoPair = false;
            Pair = false;



        }
    }
}
