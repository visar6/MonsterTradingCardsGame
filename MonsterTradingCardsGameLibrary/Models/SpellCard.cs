using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGameLibrary.Enums;

namespace MonsterTradingCardsGameLibrary.Models
{
    public sealed class SpellCard : Card
    {
        public SpellCard(int id, string name, int damage, ElementType elementType) : base(id, name, damage, elementType) { }
    }
}