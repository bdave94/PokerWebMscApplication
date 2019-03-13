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

        public void AddNewPlayer(string name)
        {
            Player p = new Player();
            p.name = name;
            p.tablePosition = Players.Count;
            p.chips = 500;
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
