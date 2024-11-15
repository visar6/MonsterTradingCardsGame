using MonsterTradingCardsGameLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameLibrary
{
    public class Package
    {
        public int Id { get; set; }

        public List<Card> Cards { get; set; }

        public Package(int id, List<Card> cards)
        {
            Id = id;
            Cards = cards;
        }
    }
}
