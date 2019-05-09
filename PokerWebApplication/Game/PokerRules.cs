using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerWebApplication.Game
{
    public class PokerRules : IComparer<Player>
    {


        public static PokerHand SetPokerHand(List<Card> playerHand, List<Card> gameHand) 
        {
            int gameHandSize = gameHand.Count;
            PokerHand ph = new PokerHand();

            List<Card> allHand = new List<Card>();
            for (int i = 0; i < gameHand.Count; i++)
            {
                allHand.Add(gameHand[i]);

            }

           
            allHand.Add(playerHand[0]);
            allHand.Add(playerHand[1]);
            
            
            List<Card> sortedHand = allHand.OrderBy(o => o.Rank).ToList();

            //check for straightFlush / straight
            int firstStraightCardIndex = 0;
            int lastStraightCardIndex = 4;

            bool straightFound = false;
            bool newstraightFound = false;
            bool straightFlushFound = false;
            int straightHigh = 0;
            int straightFlushHigh = 0;
            int straightLength = 0;
            while (lastStraightCardIndex < sortedHand.Count)
            {
                //row can end with ace
                newstraightFound = true;
                int newstraightLength = 1;
                for (int n = firstStraightCardIndex; n < lastStraightCardIndex; n++)
                {
                    newstraightLength += 1;
                    if (sortedHand[n].Rank != sortedHand[n + 1].Rank - 1)
                    {
                        newstraightLength -= 1;
                        newstraightFound = false;
                        if (newstraightLength > straightLength)
                            straightLength = newstraightLength;

                        newstraightLength = 1;
                    }

                }

                if (newstraightLength > straightLength)
                    straightLength = newstraightLength;

                if (newstraightFound)
                {
                    straightFound = true;
                    straightHigh = sortedHand[lastStraightCardIndex].Rank;

                    bool newstraightFlushFound = true;

                    for (int i = firstStraightCardIndex; i < lastStraightCardIndex; i++)
                    {

                        if (sortedHand[i].Suit.Equals(sortedHand[i + 1].Suit) == false)
                        {
                            newstraightFlushFound = false;
                            i += 100;
                        }
                    }


                    if (newstraightFlushFound)
                    {
                        straightFlushFound = true;
                        straightFlushHigh = straightHigh;
                    }


                }

                //row can start with ace
                List<Card> sortedHandAceLow = new List<Card>();
                for (int n = 0; n < sortedHand.Count; n++)
                {
                    Card c = sortedHand[n];
                    sortedHandAceLow.Add(new Card(c.Suit, c.Rank));
                }

                foreach (Card c in sortedHandAceLow)
                {
                    if (c.Rank == 14)
                    {
                        c.Rank=1;
                    }

                }
                sortedHandAceLow = sortedHandAceLow.OrderBy(o => o.Rank).ToList();

                newstraightFound = true;
                newstraightLength = 1;
                for (int n = firstStraightCardIndex; n < lastStraightCardIndex; n++)
                {
                    newstraightLength += 1;

                    if (sortedHandAceLow[n].Rank != sortedHandAceLow[n + 1].Rank - 1)
                    {
                        newstraightLength -= 1;

                        newstraightFound = false;
                        if (newstraightLength > straightLength)
                            straightLength = newstraightLength;

                        newstraightLength = 1;
                    }

                }
                if (newstraightLength > straightLength)
                    straightLength = newstraightLength;



                if (newstraightFound)
                {
                    straightFound = true;
                    straightHigh = sortedHandAceLow[lastStraightCardIndex].Rank;

                    bool newstraightFlushFound = true;

                    for (int i = firstStraightCardIndex; i < lastStraightCardIndex; i++)
                    {

                        if (sortedHandAceLow[i].Suit.Equals(sortedHandAceLow[i + 1].Suit) == false)
                        {
                            newstraightFlushFound = false;
                            i += 100;
                        }

                    }


                    if (newstraightFlushFound)
                    {
                        straightFlushFound = true;
                        straightFlushHigh = straightHigh;
                    }

                }


                firstStraightCardIndex += 1;
                lastStraightCardIndex += 1;

            }

            ph.StraightLength = straightLength;

            if (straightFlushFound)
            {
                ph.StraightFlush = true;
                ph.StraightFlushBigest = straightFlushHigh;
            }

            if (straightFound && straightFlushFound == false)
            {
                ph.Straight = true;
                ph.StraightHighest = straightHigh;
            }

            //check for Poker
            bool pokerFound = false;
            int pokerstr = 0;
            int pokerhighcard = 0;

            int firstPokerCardIndex = 0;
            int lastPokerCardIndex = 3;

            while (lastPokerCardIndex < sortedHand.Count)
            {
                bool newpokerFound = true;
                for (int i = firstPokerCardIndex; i < lastPokerCardIndex; i++)
                {
                    if (sortedHand[i].Rank != sortedHand[i + 1].Rank)
                        newpokerFound = false;
                }

                if (newpokerFound)
                {
                    pokerFound = true;
                    pokerstr = sortedHand[firstPokerCardIndex].Rank;
                    pokerhighcard = sortedHand[sortedHand.Count - 1].Rank;
                }
                firstPokerCardIndex += 1;
                lastPokerCardIndex += 1;

            }
            if (pokerFound)
            {
                ph.Poker = true;
                ph.PokerStrength = pokerstr;
                ph.PokerHighestCard = pokerhighcard;
            }


            //check for Full House         
            int fullHousetwostr = 0;
            int fullHousethreestr = 0;

            int firstfullhIndex = 0;
            int lastfullhIndex = 2;

            //---search for three of akind
            while (lastfullhIndex < sortedHand.Count)
            {
                bool newthreeOfaKindFound = true;
                for (int i = firstfullhIndex; i < lastfullhIndex; i++)
                {
                    if (sortedHand[i].Rank != sortedHand[i + 1].Rank)
                        newthreeOfaKindFound = false;
                }

                if (newthreeOfaKindFound)
                {

                    if (sortedHand[firstfullhIndex].Rank > fullHousethreestr)
                    {
                        fullHousethreestr = sortedHand[firstfullhIndex].Rank;
                    }

                }
                firstfullhIndex += 1;
                lastfullhIndex += 1;
            }

            //---search for two of a kind
            firstfullhIndex = 0;
            lastfullhIndex = 1;
            while (lastfullhIndex < sortedHand.Count)
            {
                bool newpairFound = true;
                for (int i = firstfullhIndex; i < lastfullhIndex; i++)
                {
                    if (sortedHand[i].Rank != sortedHand[i + 1].Rank ||
                        sortedHand[i].Rank == fullHousethreestr)
                        newpairFound = false;
                }

                if (newpairFound)
                {

                    if (sortedHand[firstfullhIndex].Rank > fullHousetwostr)
                    {
                        fullHousetwostr = sortedHand[firstfullhIndex].Rank;
                    }

                }
                firstfullhIndex += 1;
                lastfullhIndex += 1;
            }

            if (fullHousethreestr != 0 && fullHousetwostr != 0)
            {
                ph.FullHouse = true;
                ph.FullHouseTwoStr = fullHousetwostr;
                ph.FullHouseThreeStr = fullHousethreestr;
            }


            //check for flush
            bool flushFound = false;
            int[] flushhighcards = new int[5];
            string suit = "";

            int flushLength = 0;


            List<Card> color_hearts = new List<Card>();
            List<Card> color_diamonds = new List<Card>();
            List<Card> color_clubs = new List<Card>();
            List<Card> color_spades = new List<Card>();
            for (int i = 0; i < sortedHand.Count; i++)
            {
                string cardType = sortedHand[i].Suit;
                if (cardType.Equals("hearts"))
                    color_hearts.Add(sortedHand[i]);

                if (cardType.Equals("diamonds"))
                    color_diamonds.Add(sortedHand[i]);

                if (cardType.Equals("clubs"))
                    color_clubs.Add(sortedHand[i]);

                if (cardType.Equals("spades"))
                    color_spades.Add(sortedHand[i]);
            }
            int newflushLength = 0;
            if (color_hearts.Count >= 5)
            {
                flushFound = true;
                suit = "hearts";
                for(int i= 0;i <5; i++)
                {
                    flushhighcards[i] = color_hearts[4-i].Rank;
                }
               
            }
            else
            {
                newflushLength = color_hearts.Count;
                if (newflushLength > flushLength)
                    flushLength = newflushLength;
            }


            if (color_diamonds.Count >= 5)
            {
                flushFound = true;
                suit = "diamonds";
                for (int i = 0; i < 5; i++)
                {
                    flushhighcards[i] = color_diamonds[4 - i].Rank;
                }
            }
            else
            {
                newflushLength = color_diamonds.Count;
                if (newflushLength > flushLength)
                    flushLength = newflushLength;
            }


            if (color_clubs.Count >= 5)
            {
                flushFound = true;
                suit = "clubs";
                for (int i = 0; i < 5; i++)
                {
                    flushhighcards[i] = color_clubs[4 - i].Rank;
                }
            }
            else
            {
                newflushLength = color_clubs.Count;
                if (newflushLength > flushLength)
                    flushLength = newflushLength;
            }


            if (color_spades.Count >= 5)
            {
                flushFound = true;
                suit = "spades";
                for (int i = 0; i < 5; i++)
                {
                    flushhighcards[i] = color_spades[4 - i].Rank;
                }
            }
            else
            {
                newflushLength = color_spades.Count;
                if (newflushLength > flushLength)
                    flushLength = newflushLength;
            }


            if (flushFound)
            {
                ph.Flush = true;
                ph.FlushSuit = suit;
                ph.FlushHighest = flushhighcards[0];
                ph.FlushSecondHighest = flushhighcards[1];
                ph.FlushThirdHighest = flushhighcards[2];
                ph.FlushFourthHighest = flushhighcards[3];
                ph.FlushFifthHighest = flushhighcards[4];
            }

            ph.FlushLength = flushLength;




            //check for three of a kind
            bool threeOfaKindfound = false;
            int threeOfaKindStr = 0;
            int threeOfaKindHigh = 0;
            int threeOfaKindSecondHigh = 0;

            int threeOfaKindfirstindex = 0;
            int threeOfaKindLastindex = 2;

            while (threeOfaKindLastindex < sortedHand.Count)
            {
                bool newthreeOfaKindFound = true;
                for (int i = threeOfaKindfirstindex; i < threeOfaKindLastindex; i++)
                {
                    if (sortedHand[i].Rank != sortedHand[i + 1].Rank)
                        newthreeOfaKindFound = false;
                }

                if (newthreeOfaKindFound)
                {
                    threeOfaKindfound = true;
                    if (sortedHand[threeOfaKindfirstindex].Rank > threeOfaKindStr)
                    {

                        threeOfaKindStr = sortedHand[threeOfaKindfirstindex].Rank;

                        if (threeOfaKindLastindex <= sortedHand.Count - 3)
                        {
                            
                            threeOfaKindHigh = sortedHand[sortedHand.Count - 1].Rank;

                           
                            threeOfaKindSecondHigh = sortedHand[sortedHand.Count - 2].Rank;

                        }

                        if (threeOfaKindLastindex == sortedHand.Count - 2)
                        {
                            
                            threeOfaKindHigh = sortedHand[sortedHand.Count - 1].Rank;

                           
                            threeOfaKindSecondHigh = sortedHand[sortedHand.Count - 5].Rank;

                        }

                        if (threeOfaKindLastindex == sortedHand.Count - 1)
                        {

                           
                            threeOfaKindHigh = sortedHand[sortedHand.Count - 4].Rank;

                           
                            threeOfaKindSecondHigh = sortedHand[sortedHand.Count - 5].Rank;

                        }

                    }

                }
                threeOfaKindfirstindex += 1;
                threeOfaKindLastindex += 1;
            }

            if (threeOfaKindfound)
            {
                ph.ThreeOfaKind = true;
                ph.ThreeOfaKindHighest = threeOfaKindStr;
                ph.ThreeOfaKindHighestCard = threeOfaKindHigh;
                ph.ThreeOfaKindSecondHighestCard = threeOfaKindSecondHigh;
            }


            //Check for two pairs
            bool twoPairfound = false;
            bool onePairfound = false;
            int pairOneStr = 0;
            int pairTwoStr = 0;
            int twoPairHigh = 0;

            int twopairfirstindex = 0;
            int twopairLastindex = 1;

            int secondpairLastIndex = -1;
            int firstPairLastIndex = -1;

            while (twopairLastindex < sortedHand.Count)
            {

                bool newpairFound = true;
                for (int i = twopairfirstindex; i < twopairLastindex; i++)
                {
                    if (sortedHand[i].Rank != sortedHand[i + 1].Rank)
                        newpairFound = false;
                }

                if (newpairFound)
                {
                    if (onePairfound == false)
                    {
                        pairOneStr = sortedHand[twopairLastindex].Rank;
                        onePairfound = true;
                        firstPairLastIndex = twopairLastindex;

                    }
                    else
                    {
                        pairTwoStr = pairOneStr;
                        secondpairLastIndex = firstPairLastIndex;
                        pairOneStr = sortedHand[twopairLastindex].Rank;
                        firstPairLastIndex = twopairLastindex;
                        twoPairfound = true;
                    }


                    if (twoPairfound)
                    {

                        if (firstPairLastIndex <= sortedHand.Count - 2)
                        {
                            twoPairHigh = sortedHand[sortedHand.Count - 1].Rank;
                        }

                        if (firstPairLastIndex == sortedHand.Count - 1 &&
                            secondpairLastIndex <= sortedHand.Count - 4)
                        {
                            twoPairHigh = sortedHand[sortedHand.Count - 3].Rank;


                        }

                        if (firstPairLastIndex == sortedHand.Count - 1 &&
                            secondpairLastIndex == sortedHand.Count - 3)
                        {
                           
                            twoPairHigh = sortedHand[sortedHand.Count - 5].Rank;

                        }

                    }

                }
                twopairfirstindex += 1;
                twopairLastindex += 1;
            }

            if (twoPairfound)
            {
                ph.TwoPair = true;
                ph.PairOneStr = pairOneStr;
                ph.PairTwoStr = pairTwoStr;
                ph.TwoPairHighestCard = twoPairHigh;

            }


            //check for one pair
            bool Pairfound = false;
            int pairStr = 0;
            int PairHigh = 0;
            int pairsecondheigh = 0;
            int pairthirdhigh = 0;

            int pairfirstIndex = 0;
            int PairLastIndex = 1;

            while (PairLastIndex < sortedHand.Count)
            {

                bool newpairFound = true;
                for (int i = pairfirstIndex; i < PairLastIndex; i++)
                {
                    if (sortedHand[i].Rank != sortedHand[i + 1].Rank)
                        newpairFound = false;
                }

                if (newpairFound)
                {

                    pairStr = sortedHand[PairLastIndex].Rank;
                    Pairfound = true;

                    if (PairLastIndex <= sortedHand.Count - 4)
                    {
                        PairHigh = sortedHand[sortedHand.Count - 1].Rank;
                        pairsecondheigh = sortedHand[sortedHand.Count - 2].Rank;
                        pairthirdhigh = sortedHand[sortedHand.Count - 3].Rank;
                    }


                    if (PairLastIndex == sortedHand.Count - 3)
                    {
                        PairHigh = sortedHand[sortedHand.Count - 1].Rank;
                        pairsecondheigh = sortedHand[sortedHand.Count - 2].Rank;
                        pairthirdhigh = sortedHand[sortedHand.Count - 5].Rank;

                    }

                    if (PairLastIndex == sortedHand.Count - 2)
                    {
                        PairHigh = sortedHand[sortedHand.Count - 1].Rank;

                        
                        pairsecondheigh = sortedHand[sortedHand.Count - 4].Rank;
                        pairthirdhigh = sortedHand[sortedHand.Count - 5].Rank;
                    }


                    if (PairLastIndex == sortedHand.Count - 1)
                    {
                        PairHigh = sortedHand[sortedHand.Count - 3].Rank;

                        pairsecondheigh = sortedHand[sortedHand.Count - 4].Rank;
                        pairthirdhigh = sortedHand[sortedHand.Count - 5].Rank;

                    }

                }
                pairfirstIndex += 1;
                PairLastIndex += 1;
            }

            if (Pairfound)
            {
                ph.Pair = true;
                ph.PairStr = pairStr;
                ph.PairHighestCard = PairHigh;
                ph.PairHighestSecondCard = pairsecondheigh;
                ph.PairHighestThirdCard = pairthirdhigh;
            }


            /* Set High Cards*/
            ph.HighcardFirst = sortedHand[sortedHand.Count - 1].Rank;
            ph.HighcardSecond = sortedHand[sortedHand.Count - 2].Rank;
            ph.HighcardThird = sortedHand[sortedHand.Count - 3].Rank;        
            ph.HighcardFourth = sortedHand[sortedHand.Count - 4].Rank; 
            ph.HighcardFifth = sortedHand[sortedHand.Count - 5].Rank;

               
            return ph;
            
            
        }


        public static string GetPokerHandAsText(PokerHand ph)
        {
            string result = "";

            if (ph.StraightFlush)
            {
                result += GetRankName(ph.StraightFlushBigest,false)+ "-high, Straight Flush";
                return result;
            }

            if (ph.Poker)
            {
                result += "Quads of "+ GetRankName(ph.PokerStrength, true);
                return result;
            }

            if (ph.FullHouse)
            {
                result += "a Full-House, "+ GetRankName(ph.FullHouseThreeStr, true)+" over "+ GetRankName(ph.FullHouseTwoStr, true);
                return result;
            }

            if (ph.Flush)
            {
                result += GetRankName(ph.FlushHighest, false)+"-high Flush";
                return result;
            }

            if (ph.Straight)
            {
                result += GetRankName(ph.StraightHighest, false)+ "-high Straight";
                return result;
            }

            if (ph.ThreeOfaKind)
            {
                result += "Three of a kind "+ GetRankName(ph.ThreeOfaKindHighest, true);
                return result;
            }

            if (ph.TwoPair)
            {
                result += "Pair of "+ GetRankName(ph.PairOneStr, true)+" and "+ GetRankName(ph.PairTwoStr, true);
                return result;
            }

            if (ph.Pair)
            {
                result += "a Pair of " + GetRankName(ph.PairStr,true);
                return result;
            }
            else
            {
                result += "Highcard "+ GetRankName(ph.HighcardFirst, false);
                return result;
            }

            
        }

        private static string GetRankName(int rank, bool isPlural)  {
            string rankName;
            string[] rankNames = { "two", "three", "four", "five", "six", "seven", "eight",
            "nine", "ten", "jack", "queen", "king", "ace"};

            rankName = rankNames[rank - 2];
            if(isPlural)
            {
                if(rank == 6)
                {
                    rankName += "es";
                } else
                {
                    rankName += "s";

                }

            }

            return rankName;
        }

        public static List<bool> getHighlightedCards(List<Card> playerHand, List<Card> gameHand, PokerHand ph)
        {
            List<bool> result = new List<bool>();
            List<int> indicies = new List<int>();

            for (int i = 0; i <7; i++)
            {
                result.Add(false);
                indicies.Add(i);
            }

            List<Card> allHand = new List<Card>();
            for (int i = 0; i < gameHand.Count; i++)
            {
                allHand.Add(gameHand[i]);

            }
            allHand.Add(playerHand[0]);
            allHand.Add(playerHand[1]);

            List<int> cardstofindRank = new List<int>();


            if(ph.StraightFlush)
            {
                string suit = ph.FlushSuit;

                
                for (int i = 0; i < 5; i++)
                {
                    cardstofindRank.Add(ph.StraightFlushBigest-i);
                }

                for (int j = 0; j < 5; j++)
                {
                    int index = allHand.FindIndex( card => card.Rank == cardstofindRank[j] && card.Suit.Equals(suit));

                    result[indicies[index]] = true;
                    allHand.RemoveAt(index);
                    indicies.RemoveAt(index);

                }
                return result;
            }

            if(ph.Poker)
            {
                for (int i = 0; i < 4; i++)
                {
                    cardstofindRank.Add(ph.PokerStrength);
                }
                cardstofindRank.Add(ph.PokerHighestCard);
                for (int j = 0; j < 5; j++)
                {
                    int index = allHand.FindIndex( card => card.Rank == cardstofindRank[j]);

                    result[indicies[index]] = true;
                    allHand.RemoveAt(index);
                    indicies.RemoveAt(index);

                }

                return result;

            }

            if(ph.FullHouse)
            {
                for (int i = 0; i < 3; i++)
                {
                    cardstofindRank.Add(ph.FullHouseThreeStr);
                }
                for (int i = 0; i < 2; i++)
                {
                    cardstofindRank.Add(ph.FullHouseTwoStr);
                }
                for (int j = 0; j < 5; j++)
                {
                    int index = allHand.FindIndex( card => card.Rank == cardstofindRank[j]);

                    result[indicies[index]] = true;
                    allHand.RemoveAt(index);
                    indicies.RemoveAt(index);

                }

                return result;


            }


            if(ph.Flush)
            {
                string suit = ph.FlushSuit;
                cardstofindRank.Add(ph.FlushHighest);
                cardstofindRank.Add(ph.FlushSecondHighest);
                cardstofindRank.Add(ph.FlushThirdHighest);
                cardstofindRank.Add(ph.FlushFourthHighest);
                cardstofindRank.Add(ph.FlushFifthHighest);


                for (int j = 0; j < 5; j++)
                {
                    int index = allHand.FindIndex( card => card.Rank == cardstofindRank[j] && card.Suit.Equals(suit));

                    result[indicies[index]] = true;
                    allHand.RemoveAt(index);
                    indicies.RemoveAt(index);

                }
                return result;

            }

            if(ph.Straight)
            {
                for (int i = 0; i < 5; i++)
                {
                    cardstofindRank.Add(ph.StraightHighest - i);
                }

                for (int j = 0; j < 5; j++)
                {
                    int index = allHand.FindIndex( card => card.Rank == cardstofindRank[j]);

                    result[indicies[index]] = true;
                    allHand.RemoveAt(index);
                    indicies.RemoveAt(index);

                }
                return result;

            }


            if(ph.ThreeOfaKind)
            {
                for (int i = 0; i < 3; i++)
                {
                    cardstofindRank.Add(ph.ThreeOfaKindHighest);
                }
                cardstofindRank.Add(ph.ThreeOfaKindHighestCard);
                cardstofindRank.Add(ph.ThreeOfaKindSecondHighestCard);

                for (int j = 0; j < 5; j++)
                {
                    int index = allHand.FindIndex( card => card.Rank == cardstofindRank[j]);

                    result[indicies[index]] = true;
                    allHand.RemoveAt(index);
                    indicies.RemoveAt(index);

                }
                return result;


            }

            if (ph.TwoPair)
            {
                for (int i = 0; i < 2; i++)
                {
                    cardstofindRank.Add(ph.PairOneStr);
                }
                for (int i = 0; i < 2; i++)
                {
                    cardstofindRank.Add(ph.PairTwoStr);
                }
               
                cardstofindRank.Add(ph.TwoPairHighestCard);

                for (int j = 0; j < 5; j++)
                {
                    int index = allHand.FindIndex( card => card.Rank == cardstofindRank[j]);

                    result[indicies[index]] = true;
                    allHand.RemoveAt(index);
                    indicies.RemoveAt(index);

                }
                return result;


            }



            if (ph.Pair)
            {
                cardstofindRank.Add(ph.PairStr);
                cardstofindRank.Add(ph.PairStr);
                cardstofindRank.Add(ph.PairHighestCard);
                cardstofindRank.Add(ph.PairHighestSecondCard);
                cardstofindRank.Add(ph.PairHighestThirdCard);

                for (int j = 0; j < 5; j++)
                {
                    int index = allHand.FindIndex( card => card.Rank == cardstofindRank[j]);

                    result[indicies[index]] = true;
                    allHand.RemoveAt(index);
                    indicies.RemoveAt(index);

                }

                return result;
            } else
            {
                cardstofindRank.Add(ph.HighcardFirst);
                cardstofindRank.Add(ph.HighcardSecond);
                cardstofindRank.Add(ph.HighcardThird);
                cardstofindRank.Add(ph.HighcardFourth);
                cardstofindRank.Add(ph.HighcardFifth);

                for (int j = 0; j < 5; j++)
                {
                    int index = allHand.FindIndex( card => card.Rank == cardstofindRank[j]);

                    result[indicies[index]] = true;
                    allHand.RemoveAt(index);
                    indicies.RemoveAt(index);

                }

                return result;



            }


        }


        public  int Compare(Player firstPlayer, Player secondPlayer )
        {
            PokerHand firsthand = firstPlayer.pHand;
            PokerHand secondhand = secondPlayer.pHand;

            /* compare straight flush */
            if (firsthand.StraightFlush && secondhand.StraightFlush == false)
                return 1;


            if (secondhand.StraightFlush && firsthand.StraightFlush == false)
                return -1;

            if (firsthand.StraightFlush && secondhand.StraightFlush)
            {
                if (firsthand.StraightFlushBigest > secondhand.StraightFlushBigest)
                    return 1;

                if (firsthand.StraightFlushBigest < secondhand.StraightFlushBigest)
                    return -1;


                if (firsthand.StraightFlushBigest == secondhand.StraightFlushBigest)
                    return 0;
            }

            /* compare Poker*/
            if (firsthand.Poker && secondhand.Poker == false)
                return 1;

            if (secondhand.Poker && firsthand.Poker == false)
                return -1;


            if (firsthand.Poker && secondhand.Poker)
            {
                if (firsthand.PokerStrength > secondhand.PokerStrength)
                    return 1;

                if (firsthand.PokerStrength < secondhand.PokerStrength)
                    return -1;

                if (firsthand.PokerStrength == secondhand.PokerStrength)
                {
                    if (firsthand.PokerHighestCard > secondhand.PokerHighestCard)
                        return 1;

                    if (firsthand.PokerHighestCard < secondhand.PokerHighestCard)
                        return -1;

                    if (firsthand.PokerHighestCard == secondhand.PokerHighestCard)
                        return 0;
                }

            }

            /* compare FullHouse*/
            if (firsthand.FullHouse && secondhand.FullHouse == false)
                return 1;


            if (secondhand.FullHouse && firsthand.FullHouse == false)
                return -1;

            if (firsthand.FullHouse && secondhand.FullHouse)
            {
                if (firsthand.FullHouseThreeStr > secondhand.FullHouseThreeStr)
                    return 1;

                if (firsthand.FullHouseThreeStr < secondhand.FullHouseThreeStr)
                    return -1;

                if (firsthand.FullHouseThreeStr == secondhand.FullHouseThreeStr)
                {
                    if (firsthand.FullHouseTwoStr > secondhand.FullHouseTwoStr)
                        return 1;

                    if (firsthand.FullHouseTwoStr < secondhand.FullHouseTwoStr)
                        return -1;

                    if (firsthand.FullHouseTwoStr == secondhand.FullHouseTwoStr)
                        return 0;
                }

            }

            /* comapre flush*/
            if (firsthand.Flush && secondhand.Flush == false)
                return 1;

            if (secondhand.Flush && firsthand.Flush == false)
                return -1;

            if (firsthand.Flush && secondhand.Flush)
            {
                if (firsthand.FlushHighest > secondhand.FlushHighest)
                    return 1;

                if (firsthand.FlushHighest < secondhand.FlushHighest)
                    return -1;

                if (firsthand.FlushHighest == secondhand.FlushHighest)
                {
                    if (firsthand.FlushSecondHighest > secondhand.FlushSecondHighest)
                        return 1;

                    if (firsthand.FlushSecondHighest < secondhand.FlushSecondHighest)
                        return -1;

                    if (firsthand.FlushSecondHighest == secondhand.FlushSecondHighest)
                    {
                        if (firsthand.FlushThirdHighest > secondhand.FlushThirdHighest)
                            return 1;

                        if (firsthand.FlushThirdHighest < secondhand.FlushThirdHighest)
                            return -1;

                        if (firsthand.FlushThirdHighest == secondhand.FlushThirdHighest)
                        {

                            if (firsthand.FlushFourthHighest > secondhand.FlushFourthHighest)
                                return 1;

                            if (firsthand.FlushFourthHighest < secondhand.FlushFourthHighest)
                                return -1;

                            if(firsthand.FlushFourthHighest ==  secondhand.FlushFourthHighest)
                            {

                                if (firsthand.FlushFifthHighest > secondhand.FlushFifthHighest)
                                    return 1;

                                if (firsthand.FlushFifthHighest < secondhand.FlushFifthHighest)
                                    return -1;

                                if (firsthand.FlushFifthHighest == secondhand.FlushFifthHighest)
                                    return 0;

                            }

                        }
                    }
                }
                    
            }




            /* comapre straight*/
            if (firsthand.Straight && secondhand.Straight == false)
                return 1;

            if (secondhand.Straight && firsthand.Straight == false)
                return -1;

            if (firsthand.Straight && secondhand.Straight)
            {
                if (firsthand.StraightHighest > secondhand.StraightHighest)
                    return 1;

                if (firsthand.StraightHighest < secondhand.StraightHighest)
                    return -1;

                if (firsthand.StraightHighest == secondhand.StraightHighest)
                    return 0;
            }

            /* comapre three of a kinds*/
            if (firsthand.ThreeOfaKind && secondhand.ThreeOfaKind == false)
                return 1;

            if (secondhand.ThreeOfaKind && firsthand.ThreeOfaKind == false)
                return -1;

            if (firsthand.ThreeOfaKind && secondhand.ThreeOfaKind)
            {
                if (firsthand.ThreeOfaKindHighest > secondhand.ThreeOfaKindHighest)
                    return 1;

                if (firsthand.ThreeOfaKindHighest < secondhand.ThreeOfaKindHighest)
                    return -1;

                if (firsthand.ThreeOfaKindHighest == secondhand.ThreeOfaKindHighest)
                {
                    if (firsthand.ThreeOfaKindHighestCard > secondhand.ThreeOfaKindHighestCard)
                        return 1;

                    if (firsthand.ThreeOfaKindHighestCard < secondhand.ThreeOfaKindHighestCard)
                        return -1;

                    if (firsthand.ThreeOfaKindHighestCard == secondhand.ThreeOfaKindHighestCard)
                    {
                        if (firsthand.ThreeOfaKindSecondHighestCard > secondhand.ThreeOfaKindSecondHighestCard)
                            return 1;

                        if (firsthand.ThreeOfaKindSecondHighestCard < secondhand.ThreeOfaKindSecondHighestCard)
                            return -1;

                        if (firsthand.ThreeOfaKindSecondHighestCard == secondhand.ThreeOfaKindSecondHighestCard)
                            return 0;

                    }
                }
            }




            /* comapre twoPair*/
            if (firsthand.TwoPair && secondhand.TwoPair == false)
                return 1;

            if (secondhand.TwoPair && firsthand.TwoPair == false)
                return -1;

            if (firsthand.TwoPair && secondhand.TwoPair)
            {
                if (firsthand.PairOneStr > secondhand.PairOneStr)
                    return 1;

                if (firsthand.PairOneStr < secondhand.PairOneStr)
                    return -1;

                if (firsthand.PairOneStr == secondhand.PairOneStr)
                {
                    if (firsthand.PairTwoStr > secondhand.PairTwoStr)
                        return 1;

                    if (firsthand.PairTwoStr < secondhand.PairTwoStr)
                        return -1;

                    if (firsthand.PairTwoStr == secondhand.PairTwoStr)
                    {
                        if (firsthand.TwoPairHighestCard > secondhand.TwoPairHighestCard)
                            return 1;

                        if (firsthand.TwoPairHighestCard < secondhand.TwoPairHighestCard)
                            return -1;

                        if (firsthand.TwoPairHighestCard == secondhand.TwoPairHighestCard)
                            return 0;

                    }
                }
            }


            /* comapre onePair*/
            if (firsthand.Pair && secondhand.Pair == false)
                return 1;

            if (secondhand.Pair && firsthand.Pair == false)
                return -1;

            if (firsthand.Pair && secondhand.Pair)
            {
                if (firsthand.PairStr > secondhand.PairStr)
                    return 1;

                if (firsthand.PairStr < secondhand.PairStr)
                    return -1;

                if (firsthand.PairStr == secondhand.PairStr)
                {
                    if (firsthand.PairHighestCard > secondhand.PairHighestCard)
                        return 1;

                    if (firsthand.PairHighestCard < secondhand.PairHighestCard)
                        return -1;

                    if (firsthand.PairHighestCard == secondhand.PairHighestCard)
                    {
                        if (firsthand.PairHighestSecondCard > secondhand.PairHighestSecondCard)
                            return 1;

                        if (firsthand.PairHighestSecondCard < secondhand.PairHighestSecondCard)
                            return -1;

                        if (firsthand.PairHighestSecondCard == secondhand.PairHighestSecondCard)
                        {

                            if (firsthand.PairHighestThirdCard > secondhand.PairHighestThirdCard)
                                return 1;

                            if (firsthand.PairHighestThirdCard < secondhand.PairHighestThirdCard)
                                return -1;

                            if (firsthand.PairHighestThirdCard == secondhand.PairHighestThirdCard)
                                return 0;

                        }
                            

                    }
                }
            }




            /* comapre highCard*/

            if (firsthand.HighcardFirst > secondhand.HighcardFirst)
            {
                return 1;
            }

            if (firsthand.HighcardFirst < secondhand.HighcardFirst)
            {
                return -1;
            }

            if (firsthand.HighcardFirst == secondhand.HighcardFirst)
            {


                if (firsthand.HighcardSecond > secondhand.HighcardSecond)
                    return 1;

                if (firsthand.HighcardSecond < secondhand.HighcardSecond)
                    return -1;

                if (firsthand.HighcardSecond == secondhand.HighcardSecond)
                {
                    if (firsthand.HighcardThird > secondhand.HighcardThird)
                        return 1;

                    if (firsthand.HighcardThird < secondhand.HighcardThird)
                        return -1;

                    if (firsthand.HighcardThird == secondhand.HighcardThird)
                    {
                        if (firsthand.HighcardFourth > secondhand.HighcardFourth)
                            return 1;

                        if (firsthand.HighcardFourth < secondhand.HighcardFourth)
                            return -1;

                        if (firsthand.HighcardFourth == secondhand.HighcardFourth)
                        {
                            if (firsthand.HighcardFifth > secondhand.HighcardFifth)
                                return 1;

                            if (firsthand.HighcardFifth < secondhand.HighcardFifth)
                                return -1;

                            if (firsthand.HighcardFifth == secondhand.HighcardFifth)
                                return 0;

                        }

                    }
                }
            }

            return 0;
        }
    }
}
