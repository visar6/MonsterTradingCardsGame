using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using MonsterTradingCardsGameLibrary.Enums;

namespace MonsterTradingCardsGameLibrary.Models
{
    public class Battle
    {
        public string Id { get; private set; }

        public string UserId1 { get; private set; }

        public string UserId2 { get; private set; }

        public string? WinnerId { get; private set; } 

        public List<string> Log { get; private set; } 

        private const int MaxRounds = 100;

        public Battle(string userId1, string userId2)
        {
            Id = Guid.NewGuid().ToString();
            UserId1 = userId1;
            UserId2 = userId2;
            Log = new List<string>();
        }

        public void StartBattle(User player1, User player2)
        {
            Log.Add($"Battle started between {player1.Username} and {player2.Username}.");

            int round = 0;
            while (round < MaxRounds && player1.Deck?.Cards?.Count > 0 && player2.Deck?.Cards?.Count > 0)
            {
                round++;
                Log.Add($"--- Round {round} ---");

                var card1 = player1.Deck.Cards.OrderBy(_ => Guid.NewGuid()).First();
                var card2 = player2.Deck.Cards.OrderBy(_ => Guid.NewGuid()).First();

                var winner = FightRound(player1, card1, player2, card2);

                if (winner == player1)
                {
                    player1.Deck.Cards.Add(card2);
                    player2.Deck.Cards.Remove(card2);
                    Log.Add($"{player1.Username} gewinnt die Runde mit {card1.Name} ({card1.Damage}) gegen {card2.Name} ({card2.Damage}).");
                }
                else if (winner == player2)
                {
                    player2.Deck.Cards.Add(card1);
                    player1.Deck.Cards.Remove(card1);
                    Log.Add($"{player2.Username} gewinnt die Runde mit {card2.Name} ({card2.Damage}) gegen {card1.Name} ({card1.Damage}).");
                }
                else
                {
                    Log.Add($"Die Runde endet unentschieden: {card1.Name} vs {card2.Name}.");
                }
            }

            FinishBattle(player1, player2);
        }

        private User? FightRound(User player1, Card card1, User player2, Card card2)
        {
            bool isMonster1 = card1.CardType?.Equals("Monster", StringComparison.OrdinalIgnoreCase) ?? false;
            bool isMonster2 = card2.CardType?.Equals("Monster", StringComparison.OrdinalIgnoreCase) ?? false;

            if (card1.IsAffectedBySpecialRuleAgainst(card2))
                return player2;
            if (card2.IsAffectedBySpecialRuleAgainst(card1))
                return player1;

            if (isMonster1 && isMonster2)
                return card1.Damage > card2.Damage ? player1 : card1.Damage < card2.Damage ? player2 : null;

            double damage1 = card1.GetEffectiveDamage(card2);
            double damage2 = card2.GetEffectiveDamage(card1);

            return damage1 > damage2 ? player1 : damage1 < damage2 ? player2 : null;
        }



        private void FinishBattle(User player1, User player2)
        {
            if (player1.Deck?.Cards?.Count == 0)
            {
                WinnerId = player2.Id;
                Log.Add($"{player2.Username} gewinnt das Battle.");
                UpdateElo(player2, player1);
            }
            else if (player2.Deck?.Cards?.Count == 0)
            {
                WinnerId = player1.Id;
                Log.Add($"{player1.Username} gewinnt das Battle.");
                UpdateElo(player1, player2);
            }
            else
            {
                Log.Add("Battle endet nach 100 Runden ohne Sieger.");
            }
        }


        private void UpdateElo(User winner, User loser)
        {
            int eloGain = 10;
            int eloLoss = 10;

            winner.Stats.Elo += eloGain;
            loser.Stats.Elo -= eloLoss;

            winner.Stats.Wins++;
            loser.Stats.Losses++;

            Log.Add($"{winner.Username} gewinnt {eloGain} ELO-Punkte. Neuer ELO: {winner.Stats.Elo}.");
            Log.Add($"{loser.Username} verliert {eloLoss} ELO-Punkte. Neuer ELO: {loser.Stats.Elo}.");
        }
    }
}