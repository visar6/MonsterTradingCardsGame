using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameLibrary.Models
{
    public class User
    {
        public int Id { get; protected set; }

        public string Username { get; protected set; }

        public string Password { get; protected set; }

        public int Coins { get; protected set; }

        public Stack Stack { get; protected set; }

        public Deck Deck { get; protected set; }

        public Stats Stats { get; protected set; }
    }
}