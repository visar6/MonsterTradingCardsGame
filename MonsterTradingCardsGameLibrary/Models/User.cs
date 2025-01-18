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
        public string? Id { get; set; }

        public string? Username { get; set; }

        public string? Password { get;  set; }
        
        public string? Name { get; set; }

        public string? Bio { get; set; }

        public string? Image { get; set; }

        public int Coins { get;  set; }

        public Stack? Stack { get;  set; } = new Stack();

        public Deck? Deck { get;  set; } = new Deck();

        public Stats? Stats { get; set; } = new Stats();

        public User()
        {
            
        }
    }
}