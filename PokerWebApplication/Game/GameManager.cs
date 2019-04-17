using Microsoft.AspNetCore.SignalR;
using PokerWebApplication.Hubs;
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

        

        private int tableId;


        public List<Player> activePlayers = new List<Player>();

        public string Dealer { get; set; }
        public string SmallBlind { get; set; }
        public string BigBlind { get; set; }

        public int GamePhase { get; set; }
        public string GamePhaseName { get; set; }

        public string CurrentPlayer { get; set; }
        public int CurrentPlayerPos { get; private set; }

        private int nextPlayerPosition;

        private bool askFirstPlayerInPhase;

        public bool ReadyForNextRound { get; private set; }

        public bool ReadyForNextPhase { get; private set; }

        public GameManager(List<Player> PlayerList,  int tableID)
        {
           

            players = PlayerList;
           
            tableId = tableID;

            DeactivateAllIndicatorRequired = false;

            dealerNum = 0;

            ReadyForNextPhase = false;
            ReadyForNextRound = false;

            GamePhase = 0;
        }


        public void StartNextRound()
        {
            ReadyForNextPhase = false;
            ReadyForNextRound = false;
            GamePhase = 0;

            d.ResetDeck();
            dealerNum += 1;

            if (dealerNum >= activePlayers.Count)
                dealerNum = 0;

            activePlayers.Clear();
            for (int z = 0; z < players.Count; z++)
            {
                activePlayers.Add(players[z]);

                activePlayers[z].Hand.Clear();
                activePlayers[z].SmallBlind = false;
                activePlayers[z].BigBlind = false;
                activePlayers[z].Dealer = false;
                activePlayers[z].PlayersTurn = false;


                activePlayers[z].PublicHand.Clear();
                activePlayers[z].PublicHand.Add(new Card("back", 0));
                activePlayers[z].PublicHand.Add(new Card("back", 0));

                activePlayers[z].Action = "";

            }

            //clear table hand
            TableHand.Clear();
        }



       

        public void GetBlinds()
        {
            if (dealerNum < activePlayers.Count - 2)
            {
                activePlayers[dealerNum].Dealer = true;
               


                activePlayers[dealerNum + 1].SmallBlind = true;

                SmallBlind = activePlayers[dealerNum + 1].Name;

           

                activePlayers[dealerNum + 2].BigBlind = true;
                BigBlind = activePlayers[dealerNum + 2].Name;

            }

            if (dealerNum == activePlayers.Count - 2)
            {
                activePlayers[dealerNum].Dealer = true;
                Dealer = activePlayers[dealerNum].Name;

                activePlayers[dealerNum + 1].SmallBlind = true;
                SmallBlind = activePlayers[dealerNum + 1].Name;


                activePlayers[0].BigBlind = true;
                BigBlind = activePlayers[0].Name;
               
            }

            if (dealerNum == activePlayers.Count - 1)
            {
                activePlayers[dealerNum].Dealer = true;
                Dealer = activePlayers[dealerNum].Name;

                activePlayers[0].SmallBlind = true;
                SmallBlind = activePlayers[0].Name;

                activePlayers[1].BigBlind = true;
                BigBlind = activePlayers[1].Name;

            }
        }

        public void GiveCards()
        {
           
           for (int j = dealerNum+1; j < activePlayers.Count; j++)
           {
                activePlayers[j].Hand.Add(d.DrawCard());
                activePlayers[j].Hand.Add(d.DrawCard());

              
                    
           }


           for (int j = 0; j < dealerNum+1; j++)
           {
                activePlayers[j].Hand.Add(d.DrawCard());
                activePlayers[j].Hand.Add(d.DrawCard());

                
           }

            askFirstPlayerInPhase = true;
        }


       


        public void GetCurrentPlayerAction()
        {

            foreach(Player p in activePlayers)
            {
                p.PlayersTurn = false;
            }

            if (askFirstPlayerInPhase)
            {
                askFirstPlayerInPhase = false;
                if(dealerNum + 1 < activePlayers.Count)
                {
                   
                    CurrentPlayer = activePlayers[dealerNum + 1].Name;

                    activePlayers[dealerNum + 1].PlayersTurn = true;

                    CurrentPlayerPos = dealerNum + 1;
                    nextPlayerPosition = dealerNum + 2;

                    if (nextPlayerPosition == activePlayers.Count)
                    {
                        nextPlayerPosition = 0;
                    }

                }  else
                {
                    nextPlayerPosition = 1;
                    CurrentPlayer = activePlayers[0].Name;
                    activePlayers[0].PlayersTurn = true;
                    CurrentPlayerPos = 0;
                }
              

            } else
            {
                if(nextPlayerPosition == dealerNum)
                {
                    CurrentPlayerPos = nextPlayerPosition;
                    CurrentPlayer = activePlayers[nextPlayerPosition].Name;
                    activePlayers[nextPlayerPosition].PlayersTurn = true;
                    askFirstPlayerInPhase = true;

                    ReadyForNextPhase = true;
                   
                    GamePhase += 1;
                    if(GamePhase == 4 )
                    {
                        ReadyForNextRound = true;
                    }

                } else
                {

                    CurrentPlayerPos = nextPlayerPosition;
                    CurrentPlayer = activePlayers[nextPlayerPosition].Name;
                    activePlayers[nextPlayerPosition].PlayersTurn = true;
                    nextPlayerPosition += 1;

                    if (nextPlayerPosition == activePlayers.Count)
                    {
                        nextPlayerPosition = 0;
                    }
                    
                }
 
            }

        }

      

        public void ProcessPlayerAction(string playerAction)
        {

            if(playerAction.Equals("Called"))
            {
                activePlayers[CurrentPlayerPos].Action = "Call";
            }

        }


        public void StartNextPhase()
        {
            foreach(Player p in activePlayers)
            {
                p.Action = "";
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

        public void FinishRound()
        {
            foreach(Player p in activePlayers)
            {
               
                p.PublicHand[0].Suit = p.Hand[0].Suit;
                p.PublicHand[0].Rank = p.Hand[0].Rank;

                p.PublicHand[1].Suit = p.Hand[1].Suit;
                p.PublicHand[1].Rank = p.Hand[1].Rank;

            }
           


        }







    }
}
