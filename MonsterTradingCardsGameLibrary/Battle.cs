using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameLibrary
{
    public class Battle
    {
        public int Id { get; protected set; }

        public int UserId1 { get; protected set; }
        
        public int UserId2 { get; protected set; }
        
        public int WinnerId { get; protected set; }

        public Battle(int id, int userId1, int userId2, int winnerId)
        {
            Id = id;
            UserId1 = userId1;
            UserId2 = userId2;
            WinnerId = winnerId;
        }
    }
}
