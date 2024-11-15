using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameLibrary
{
    public class Deck
    {
        public int Id { get; protected set; }

        public int UserId { get; protected set; }

        public List<Card> Cards { get; protected set; }

        public Deck(List<Card> cards)
        {
            Cards = cards;
        }

        public Deck(int id, int userId, List<Card> cards)
        {
            Id = id;
            UserId = userId;
            Cards = cards;
        }
    }
}
