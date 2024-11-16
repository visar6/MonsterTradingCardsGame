using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardsGameLibrary.Enums;

namespace MonsterTradingCardsGameLibrary.Models
{
    public sealed class MonsterCard : Card
    {
        public MonsterCard(int id, string name, int damage, ElementType elementType) : base(id, name, damage, elementType) { }
    }
}