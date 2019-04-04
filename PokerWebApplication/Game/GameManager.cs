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
        private List<Player> players = new List<Player>();

       

        private List<Card> TableHand = new List<Card>();

      

        private Deck d = new Deck();

        private int dealerNum = 0;

        private GameHub hub;

        private int tableId;

        public GameManager(List<Player> PlayerList, GameHub hub, int tableID )
        {
            players = PlayerList;
            this.hub = hub;
            tableId = tableID;
        }

        public async Task StartGameAsync()
        {
            dealerNum = 0;
            List<Player> activePlayers = new List<Player>();

            //play rounds of poker
            for (int i = 0; i < 3; i++)
            {


                if (dealerNum >= activePlayers.Count)
                    dealerNum = 0;


                activePlayers.Clear();
                for (int z = 0; z < players.Count; z++)
                {
                    activePlayers.Add(players[z]);

                    activePlayers[z].ClearHand();
                    activePlayers[z].SmallBlind = false;
                    activePlayers[z].BigBlind = false;
                }

                //reset blinds and dealer label backcolor
                await hub.DeactivateAllIndicatorAsync(tableId);

                //clear table hand
                TableHand.Clear();


                //get small blinds big blinds
                GetBlinds(dealerNum, activePlayers);
                //give two cards for players
                GiveCards(dealerNum, activePlayers);

               

                //ask if players want to play with their cards
                GetRemainingPlayersFlop(dealerNum, activePlayers);

                hub.ResetClientCardPictures();
                d.ResetDeck();

                dealerNum += 1;
            }

        }

       

        private void GetBlinds(int dealerNum, List<Player> activePlayers)
        {
            if (dealerNum < activePlayers.Count - 2)
            {
                activePlayers[dealerNum].Dealer = true;
                hub.ActivateDealerIndicator(activePlayers[dealerNum ].Name);


                activePlayers[dealerNum + 1].SmallBlind = true;

                hub.ActivateSmallBlindIndicator(activePlayers[dealerNum + 1].Name);

           

                activePlayers[dealerNum + 2].BigBlind = true;
                hub.ActivateBigBlindIndicator(activePlayers[dealerNum + 2].Name);

            }

            if (dealerNum == activePlayers.Count - 2)
            {
                activePlayers[dealerNum].Dealer = true;
                hub.ActivateDealerIndicator(activePlayers[dealerNum].Name);

                activePlayers[dealerNum + 1].SmallBlind = true;
                hub.ActivateSmallBlindIndicator(activePlayers[dealerNum + 1].Name);


                activePlayers[0].BigBlind = true;
                hub.ActivateBigBlindIndicator(activePlayers[0].Name);
               
            }

            if (dealerNum == activePlayers.Count - 1)
            {
                activePlayers[dealerNum].Dealer = true;
                hub.ActivateDealerIndicator(activePlayers[dealerNum].Name);

                activePlayers[0].SmallBlind = true;
                hub.ActivateSmallBlindIndicator(activePlayers[0].Name);

                activePlayers[1].BigBlind = true;
                hub.ActivateBigBlindIndicator(activePlayers[1].Name);

            }
        }

        private void GiveCards(int dealerNum, List<Player> activePlayers)
        {
           
           for (int j = dealerNum+1; j < activePlayers.Count; j++)
           {
                activePlayers[j].AddCardHand(d.DrawCard());
                activePlayers[j].AddCardHand(d.DrawCard());

                hub.ShowPlayerCards(activePlayers[j].ConnectionId, activePlayers[j].Hand);
                    
           }


           for (int j = 0; j < dealerNum+1; j++)
           {
                activePlayers[j].AddCardHand(d.DrawCard());
                activePlayers[j].AddCardHand(d.DrawCard());

                hub.ShowPlayerCards(activePlayers[j].ConnectionId, activePlayers[j].Hand);
           }
            
        }



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

    }
}
