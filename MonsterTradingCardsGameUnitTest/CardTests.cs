using MonsterTradingCardsGameLibrary.Enums;
using MonsterTradingCardsGameLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameUnitTest
{
    [TestClass]
    public class CardTests
    {
        [TestMethod]
        public void IsAffectedBySpecialRuleAgainst_GoblinVsDragon_ReturnsTrue()
        {
            var goblinCard = new Card { Name = "Goblin" };
            var dragonCard = new Card { Name = "Dragon" };

            bool result = goblinCard.IsAffectedBySpecialRuleAgainst(dragonCard);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsAffectedBySpecialRuleAgainst_WizardVsOrk_ReturnsTrue()
        {
            var wizardCard = new Card { Name = "Wizard" };
            var orkCard = new Card { Name = "Ork" };

            bool result = wizardCard.IsAffectedBySpecialRuleAgainst(orkCard);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsAffectedBySpecialRuleAgainst_KnightVsWaterSpell_ReturnsTrue()
        {
            var knightCard = new Card { Name = "Knight", CardType = "Monster" };
            var waterSpellCard = new Card { CardType = "Spell", ElementType = ElementType.Water };

            bool result = knightCard.IsAffectedBySpecialRuleAgainst(waterSpellCard);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetEffectiveDamage_WaterVsFire_ReturnsDoubleDamage()
        {
            var waterCard = new Card { Damage = 50, ElementType = ElementType.Water };
            var fireCard = new Card { ElementType = ElementType.Fire };

            double effectiveDamage = waterCard.GetEffectiveDamage(fireCard);

            Assert.AreEqual(100, effectiveDamage);
        }

        [TestMethod]
        public void GetEffectiveDamage_FireVsWater_ReturnsHalfDamage()
        {
            var fireCard = new Card { Damage = 50, ElementType = ElementType.Fire };
            var waterCard = new Card { ElementType = ElementType.Water };
            
            double effectiveDamage = fireCard.GetEffectiveDamage(waterCard);

            Assert.AreEqual(25, effectiveDamage);
        }
    }

}
