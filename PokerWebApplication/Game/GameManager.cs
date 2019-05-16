using Microsoft.AspNetCore.SignalR;
using PokerWebApplication.Hubs;
using PokerWebApplication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PokerWebApplication.Game
{
    public class GameManager
    {

        public bool DeactivateAllIndicatorRequired { get; set; }


        private List<Player> players = new List<Player>();

        public List<Card> TableHand { get; } = new List<Card>();


        private Deck d = new Deck();

        private int dealerNum = 0;

        //Active player index of an active player who decides his action, last in a phase
        private int lastPlayerActionPosition = 0;

        //Active player index of an active player who decided his action is Raise, last in a phase
        private int lastPlayerRaisePosition = -1;

      
        private int tableId;

        //The amount of money needed to stay in the phase
        private int raiseValue = 0;

        public int Pot { get; private set; }

        public List<Player> ActivePlayers { get; } = new List<Player>();

        public List<Player> AllInPlayers { get; } = new List<Player>();

        

        public List<ScoreInfo> ScoreBoard { get; } = new List<ScoreInfo>();

       

        public List<string> PlayerKonckedOut { get; } = new List<string>();

        public string Dealer { get; set; }
        public string SmallBlind { get; set; }
        public string BigBlind { get; set; }

        public int BlindValue { get; private set; }

        public int GamePhase { get; set; }
        public string GamePhaseName { get; set; }

        public string CurrentPlayer { get; set; }
        public int CurrentPlayerPos { get; private set; }

        private int nextPlayerPosition;

        private bool askFirstPlayerInPhase;

       

        public bool ReadyForNextRound { get; private set; }

        public bool ReadyForNextPhase { get; private set; }
        public string WinnerPlayer { get; private set; }
        public bool GameEnded { get; private set; }
        public List<bool> TableHandHighlight { get; private set; }

        public string VictoryText { get; set; }

        public GameManager()
        {
           

           
        }

        public void Init(List<Player> PlayerList, int tableID)
        {
           
            players = PlayerList;

            tableId = tableID;

            DeactivateAllIndicatorRequired = false;

            dealerNum = 0;

            ReadyForNextPhase = false;
            ReadyForNextRound = false;

            GameEnded = false;

            GamePhase = 0;

            BlindValue = 40;

            TableHandHighlight = new List<bool>();

            VictoryText = "";
            WinnerPlayer = "";
        }

        public void Clear()
        {
            players.Clear();
            TableHand.Clear();
            ScoreBoard.Clear();
            PlayerKonckedOut.Clear();
        }

        public void StartNextRound()
        {
            ReadyForNextPhase = false;
            ReadyForNextRound = false;
            GamePhase = 0;
            Pot = 0;
            raiseValue = 0;
            lastPlayerRaisePosition = -1;

            d.ResetDeck();
            dealerNum += 1;
            VictoryText = "";

            for (int i = 0; i < players.Count; i++)
            {
                if(players[i].Action.Equals("giveup"))
                {
                    PlayerKonckedOut.Add(players[i].Name);
                    players.Remove(players[i]);
                    i -= 1;
                }

            }

            ActivePlayers.Clear();
            AllInPlayers.Clear();

            foreach(Player p in players)
            {
                p.Hand.Clear();
                p.SmallBlind = false;
                p.BigBlind = false;
                p.Dealer = false;
                p.PlayersTurn = false;
                p.IsAllIn = false;
                p.RaiseValue = 0;
                p.AllInValue = 0;
                p.PotMoney = 0;
                p.CardOneHighlight = false;
                p.CardTwoHighlight = false;



                p.PublicHand.Clear();
               

                p.Action = "";

                if(p.Chips > 0)
                {
                    ActivePlayers.Add(p);
                    p.PublicHand.Add(new Card("back", 0));
                    p.PublicHand.Add(new Card("back", 0));
                } else
                {
                    p.PublicHand.Add(new Card(" ", 0));
                    p.PublicHand.Add(new Card(" ", 0));

                    if(PlayerKonckedOut.Contains(p.Name) == false)
                    {
                        PlayerKonckedOut.Add(p.Name);
                    }

                }

            }
            ScoreBoard.Clear();
            foreach (Player p in ActivePlayers)
            {
                ScoreBoard.Add( new ScoreInfo() { Name=p.Name, Chips= p.Chips});

            }
            ScoreBoard.Sort(new ScoreInfo());

            ScoreBoard.Reverse();

            for(int i = PlayerKonckedOut.Count-1; i >= 0; i--)
            {
                ScoreBoard.Add(new ScoreInfo() { Name = PlayerKonckedOut[i], Chips = 0 });
            }


            if (dealerNum >= ActivePlayers.Count)
                dealerNum = 0;

            lastPlayerActionPosition = dealerNum;

            //clear table hand
            TableHand.Clear();

            TableHandHighlight.Clear();
            for(int i =0; i < 5; i++)
            {
                TableHandHighlight.Add(false);
            }
        }



       

        public void GetBlinds()
        {
            raiseValue = BlindValue;

            if (dealerNum < ActivePlayers.Count - 2)
            {
                ActivePlayers[dealerNum].Dealer = true;
               


                ActivePlayers[dealerNum + 1].SmallBlind = true;

                SmallBlind = ActivePlayers[dealerNum + 1].Name;

           

                ActivePlayers[dealerNum + 2].BigBlind = true;
                BigBlind = ActivePlayers[dealerNum + 2].Name;

            }

            if (dealerNum == ActivePlayers.Count - 2)
            {
                ActivePlayers[dealerNum].Dealer = true;
                Dealer = ActivePlayers[dealerNum].Name;

                ActivePlayers[dealerNum + 1].SmallBlind = true;
                SmallBlind = ActivePlayers[dealerNum + 1].Name;


                ActivePlayers[0].BigBlind = true;
                BigBlind = ActivePlayers[0].Name;
               
            }

            if (dealerNum == ActivePlayers.Count - 1)
            {
                ActivePlayers[dealerNum].Dealer = true;
                Dealer = ActivePlayers[dealerNum].Name;

                ActivePlayers[0].SmallBlind = true;
                SmallBlind = ActivePlayers[0].Name;

                ActivePlayers[1].BigBlind = true;
                BigBlind = ActivePlayers[1].Name;

            }

            foreach(Player p in ActivePlayers)
            {
                if(p.BigBlind)
                {
                    if(BlindValue >= p.Chips)
                    {
                        p.IsAllIn = true;
                        AllInPlayers.Add(ActivePlayers[CurrentPlayerPos]);
                        p.AllInValue = p.Chips;
                        Pot += p.Chips;

                        p.Chips = 0;

                    } else
                    {
                        p.RaiseValue = BlindValue;
                        p.Chips -= BlindValue;
                        p.PotMoney += BlindValue;
                        Pot += BlindValue;
                    }                  
                }

                if (p.SmallBlind)
                {
                    if (BlindValue / 2 >= p.Chips)
                    {
                        p.IsAllIn = true;
                        AllInPlayers.Add(ActivePlayers[CurrentPlayerPos]);
                        p.AllInValue = p.Chips;
                        Pot += p.Chips;

                        p.Chips = 0;

                    }
                    else
                    {
                        p.RaiseValue = BlindValue / 2;
                        p.Chips -= BlindValue / 2;
                        p.PotMoney += BlindValue / 2;
                        Pot += BlindValue / 2;
                    }
                }

            }


        }

        public void GiveCards()
        {
           
           for (int j = dealerNum+1; j < ActivePlayers.Count; j++)
           {
                ActivePlayers[j].Hand.Add(d.DrawCard());
                ActivePlayers[j].Hand.Add(d.DrawCard());

              
                    
           }


           for (int j = 0; j < dealerNum+1; j++)
           {
                ActivePlayers[j].Hand.Add(d.DrawCard());
                ActivePlayers[j].Hand.Add(d.DrawCard());

                
           }

            askFirstPlayerInPhase = true;
        }


       


        public void GetCurrentPlayerAction()
        {

            foreach(Player p in ActivePlayers)
            {
                p.PlayersTurn = false;
            }

            if (askFirstPlayerInPhase)
            {
                askFirstPlayerInPhase = false;
               
                   

                CurrentPlayerPos = -1;
                nextPlayerPosition = -1;

                bool foundCurrentPlayerPos = false;
                bool foundNextPlayerPos = false;
                for(int i = lastPlayerActionPosition + 1; i < ActivePlayers.Count; i++)
                {

                    if ((ActivePlayers[i].IsAllIn == false  || (ActivePlayers[i].IsAllIn == true && i == lastPlayerRaisePosition)) 
                        && foundCurrentPlayerPos == true && foundNextPlayerPos == false)
                    {
                        foundNextPlayerPos = true;
                        nextPlayerPosition = i;
                    }

                    if (ActivePlayers[i].IsAllIn == false && foundCurrentPlayerPos== false)
                    {
                        foundCurrentPlayerPos = true;
                        CurrentPlayerPos = i;
                        CurrentPlayer = ActivePlayers[CurrentPlayerPos].Name;
                        ActivePlayers[CurrentPlayerPos].PlayersTurn = true;
                    }   

                       

                }
                if(foundCurrentPlayerPos == false || foundNextPlayerPos == false)
                {
                    for (int i = 0; i < lastPlayerActionPosition + 1; i++)
                    {

                        if ((ActivePlayers[i].IsAllIn == false || (ActivePlayers[i].IsAllIn == true && i == lastPlayerRaisePosition)) 
                            && foundCurrentPlayerPos == true && foundNextPlayerPos == false)
                        {
                            foundNextPlayerPos = true;
                            nextPlayerPosition = i;
                        }

                        if (ActivePlayers[i].IsAllIn == false && foundCurrentPlayerPos == false)
                        {
                                foundCurrentPlayerPos = true;
                                CurrentPlayerPos = i;
                                CurrentPlayer = ActivePlayers[CurrentPlayerPos].Name;
                                ActivePlayers[CurrentPlayerPos].PlayersTurn = true;
                        }

                            

                    }

                }

                if (foundNextPlayerPos == false && foundCurrentPlayerPos == true)
                {
                    askFirstPlayerInPhase = true;

                    ReadyForNextPhase = true;

                    GamePhase += 1;
                    if (GamePhase == 4)
                    {
                        ReadyForNextRound = true;
                    }

                }


                if (nextPlayerPosition == lastPlayerRaisePosition)
                {
                    askFirstPlayerInPhase = true;

                    ReadyForNextPhase = true;

                    GamePhase += 1;
                    if (GamePhase == 4)
                    {
                        ReadyForNextRound = true;
                    }

                }


            } else
            {
                if(nextPlayerPosition == lastPlayerActionPosition)
                {
                    CurrentPlayerPos = nextPlayerPosition;
                    CurrentPlayer = ActivePlayers[nextPlayerPosition].Name;
                    ActivePlayers[nextPlayerPosition].PlayersTurn = true;
                    askFirstPlayerInPhase = true;

                    nextPlayerPosition += 1;

                    if (nextPlayerPosition == ActivePlayers.Count)
                    {
                        nextPlayerPosition = 0;
                    }

                    
                   
                   

                } else
                {
                   
                    

                    bool foundCurrentPlayerPos = false;
                    bool foundNextPlayerPos = false;
                    if(nextPlayerPosition > lastPlayerActionPosition)
                    {
                        for (int i = nextPlayerPosition; i < ActivePlayers.Count; i++)
                        {

                            if ((ActivePlayers[i].IsAllIn == false || (ActivePlayers[i].IsAllIn == true && i == lastPlayerRaisePosition)) 
                                && foundCurrentPlayerPos == true && foundNextPlayerPos == false)
                            {
                                foundNextPlayerPos = true;
                                nextPlayerPosition = i;
                            }


                            if (ActivePlayers[i].IsAllIn == false && foundCurrentPlayerPos == false)
                            {
                                foundCurrentPlayerPos = true;
                                CurrentPlayerPos = i;
                                CurrentPlayer = ActivePlayers[CurrentPlayerPos].Name;
                                ActivePlayers[CurrentPlayerPos].PlayersTurn = true;
                            }

                        }

                        if (foundCurrentPlayerPos == false || foundNextPlayerPos == false)
                        {
                            for (int i = 0; i < lastPlayerActionPosition + 1; i++)
                            {

                                if ((ActivePlayers[i].IsAllIn == false || (ActivePlayers[i].IsAllIn == true && i == lastPlayerRaisePosition)) 
                                    && foundCurrentPlayerPos == true && foundNextPlayerPos == false)
                                {
                                    foundNextPlayerPos = true;
                                    nextPlayerPosition = i;
                                }



                                if (ActivePlayers[i].IsAllIn == false && foundCurrentPlayerPos == false)
                                {
                                    foundCurrentPlayerPos = true;
                                    CurrentPlayerPos = i;
                                    CurrentPlayer = ActivePlayers[CurrentPlayerPos].Name;
                                    ActivePlayers[CurrentPlayerPos].PlayersTurn = true;
                                }



                            }

                        }


                    } else
                    {

                        if (foundCurrentPlayerPos == false || foundNextPlayerPos == false)
                        {
                            for (int i = nextPlayerPosition; i < lastPlayerActionPosition + 1; i++)
                            {

                                if ((ActivePlayers[i].IsAllIn == false || (ActivePlayers[i].IsAllIn == true && i == lastPlayerRaisePosition)) && foundCurrentPlayerPos == true && foundNextPlayerPos == false)
                                {
                                    foundNextPlayerPos = true;
                                    nextPlayerPosition = i;
                                }



                                if (ActivePlayers[i].IsAllIn == false && foundCurrentPlayerPos == false)
                                {
                                    foundCurrentPlayerPos = true;
                                    CurrentPlayerPos = i;
                                    CurrentPlayer = ActivePlayers[CurrentPlayerPos].Name;
                                    ActivePlayers[CurrentPlayerPos].PlayersTurn = true;
                                }
                            }

                        }

                    }
                   

                    if (foundNextPlayerPos == false && foundCurrentPlayerPos == true)
                    {
                        askFirstPlayerInPhase = true;

                        ReadyForNextPhase = true;

                        GamePhase += 1;
                        if (GamePhase == 4)
                        {
                            ReadyForNextRound = true;
                        }

                    }


                    if (nextPlayerPosition == lastPlayerRaisePosition)
                    {

                        askFirstPlayerInPhase = true;

                        ReadyForNextPhase = true;

                        GamePhase += 1;
                        if (GamePhase == 4)
                        {
                            ReadyForNextRound = true;
                        }
                    }


                }
 
            }

        }

        

        public void ProcessPlayerAction(string playerAction)
        {

            if(playerAction.Equals("Called"))
            {

                if (CurrentPlayerPos == lastPlayerActionPosition &&  
                    (lastPlayerRaisePosition != -1 && nextPlayerPosition != lastPlayerRaisePosition) == false)
                {
                    ReadyForNextPhase = true;

                    GamePhase += 1;
                    if (GamePhase == 4)
                    {
                        ReadyForNextRound = true;
                    }

                }

                if (ActivePlayers[CurrentPlayerPos].Chips <= raiseValue)
                {
                    //player action is All in
                    ActivePlayers[CurrentPlayerPos].Action = "All in";

                    ActivePlayers[CurrentPlayerPos].AllInValue = ActivePlayers[CurrentPlayerPos].Chips;

                    Pot += ActivePlayers[CurrentPlayerPos].Chips;

                    ActivePlayers[CurrentPlayerPos].PotMoney += ActivePlayers[CurrentPlayerPos].Chips;

                    ActivePlayers[CurrentPlayerPos].Chips = 0;
                } else
                {
                    //player action is Call
                    ActivePlayers[CurrentPlayerPos].Action = "Call";

                    ActivePlayers[CurrentPlayerPos].Chips -= raiseValue - ActivePlayers[CurrentPlayerPos].RaiseValue;

                    Pot += raiseValue - ActivePlayers[CurrentPlayerPos].RaiseValue;
                    ActivePlayers[CurrentPlayerPos].PotMoney += raiseValue - ActivePlayers[CurrentPlayerPos].RaiseValue;

                    ActivePlayers[CurrentPlayerPos].RaiseValue =raiseValue;
                }


                    

            }

            if (playerAction.Equals("Folded") || playerAction.Equals("giveup"))
            {

                if (CurrentPlayerPos == lastPlayerActionPosition && 
                    (lastPlayerRaisePosition != -1 && nextPlayerPosition != lastPlayerRaisePosition) == false)
                {
                    ReadyForNextPhase = true;

                    GamePhase += 1;
                    if (GamePhase == 4)
                    {
                        ReadyForNextRound = true;
                    }

                }

                if(playerAction.Equals("giveup"))
                {
                    ActivePlayers[CurrentPlayerPos].Action = "giveup";
                } else
                {
                    ActivePlayers[CurrentPlayerPos].Action = "Fold";
                }
                

                ActivePlayers[CurrentPlayerPos].PlayersTurn = false;
                Player p = ActivePlayers.Find(player => player.Name == CurrentPlayer);
                p.PlayersTurn = false;
                ActivePlayers.Remove(p);

                
                if(CurrentPlayerPos == 0)
                {
                   
                    if (CurrentPlayerPos == lastPlayerActionPosition)
                    {
                        lastPlayerActionPosition = ActivePlayers.Count - 1;
                        
                    } else
                    {
                        lastPlayerActionPosition -= 1;
                    }

                    CurrentPlayerPos = ActivePlayers.Count - 1;
                    nextPlayerPosition = 0;

                    if(lastPlayerRaisePosition != -1)
                    {
                        lastPlayerRaisePosition -= 1;
                    }
                  

                } else
                {
                  
                    if (CurrentPlayerPos == lastPlayerActionPosition)
                    {
                        lastPlayerActionPosition -= 1;
                    } else
                    {
                        if (CurrentPlayerPos < lastPlayerActionPosition)
                        {
                            lastPlayerActionPosition -= 1;
                        }

                    }

                    if(nextPlayerPosition != 0)
                    {
                        nextPlayerPosition -= 1;
                    }

                    if(CurrentPlayerPos < lastPlayerRaisePosition && lastPlayerRaisePosition != -1)
                    {
                        lastPlayerRaisePosition -= 1;
                    }

                    CurrentPlayerPos -= 1;

                }

            }


            if (playerAction.Contains("Raised"))
            {
                bool canPlayerRaise = true;

                Player p = ActivePlayers.Find(player => player.Name == CurrentPlayer);
                int playerraisevalue = int.Parse(playerAction.Split(" ")[1]);

                if (playerraisevalue > p.Chips)
                {
                    playerraisevalue = p.Chips;
                }

                if ( p.Chips <= raiseValue )
                {
                    canPlayerRaise = false;
                }

                if(ActivePlayers.Count - AllInPlayers.Count == 1 )
                {
                    canPlayerRaise = false;
                }

                if (lastPlayerRaisePosition != -1)
                {

                    if(lastPlayerRaisePosition > lastPlayerActionPosition)
                    {
                        if (CurrentPlayerPos > lastPlayerActionPosition && CurrentPlayerPos < lastPlayerRaisePosition)
                            canPlayerRaise = false;

                    } else
                    {
                        if (CurrentPlayerPos > lastPlayerActionPosition ||  CurrentPlayerPos < lastPlayerRaisePosition)
                            canPlayerRaise = false;
                    }


                }
                
                if(canPlayerRaise)
                {
                              
                    lastPlayerRaisePosition = CurrentPlayerPos;
                                  
                    if(playerraisevalue == p.Chips || p.Chips <= (raiseValue+ playerraisevalue))
                    {
                        //player action is All in
                        ActivePlayers[CurrentPlayerPos].Action = "All in";
                        ActivePlayers[CurrentPlayerPos].AllInValue = ActivePlayers[CurrentPlayerPos].Chips;
                        if (raiseValue <= p.Chips)
                        {
                            raiseValue = p.Chips;
                        }


                       
                        Pot += p.Chips;
                        ActivePlayers[CurrentPlayerPos].PotMoney += ActivePlayers[CurrentPlayerPos].Chips;

                        p.Chips = 0;

                    } else
                    {
                        //player action is raise
                        ActivePlayers[CurrentPlayerPos].Action = playerAction;

                        raiseValue += playerraisevalue;

                        p.Chips -= raiseValue;
                        Pot += raiseValue;
                        ActivePlayers[CurrentPlayerPos].PotMoney += raiseValue;

                        p.RaiseValue += raiseValue;

                    }

                } else
                {

                    if ( p.Chips <= raiseValue )
                    {
                        //player action is All in
                        ActivePlayers[CurrentPlayerPos].Action = "All in";
                        ActivePlayers[CurrentPlayerPos].AllInValue = ActivePlayers[CurrentPlayerPos].Chips;

                        Pot += p.Chips;
                        ActivePlayers[CurrentPlayerPos].PotMoney += ActivePlayers[CurrentPlayerPos].Chips;

                        p.Chips = 0;


                        if (CurrentPlayerPos == lastPlayerActionPosition &&
                            (lastPlayerRaisePosition != -1 && nextPlayerPosition != lastPlayerRaisePosition) == false)
                        {
                            ReadyForNextPhase = true;

                            GamePhase += 1;
                            if (GamePhase == 4)
                            {
                                ReadyForNextRound = true;
                            }

                        }

                    } else
                    {
                        //player action is call
                        ActivePlayers[CurrentPlayerPos].Action = "Call";
                        ActivePlayers[CurrentPlayerPos].Chips -= raiseValue - ActivePlayers[CurrentPlayerPos].RaiseValue;



                        Pot += raiseValue - ActivePlayers[CurrentPlayerPos].RaiseValue;

                        ActivePlayers[CurrentPlayerPos].PotMoney += raiseValue - ActivePlayers[CurrentPlayerPos].RaiseValue;

                        if (CurrentPlayerPos == lastPlayerActionPosition &&
                            (lastPlayerRaisePosition != -1 && nextPlayerPosition != lastPlayerRaisePosition) == false)
                        {
                            ReadyForNextPhase = true;

                            GamePhase += 1;
                            if (GamePhase == 4)
                            {
                                ReadyForNextRound = true;
                            }

                        }

                    }                
                }

            }

            if (ActivePlayers[CurrentPlayerPos].Chips == 0)
            {
                AllInPlayers.Add(ActivePlayers[CurrentPlayerPos]);

                ActivePlayers[CurrentPlayerPos].IsAllIn = true;

            }


        }


        public void StartNextPhase()
        {
            lastPlayerRaisePosition = -1;

            raiseValue = 0;
            

            for(int i=0; i <  ActivePlayers.Count; i++)
            {
                Player p = ActivePlayers[i];
                if(p.Action.Equals("giveup") == false)
                {
                    p.Action = "";
                }
               
                p.RaiseValue = 0;

                if(p.IsAllIn)
                {
                    p.Action = "All in";
                    if (i == lastPlayerActionPosition)
                    {
                        if(i == 0)
                        {
                            lastPlayerActionPosition = ActivePlayers.Count - 2;
                        } else
                        {
                            lastPlayerActionPosition -= 1;
                        }

                    }

                    ActivePlayers.Remove(p);
                    i -= 1;
                }

            }

            switch (GamePhase)
            {
                case 1:  StartFlop(); break;
                case 2:  StartTurn(); break;
                case 3:  StartRiver(); break;
            }
        }


       
        private void StartFlop()
        {
            ReadyForNextPhase = false;
            GamePhaseName = "Flop";
            for (int i = 0; i < 3; i++)
            {
                TableHand.Add(d.DrawCard());
            }

            

        }

        private void StartTurn()
        {
            ReadyForNextPhase = false;
            GamePhaseName = "Turn";

            TableHand.Add(d.DrawCard());

           
        }

        private void StartRiver()
        {
            ReadyForNextPhase = false;
            GamePhaseName = "River";
            TableHand.Add(d.DrawCard());

            
        }

        public string FinishRound()
        {
            List<Player> playersStayedIn = new List<Player>();
            List<Player> playersWithChipsLeft = new List<Player>();

            if(ActivePlayers.Count == 1)
            {
                playersStayedIn.Add(ActivePlayers[0]);

                ActivePlayers[0].PlayersTurn = false;
            } else
            {
                foreach (Player p in ActivePlayers)
                {

                    p.PublicHand[0].Suit = p.Hand[0].Suit;
                    p.PublicHand[0].Rank = p.Hand[0].Rank;

                    p.PublicHand[1].Suit = p.Hand[1].Suit;
                    p.PublicHand[1].Rank = p.Hand[1].Rank;

                    p.pHand = PokerRules.SetPokerHand(p.Hand, TableHand);

                    playersStayedIn.Add(p);

                    p.PlayersTurn = false;

                }

            }


            if (ActivePlayers.Count == 0 && AllInPlayers.Count == 1)
            {
                playersStayedIn.Add(AllInPlayers[0]);

                AllInPlayers[0].PlayersTurn = false;
            }
            else
            {
                foreach (Player p in AllInPlayers)
                {
                    p.PublicHand[0].Suit = p.Hand[0].Suit;
                    p.PublicHand[0].Rank = p.Hand[0].Rank;

                    p.PublicHand[1].Suit = p.Hand[1].Suit;
                    p.PublicHand[1].Rank = p.Hand[1].Rank;
                    p.pHand = PokerRules.SetPokerHand(p.Hand, TableHand);

                    playersStayedIn.Add(p);
                }
            }


            if(ActivePlayers.Count == 1  || (ActivePlayers.Count == 0 && AllInPlayers.Count==1) == false)
            {
                playersStayedIn.Sort(new PokerRules());
            }               


          

            if(playersStayedIn[playersStayedIn.Count-1].IsAllIn == false)
            {

                playersStayedIn[playersStayedIn.Count - 1].Chips += Pot;
            } else
            {
                foreach(Player p in players)
                {
                    if(p.PotMoney > playersStayedIn[playersStayedIn.Count - 1].PotMoney)
                    {
                        p.PotMoney -= playersStayedIn[playersStayedIn.Count - 1].PotMoney;

                        playersStayedIn[playersStayedIn.Count - 1].Chips += playersStayedIn[playersStayedIn.Count - 1].PotMoney;

                        p.Chips += p.PotMoney;


                    } else
                    {
                        playersStayedIn[playersStayedIn.Count - 1].Chips += p.PotMoney;
                    }

                }             
            }

            playersStayedIn[playersStayedIn.Count - 1].Action = "Winner";

            // highlight the round winner at the poker table
            playersStayedIn[playersStayedIn.Count - 1].PlayersTurn = true;

            string result = playersStayedIn[playersStayedIn.Count - 1].Name + " wins";
            if (ActivePlayers.Count > 1)
            {
                PokerHand ph = playersStayedIn[playersStayedIn.Count - 1].pHand;
                result += " with " +
                PokerRules.GetPokerHandAsText(playersStayedIn[playersStayedIn.Count - 1].pHand);

                

                List<bool> highlightedcards = PokerRules.getHighlightedCards(playersStayedIn[playersStayedIn.Count - 1].Hand,
                    TableHand, playersStayedIn[playersStayedIn.Count - 1].pHand);

                playersStayedIn[playersStayedIn.Count - 1].CardOneHighlight = highlightedcards[5];
                playersStayedIn[playersStayedIn.Count - 1].CardTwoHighlight = highlightedcards[6];
                highlightedcards.RemoveRange(5, 2);
                TableHandHighlight = highlightedcards;
            }

            VictoryText = result;


            List<Player> playersNotGaveUp = new List<Player>();
            foreach (Player p in players)
            {
                if (p.Chips > 0)
                {
                    playersWithChipsLeft.Add(p);
                }

                if(p.Action.Equals("giveup") == false)
                {
                    playersNotGaveUp.Add(p);

                } 

            }
            
            if(playersWithChipsLeft.Count == 1)
            {

                result += " - "+playersWithChipsLeft[0].Name + " has won the match!";
                VictoryText = playersWithChipsLeft[0].Name + " has won the match!";
                WinnerPlayer = playersWithChipsLeft[0].Name;
                GameEnded = true;
            }

            if (playersNotGaveUp.Count == 1)
            {

                result += " - " + playersNotGaveUp[0].Name + " has won the match!";
                VictoryText = playersNotGaveUp[0].Name + " has won the match!";
                WinnerPlayer = playersNotGaveUp[0].Name;
                GameEnded = true;
            }

            return result;

        }

        public int GetCurrentPlayerCallValue()
        {
            int callValue = 0;

            callValue = raiseValue - ActivePlayers[CurrentPlayerPos].RaiseValue;

            return callValue;
        }

        public void RemovePlayer(string name)
        {
            Player p = players.Find(player => player.Name == name);
            int playerChips = p.Chips;
            p.Chips = 0;
            

            if(playerChips > 0 && players.Count != 0)
            {
                int playerChipRemainder = playerChips % players.Count;
                if (playerChipRemainder !=0 )
                {
                    
                    playerChips -= playerChipRemainder;
                    players[0].Chips += playerChipRemainder;
                }

                foreach(Player player in players)
                {
                    player.Chips += playerChips;
                }
            }
        }

        public void RemovePlayer(Player p)
        {
            players.Remove(p);
        }



    }
}
