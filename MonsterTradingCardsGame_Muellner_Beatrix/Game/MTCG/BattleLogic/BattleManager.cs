using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MTCG.Database;
using MTCG.Cards;

namespace MTCG.BattleLogic {

    public class Battle_Functions {

        public DB_Functions DB_Functions { get; set; }

        const int K_FACTOR = 30;

        public UserData playerOne;
        public UserData playerTwo;
        public List<CardData> deckPlayerOne;
        public List<CardData> deckPlayerTwo;
        public List<string> gameLogic;
        public Random randomGenerator;

        public Battle_Functions(UserData playerOne, UserData playerTwo, List<CardData> deck1, List<CardData> deck2, DB_Functions dbFunctions) {

            this.DB_Functions = dbFunctions;
            this.playerOne = playerOne;
            this.playerTwo = playerTwo;
            this.deckPlayerOne = new List<CardData>(deck1);
            this.deckPlayerTwo = new List<CardData>(deck2);
            this.gameLogic = new List<string>();
            this.randomGenerator = new Random();
        }

        public async Task StartGameBattle() {

            int battleRound = 0;
            bool battleConcluded = false;

            while (CanContinueBattle(battleRound, battleConcluded)) {

                battleRound++;
                LogRoundStart(battleRound);

                CardData challengerCard = SelectRandomCard(deckPlayerOne);
                CardData defenderCard = SelectRandomCard(deckPlayerTwo);
                LogCardPlay(challengerCard, defenderCard);

                if (EvaluateConditions(challengerCard, defenderCard)) {

                    continue;
                }

                battleConcluded = ResolveCombatRound(challengerCard, defenderCard, battleRound);
            }

            FinalizeBattle(battleConcluded, battleRound);
            await UpdateBattleStats();
        }

        public bool CanContinueBattle(int currentRound, bool isConcluded) {

            return deckPlayerOne.Count > 0 && deckPlayerTwo.Count > 0 && currentRound < 100 && !isConcluded;
        }

        public void LogRoundStart(int roundNumber) {

            gameLogic.Add($"Round {roundNumber} begins:");
        }

        public CardData SelectRandomCard(List<CardData> deck) {

            return deck[randomGenerator.Next(deck.Count)];
        }

        public void LogCardPlay(CardData cardOne, CardData cardTwo) {

            gameLogic.Add($"{playerOne.OwnerAccount} plays {cardOne.CardName}, {playerTwo.OwnerAccount} plays {cardTwo.CardName}");
            ActivateRandomEffect();
        }

        public bool EvaluateConditions(CardData cardOne, CardData cardTwo) {

            return AssessEncounterRules(cardOne, cardTwo);
        }

        public bool ResolveCombatRound(CardData cardOne, CardData cardTwo, int round) {

            double damageOne = DetermineCombatDamage(cardOne, cardTwo);
            double damageTwo = DetermineCombatDamage(cardTwo, cardOne);

            if (damageOne > damageTwo) {

                RecordRoundOutcome(playerOne, playerTwo, cardOne, cardTwo);
            }

            else if (damageTwo > damageOne) {

                RecordRoundOutcome(playerTwo, playerOne, cardTwo, cardOne);
            }

            else {

                gameLogic.Add("Round ends in a draw.");
            }

            return deckPlayerOne.Count == 0 || deckPlayerTwo.Count == 0;
        }

        public void FinalizeBattle(bool isConcluded, int roundsCompleted) {

            if (roundsCompleted >= 100 || isConcluded) {

                gameLogic.Add("Battle concluded due to end of rounds or a player running out of cards.");
            }

            CalculatePlayersRating();
        }

        public void CalculatePlayersRating() {

            double expectedScorePlayerOne = 1 / (1 + Math.Pow(10, (playerTwo.RatingPoints - playerOne.RatingPoints) / 400.0));
            double expectedScorePlayerTwo = 1 / (1 + Math.Pow(10, (playerOne.RatingPoints - playerTwo.RatingPoints) / 400.0));

            int actualScorePlayerOne = deckPlayerOne.Count > 0 ? 1 : 0;
            int actualScorePlayerTwo = deckPlayerTwo.Count > 0 ? 1 : 0;

            playerOne.RatingPoints += (int)(K_FACTOR * (actualScorePlayerOne - expectedScorePlayerOne));
            playerTwo.RatingPoints += (int)(K_FACTOR * (actualScorePlayerTwo - expectedScorePlayerTwo));

            gameLogic.Add($"{playerOne.PublicName} new rating: {playerOne.RatingPoints}, {playerTwo.PublicName} new rating: {playerTwo.RatingPoints}");
        }

        public void RecordRoundOutcome(UserData winner, UserData loser, CardData winningCard, CardData losingCard) {

            List<CardData> winnerDeck = winner == playerOne ? deckPlayerOne : deckPlayerTwo;
            List<CardData> loserDeck = loser == playerOne ? deckPlayerOne : deckPlayerTwo;

            loserDeck.Remove(losingCard);
            winnerDeck.Add(losingCard);

            gameLogic.Add($"{winner.PublicName} wins the round with {winningCard.CardName}. {losingCard.CardName} is moved to {winner.PublicName}'s deck.");
        }


        public async Task UpdateBattleStats() {

            await DB_Functions.UpdateUserStats(playerOne);
            await DB_Functions.UpdateUserStats(playerTwo);
            gameLogic.ForEach(Console.WriteLine);
        }

        public void ActivateRandomEffect() {

            int chance = randomGenerator.Next(100);

            if (chance < 10) {

                int effectType = randomGenerator.Next(2);

                if (effectType == 0) {

                    EnhanceAttackPower();
                }

                else {

                    ShiftElementAttribute();
                }
            }
        }

        public void EnhanceAttackPower() {

            List<CardData> selectedDeck = randomGenerator.Next(2) == 0 ? deckPlayerOne : deckPlayerTwo;
            CardData chosenCard = selectedDeck[randomGenerator.Next(selectedDeck.Count)];
            chosenCard.AttackPower += randomGenerator.Next(3, 8);

            gameLogic.Add($"Event Effect: {chosenCard.CardName} gains an attack boost.");
        }

        public void ShiftElementAttribute() {

            List<CardData> affectedDeck = randomGenerator.Next(2) == 0 ? deckPlayerOne : deckPlayerTwo;
            CardData affectedCard = affectedDeck[randomGenerator.Next(affectedDeck.Count)];

            List<string> potentialElements = new List<string> { "Fire", "Water", "Earth", "Air", "Magic" };
            potentialElements.Remove(affectedCard.ElementAttribute);

            string newElement = potentialElements[randomGenerator.Next(potentialElements.Count)];
            affectedCard.ElementAttribute = newElement;

            gameLogic.Add($"Event Effect: {affectedCard.CardName}'s element changes to {newElement}.");
        }

        public bool AssessEncounterRules(CardData attacker, CardData defender) {

            switch (attacker.CardName) {

                case var name when name.Contains("Goblin") && defender.CardName.Contains("Dragon"):
                    LogEncounterOutcome("Goblins dare not face Dragons. No clash occurs.");
                    return true;

                case var name when name.Contains("Wizzard") && defender.CardName.Contains("Ork"):
                    LogEncounterOutcome("Wizzards hold sway over Orks. The Orks stand down.");
                    return true;

                case var name when name.Contains("Knight") && defender.CardType == "Spell" && defender.ElementAttribute == "Water":
                    LogEncounterOutcome("Knights, burdened by their armor, succumb to Water Spells. No confrontation.");
                    return true;

                case var name when name.Contains("Kraken") && defender.CardType == "Spell":
                    LogEncounterOutcome("Krakens, creatures of deep lore, are unbound by spells. Encounter avoided.");
                    return true;

                case var name when name.Contains("FireElf") && defender.CardName.Contains("Dragon"):
                    LogEncounterOutcome("FireElves gracefully evade Dragons. No engagement.");
                    return true;

                default:
                    return false;
            }

        }

        public void LogEncounterOutcome(string message) {

            gameLogic.Add(message);
        }

        public double DetermineCombatDamage(CardData aggressor, CardData target) {

            double effectMultiplier = 1.0;

            if (IsSpell(aggressor) || IsSpell(target)) {

                effectMultiplier = CalculateElementEffect(aggressor.ElementAttribute, target.ElementAttribute);
            }

            return aggressor.AttackPower * effectMultiplier;
        }

        public bool IsSpell(CardData card) {

            return card.CardType.Equals("Spell", StringComparison.OrdinalIgnoreCase);
        }

        public double CalculateElementEffect(string aggressorElement, string targetElement) {

            if (aggressorElement == "Water") {

                return targetElement == "Fire" ? 2.0 : targetElement == "Regular" ? 0.5 : 1.0;
            }

            else if (aggressorElement == "Fire") {

                return targetElement == "Regular" ? 2.0 : targetElement == "Water" ? 0.5 : 1.0;
            }

            else if (aggressorElement == "Regular") {

                return targetElement == "Water" ? 2.0 : targetElement == "Fire" ? 0.5 : 1.0;
            }

            else if (aggressorElement == "Earth") {

                return targetElement == "Fire" ? 2.0 : targetElement == "Air" ? 0.5 : 1.0;
            }

            else if (aggressorElement == "Air") {

                return targetElement == "Water" ? 2.0 : targetElement == "Earth" ? 0.5 : 1.0;
            }

            return 1.0;
        }

        public void WinRound(UserData winner, UserData loser, CardData winningCard, CardData losingCard) {

            List<CardData> winnerDeck = (winner.PackageId == playerOne.PackageId) ? deckPlayerOne : deckPlayerTwo;
            List<CardData> loserDeck = (loser.PackageId == playerOne.PackageId) ? deckPlayerOne : deckPlayerTwo;

            loserDeck.Remove(losingCard);
            winnerDeck.Add(losingCard);

            gameLogic.Add($"{winner.OwnerAccount} VictoryCount the round with {winningCard.CardName}. {losingCard.CardName} is moved to {winner.OwnerAccount}'s deck.");
        }

    }
}
