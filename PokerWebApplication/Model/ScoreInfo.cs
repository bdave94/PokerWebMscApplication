using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerWebApplication.Model
{
    public class ScoreInfo :IComparer<ScoreInfo>
    {
        public string Name { get; set; }
        public int Chips { get; set; }

        public int Compare(ScoreInfo first, ScoreInfo second)
        {
            if (first.Chips > second.Chips)
                return 1;

            if (first.Chips < second.Chips)
                return -1;

            return 0;
        }
    }
}
