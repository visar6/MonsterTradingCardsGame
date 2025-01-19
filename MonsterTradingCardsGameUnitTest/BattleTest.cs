using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonsterTradingCardsGameLibrary.Models;
using MonsterTradingCardsGameLibrary.Enums;
using System.Collections.Generic;
using System.Linq;

namespace MonsterTradingCardsGameUnitTest
{
    [TestClass]
    public class BattleTests
    {
        [TestMethod]
        public void FightRound_MonsterVsMonster_HigherDamageWins()
        {
            var player1 = new User { Id = "1", Username = "Player1", Deck = new Deck() };
            var player2 = new User { Id = "2", Username = "Player2", Deck = new Deck() };

            var card1 = new Card { Name = "Dragon", ElementType = ElementType.Fire, Damage = 60, CardType = "Monster" };
            var card2 = new Card { Name = "Goblin", ElementType = ElementType.Normal, Damage = 50, CardType = "Monster" };

            var battle = new Battle(player1.Id, player2.Id);

            var winner = battle.FightRound(player1, card1, player2, card2);

            Assert.AreEqual(player1, winner);
        }

        [TestMethod]
        public void FightRound_SpellVsSpell_ElementAdvantageWins()
        {
            var player1 = new User { Id = "1", Username = "Player1" };
            var player2 = new User { Id = "2", Username = "Player2" };

            var card1 = new Card { Name = "Fireball", ElementType = ElementType.Fire, Damage = 50, CardType = "Spell" };
            var card2 = new Card { Name = "WaterSplash", ElementType = ElementType.Water, Damage = 50, CardType = "Spell" };

            var battle = new Battle(player1.Id, player2.Id);

            var winner = battle.FightRound(player1, card1, player2, card2);

            Assert.AreEqual(player2, winner);
        }

        [TestMethod]
        public void StartBattle_ShouldDetermineWinner()
        {
            var player1 = new User { Id = "1", Username = "Player1", Deck = new Deck() };
            var player2 = new User { Id = "2", Username = "Player2", Deck = new Deck() };

            player1.Deck.Cards = new List<Card> { new Card { Name = "FireDragon", ElementType = ElementType.Fire, Damage = 100, CardType = "Monster" } };
            player2.Deck.Cards = new List<Card> { new Card { Name = "WaterGoblin", ElementType = ElementType.Water, Damage = 20, CardType = "Monster" } };

            var battle = new Battle(player1.Id, player2.Id);
            battle.StartBattle(player1, player2);

            Assert.AreEqual(player1.Id, battle.WinnerId);
        }

        [TestMethod]
        public void FinishBattle_ShouldSetWinnerCorrectly()
        {
            var player1 = new User { Id = "1", Username = "Player1", Deck = new Deck() };
            var player2 = new User { Id = "2", Username = "Player2", Deck = new Deck() };

            player1.Deck.Cards = new List<Card>();
            player2.Deck.Cards = new List<Card> { new Card { Name = "WaterGoblin", ElementType = ElementType.Water, Damage = 20, CardType = "Monster" } };

            var battle = new Battle(player1.Id, player2.Id);
            battle.FinishBattle(player1, player2);

            Assert.AreEqual(player2.Id, battle.WinnerId);
        }

        [TestMethod]
        public void UpdateElo_ShouldChangeEloCorrectly()
        {
            var winner = new User { Id = "1", Username = "Winner", Stats = new Stats { Elo = 1000, Wins = 0 } };
            var loser = new User { Id = "2", Username = "Loser", Stats = new Stats { Elo = 1000, Losses = 0 } };

            var battle = new Battle(winner.Id, loser.Id);
            battle.UpdateElo(winner, loser);

            Assert.AreEqual(1010, winner.Stats.Elo);
            Assert.AreEqual(990, loser.Stats.Elo);
            Assert.AreEqual(1, winner.Stats.Wins);
            Assert.AreEqual(1, loser.Stats.Losses);
        }
    }
}
