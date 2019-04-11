using Microsoft.AspNetCore.SignalR;
using PokerWebApplication.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerWebApplication.Game
{
    public class Table
    {
        public int id { get; set; }
        public List<Player> Players= new List<Player>();
        public GameManager Game { get; private set; }

        public Table()
        {

        }

        public int AddNewPlayer(string name, string connectionId)
        {
            int tablePosition = Players.Count;
            Player p = new Player
            {
                Name = name,
                TablePosition = tablePosition,
                Chips = 500,
                ConnectionId = connectionId
            };

            Players.Add(p);
            return tablePosition;


        }

        public List<Player> GetPlayers()
        {
            return Players;

        }

        public int NumberOfPlayers()
        {
            return Players.Count();
        }

        public void InitGame()
        {
            Game = new GameManager(this.Players, this.id);
        }

        public async Task startGameAsync()
        {
            await Game.StartGameAsync();
        }

        

        


        public async Task NextPlayerActionAsync(string action)
        {
            await Game.NextPlayerActionAsync(action);
        }




    }
}
