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

        public Table()
        {

        }

        public void AddNewPlayer(string name, string connectionId)
        {
            Player p = new Player();
            p.Name = name;
            p.TablePosition = Players.Count;
            p.Chips = 500;
            p.ConnectionId = connectionId;
            Players.Add(p);

        }

        public List<Player> GetPlayers()
        {
            return Players;

        }

        public int NumberOfPlayers()
        {
            return Players.Count();
        }
    }
}
