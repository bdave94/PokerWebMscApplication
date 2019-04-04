using Microsoft.AspNetCore.SignalR;
using PokerWebApplication.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PokerWebApplication.Hubs
{
    public class GameHub:Hub
    {
        public static List<Table> Tables = new List<Table>();
        private const int NumberOfSeats = 3;

        public static List<string> Connections = new List<string>();


        public async Task AddPlayer(string username)
        {
            if (Tables.Count == 0)
            {
                Tables.Add(new Table { id = 1 });
            }


            int tableMostPlayerIndex = FindTableWithMostPlayers();
            
            string message; 
           
            if(tableMostPlayerIndex != -1)
            {
                Tables[tableMostPlayerIndex].AddNewPlayer(username, Context.ConnectionId);
               
                message = Tables[tableMostPlayerIndex].id.ToString();
            } else
            {
                message = "full";
            }
            await Clients.Caller.SendAsync("queueResult", message);

            if(tableMostPlayerIndex != -1)
            {
                await GetTableInfo(Tables[tableMostPlayerIndex].id);
                if (Tables[tableMostPlayerIndex].Players.Count == NumberOfSeats)
                {   

                    await StartGameAsync(Tables[tableMostPlayerIndex]);
                }
            }
            
        }
        

        public async Task StartGameAsync(Table table)
        {
            
            GameManager g = new GameManager(table.Players, this, table.id);
            await g.StartGameAsync();
        }

        public async Task DeactivateAllIndicatorAsync(int tableId)
        {
            await Clients.All.SendAsync("DeactivateAllIndicator");

        }

        public async Task GetTableInfo(int tableId )
        {
            List<Player> players = GetTablebyId(tableId).GetPlayers();
           
           
            await Clients.All.SendAsync("getInfo", players);
        }

        

        public async Task NewMessage(string username, string message)
        {
            await Clients.All.SendAsync("messageReceived", username, message);
        }

        private Table GetTablebyId(int tableID)
        {
            return Tables.SingleOrDefault(t => t.id == tableID);
        }


        private int FindTableWithMostPlayers()
        {
            int tableMostPlayerIndex = -1;
            int tableHighestPlayers = -1;
            for (int i = 0; i < Tables.Count; i++)
            {
                if (Tables[i].NumberOfPlayers() > tableHighestPlayers &&
                    Tables[i].NumberOfPlayers() != NumberOfSeats)
                {
                    tableHighestPlayers = Tables[i].NumberOfPlayers();
                    tableMostPlayerIndex = i;
                }

            }

            return tableMostPlayerIndex;

        }

        public void ActivateBigBlindIndicator(string playerName)
        {
            Clients.All.SendAsync("ActivateBigBlindIndicator", playerName);
        }

        public void ActivateSmallBlindIndicator(string playerName)
        {
            Clients.All.SendAsync("ActivateSmallBlindLabel", playerName);
        }

        public void ActivateDealerIndicator(string playerName)
        {
            Clients.All.SendAsync("ActivateDealerIndicator", playerName);
            
        }

        public void ShowPlayerCards(string connectionId, List<Card> hand)
        {
            Clients.Client(connectionId).SendAsync("ShowPlayerCards", hand);
                
        }

        public async Task PlayerCall(int playerPosition, int tableID, int callValue)
        {
            GetTablebyId(tableID).Players[playerPosition].CallValue = callValue;

            await Clients.Caller.SendAsync("PlayerCall", "callMessageRecieved");

        }

        public void ResetClientCardPictures()
        {
            Clients.All.SendAsync("ResetClientCardPictures");
        }
    }
}
