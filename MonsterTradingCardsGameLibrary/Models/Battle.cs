using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameLibrary.Models
{
    public class Battle
    {
        public int Id { get; protected set; }

        public int UserId1 { get; protected set; }

        public int UserId2 { get; protected set; }

        public int WinnerId { get; protected set; }
    }
}