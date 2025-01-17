using MonsterTradingCardsGameLibrary.Enums;
using System.Text.Json.Serialization;

namespace MonsterTradingCardsGameLibrary.Models
{
    public class Card
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public double Damage { get; set; }

        public string CardType { get; set; }

        public ElementType? ElementType { get; set; }

        public Card()
        {
            
        }

        protected Card(string id, string name, int damage, ElementType? elementType)
        {
            Id = id;
            Name = name;
            Damage = damage;
            ElementType = elementType;
        }

        public bool IsAffectedBySpecialRuleAgainst(Card opponent)
        {
            string ownName = Name ?? string.Empty;
            string opponentName = opponent.Name ?? string.Empty;
            string opponentCardType = opponent.CardType ?? string.Empty;
            string opponentElement = opponent.ElementType?.ToString() ?? string.Empty;

            if (ownName.Contains("Goblin") && opponentName.Contains("Dragon"))
                return true; 

            if (ownName.Contains("Wizard") && opponentName.Contains("Ork"))
                return true;

            if (ownName.Contains("Knight") && opponentCardType == "Spell" && opponentElement == "Water")
                return true;

            if (ownName.Contains("Kraken") && opponentCardType == "Spell")
                return true;

            if (ownName.Contains("FireElf") && opponentName.Contains("Dragon"))
                return false;

            return false;
        }

        public double GetEffectiveDamage(Card opponent)
        {
            if (ElementType.HasValue && opponent.ElementType.HasValue)
            {
                if ((ElementType == Enums.ElementType.Water && opponent.ElementType == Enums.ElementType.Fire) ||
                    (ElementType == Enums.ElementType.Fire && opponent.ElementType == Enums.ElementType.Normal) ||
                    (ElementType == Enums.ElementType.Normal && opponent.ElementType == Enums.ElementType.Water))
                {
                    return Damage * 2;
                }
                else if ((ElementType == Enums.ElementType.Fire && opponent.ElementType == Enums.ElementType.Water) ||
                         (ElementType == Enums.ElementType.Normal && opponent.ElementType == Enums.ElementType.Fire) ||
                         (ElementType == Enums.ElementType.Water && opponent.ElementType == Enums.ElementType.Normal))
                {
                    return Damage / 2;
                }
            }

            return Damage;
        }
    }
}