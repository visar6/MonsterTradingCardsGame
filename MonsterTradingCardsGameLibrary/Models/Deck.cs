using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameLibrary.Models
{
    public class Deck
    {
        public int Id { get; protected set; }

        public int UserId { get; protected set; }

        public List<Card> Cards { get; protected set; }
    }
}