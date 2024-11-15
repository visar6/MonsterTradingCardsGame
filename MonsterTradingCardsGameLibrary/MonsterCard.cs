using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameLibrary
{
    public sealed class MonsterCard : Card
    {
        public MonsterCard(int id, string name, int damage, ElementType elementType) : base(id, name, damage, elementType)
        {
        }
    
        
    }
}
