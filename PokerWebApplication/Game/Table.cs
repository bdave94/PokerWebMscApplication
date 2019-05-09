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
        public bool Full { get; set; }

        public Table()
        {
            Game = new GameManager();
            Full = false;
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
            
            Game.Init(this.Players, this.id);
        }

        public void RemovePlayer(string name)
        {
            Player p = Players.Find(player => player.Name == name);
            Game.RemovePlayer(p);
            Players.Remove(p);
        }

        public void NewRound()
        {

            Game.StartNextRound();
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Action.Equals("giveup"))
                {
                    Players.Remove(Players[i]);
                    i -= 1;
                }

            }
        }

        public void Reset()
        {
            Game.Clear();
            Players.Clear();
            Full = false;
        }
    }
}
