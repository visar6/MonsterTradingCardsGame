using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameLibrary.Models
{
    public class Stack
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public List<Card> Cards { get; set; } = new List<Card>();
    }
}