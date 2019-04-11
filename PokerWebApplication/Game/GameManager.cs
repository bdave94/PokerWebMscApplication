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

       

        private List<Card> TableHand = new List<Card>();

      

        private Deck d = new Deck();

        private int dealerNum = 0;

        

        private int tableId;


        public List<Player> activePlayers = new List<Player>();

        public string Dealer { get; set; }
        public string SmallBlind { get; set; }
        public string BigBlind { get; set; }

        public int GamePhase { get; set; }


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
        }


        public void StartNextRound()
        {
            ReadyForNextPhase = false;
            ReadyForNextRound = false;

            d.ResetDeck();
            dealerNum += 1;

            if (dealerNum >= activePlayers.Count)
                dealerNum = 0;

            activePlayers.Clear();
            for (int z = 0; z < players.Count; z++)
            {
                activePlayers.Add(players[z]);

                activePlayers[z].ClearHand();
                activePlayers[z].SmallBlind = false;
                activePlayers[z].BigBlind = false;
                activePlayers[z].Dealer = false;
                activePlayers[z].PlayersTurn = false;
            }

            //clear table hand
            TableHand.Clear();
        }






        public async Task StartGameAsync()
        {
            
            

            //play rounds of poker
            for (int i = 0; i < 3; i++)
            {

                /* hub.ResetClientCardPictures();
                
                 */
            }

        }

        internal Task NextPlayerActionAsync(string action)
        {
            throw new NotImplementedException();
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
                activePlayers[j].AddCardHand(d.DrawCard());
                activePlayers[j].AddCardHand(d.DrawCard());

              
                    
           }


           for (int j = 0; j < dealerNum+1; j++)
           {
                activePlayers[j].AddCardHand(d.DrawCard());
                activePlayers[j].AddCardHand(d.DrawCard());

                
           }

            askFirstPlayerInPhase = true;
        }


        //ask if players want to play with their cards
        public void GetRemainingPlayersFlop(int dealerNum, List<Player> activePlayers)
        {
            for (int i = dealerNum+1; i < activePlayers.Count; i++)
            {
                activePlayers[i].Call();

            }

            for (int i = 0; i < dealerNum + 1; i++)
            {

                activePlayers[i].Call();
            }

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
                   
                    CurrentPlayer = activePlayers[nextPlayerPosition].Name;
                    activePlayers[nextPlayerPosition].PlayersTurn = true;
                    askFirstPlayerInPhase = true;

                    //ReadyForNextPhase = true;
                    ReadyForNextRound = true;

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

            if(playerAction.Equals("Call"))
            {

            }

        }


        private void StartNextPhase()
        {
           


        }


        private void FinishRound()
        {



        }



    }
}
