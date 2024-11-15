using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameLibrary
{
    public sealed class SpellCard : Card
    {
        public SpellCard(int id, string name, int damage, ElementType elementType) : base(id, name, damage, elementType)
        {
        }
    }
}
