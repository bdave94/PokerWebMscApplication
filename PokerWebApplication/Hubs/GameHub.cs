using Microsoft.AspNetCore.SignalR;
using PokerWebApplication.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerWebApplication.Hubs
{
    public class GameHub:Hub
    {
        public static List<Table> Tables = new List<Table>();
        private const int NumberOfSeats = 2;
        public GameHub()
        {
            Table t = new Table();
            t.id = 1;
            Tables.Add(t);
        }

        public async Task AddPlayer(string username)
        {
            //await Clients.All.SendAsync("messageReceived", message);
            int tableMostPlayerIndex = -1;
            int tableHighestPlayers = -1;
            for (int i = 0; i < Tables.Count; i++) 
            {
              

                if(Tables[i].NumberOfPlayers() > tableHighestPlayers &&
                    Tables[i].NumberOfPlayers() != NumberOfSeats)
                {
                    tableHighestPlayers = Tables[i].NumberOfPlayers();
                    tableMostPlayerIndex = i;
                }


            }

            Tables[tableMostPlayerIndex].AddNewPlayer(username);
           
            string message = Tables[tableMostPlayerIndex].id.ToString();
            await Clients.Caller.SendAsync("queueResult", message);
        }

        public async Task GetTableInfo(string tableId )
        {
            List<Player> players = getTablebyId(tableId).GetPlayers();
           
           
            await Clients.All.SendAsync("getInfo", players);
        }

        public async Task NewMessage(string username, string message)
        {
            await Clients.All.SendAsync("messageReceived", username, message);
        }

        private Table getTablebyId(string tableID)
        {
            int id = int.Parse(tableID);
            Table result = null;
            foreach(Table t in Tables)
            {
                if (t.id == id)
                {
                    result = t;
                    break;
                }
                   
            }
            return result;

        }

    }
}
