using NUnit.Framework;
using MTCG.BattleLogic;
using MTCG.Cards;
using System.Collections.Generic;


namespace MTCG_Testing {

    public class Tests {

        private Battle_Functions battleFunctions;
        private UserData playerOne;
        private UserData playerTwo;
        private List<CardData> deckPlayerOne;
        private List<CardData> deckPlayerTwo;

        [SetUp]
        public void Setup() {

            playerOne = new UserData { PublicName = "Player1", RatingPoints = 1000, OwnerAccount = "Owner1" };
            playerTwo = new UserData { PublicName = "Player2", RatingPoints = 1000, OwnerAccount = "Owner2" };
            deckPlayerOne = new List<CardData> { new CardData { CardName = "Dragon", ElementAttribute = "Fire", AttackPower = 10 } };
            deckPlayerTwo = new List<CardData> { new CardData { CardName = "Goblin", ElementAttribute = "Earth", AttackPower = 5 } };

            battleFunctions = new Battle_Functions(playerOne, playerTwo, deckPlayerOne, deckPlayerTwo, null);
        }

        [Test]
        public void CanContinueBattle_TrueIfDecksNotEmptyAndRoundLessThan100() {

            var result = battleFunctions.CanContinueBattle(1, false);
            Assert.IsTrue(result, "Battle should continue when both decks are not empty and rounds are less than 100.");
        }

        [Test]
        public void SelectRandomCard_ReturnsCardFromDeck() {

            var selectedCard = battleFunctions.SelectRandomCard(deckPlayerOne);
            Assert.IsTrue(deckPlayerOne.Contains(selectedCard), "Selected card should be from player one's deck.");
        }

        [Test]
        public void EvaluateConditions_NoSpecialConditionsMet_ReturnsFalse() {

            var cardOne = new CardData { CardName = "Elf", ElementAttribute = "Air", AttackPower = 8 };
            var cardTwo = new CardData { CardName = "Dwarf", ElementAttribute = "Earth", AttackPower = 7 };

            var result = battleFunctions.EvaluateConditions(cardOne, cardTwo);
            Assert.IsFalse(result, "Should return false when no special conditions are met.");
        }

        [Test]
        public void CalculatePlayersRating_RatingsChangeAfterBattle() {

            deckPlayerOne.Clear();

            battleFunctions.CalculatePlayersRating();

            Assert.AreNotEqual(1000, playerOne.RatingPoints, "Player One's rating should change.");
            Assert.AreNotEqual(1000, playerTwo.RatingPoints, "Player Two's rating should change.");
        }

        [Test]
        public void ActivateRandomEffect_DoesNotAlwaysActivate() {

            bool effectActivated = false;

            for (int i = 0; i < 100; i++) {

                var initialGameLogicCount = battleFunctions.gameLogic.Count;
                battleFunctions.ActivateRandomEffect();

                if (battleFunctions.gameLogic.Count > initialGameLogicCount) {

                    effectActivated = true;
                    break;
                }
            }
            Assert.IsTrue(effectActivated, "ActivateRandomEffect should sometimes activate, but not always.");
        }

        [Test]
        public void LogRoundStart_IncrementsGameLogic() {

            var initialCount = battleFunctions.gameLogic.Count;
            battleFunctions.LogRoundStart(1);
            Assert.AreEqual(initialCount + 1, battleFunctions.gameLogic.Count, "LogRoundStart should add a message to gameLogic.");
        }

        [Test]
        public void RecordRoundOutcome_IncreasesWinnerDeckSize() {

            var winningCard = new CardData { CardName = "Dragon" };
            var losingCard = new CardData { CardName = "Goblin" };
            battleFunctions.deckPlayerOne = new List<CardData> { winningCard };
            battleFunctions.deckPlayerTwo = new List<CardData> { losingCard };

            battleFunctions.RecordRoundOutcome(playerOne, playerTwo, winningCard, losingCard);

            Assert.AreEqual(2, battleFunctions.deckPlayerOne.Count, "Winner's deck size should increase after winning a round.");
        }

        [Test]

        public void EnhanceAttackPower_IncreasesAttackPower() {

            var card = new CardData { AttackPower = 5 };
            battleFunctions.deckPlayerOne = new List<CardData> { card };

            for (int i = 0; i < 100; i++) {

                battleFunctions.EnhanceAttackPower();

                if (card.AttackPower > 5) {

                    Assert.Pass("EnhanceAttackPower should increase the attack power of a card.");
                }
            }
            Assert.Fail("EnhanceAttackPower did not increase the attack power after 100 attempts.");
        }

        [Test]
        public void ShiftElementAttribute_ChangesElementForARandomCard() {

            foreach (var card in battleFunctions.deckPlayerOne) {

                card.ElementAttribute = "Fire";
            }
            bool elementChanged = false;

            for (int i = 0; i < 100; i++) {

                battleFunctions.ShiftElementAttribute();

                if (battleFunctions.deckPlayerOne.Any(card => card.ElementAttribute != "Fire")) {

                    elementChanged = true;
                    break;
                }
            }
            Assert.IsTrue(elementChanged, "ShiftElementAttribute should change the element attribute of at least one card.");
        }

        [Test]
        public void LogCardPlay_AddsEntriesToGameLogic() {

            var initialCount = battleFunctions.gameLogic.Count;
            battleFunctions.LogCardPlay(deckPlayerOne[0], deckPlayerTwo[0]);

            Assert.Greater(battleFunctions.gameLogic.Count, initialCount, "LogCardPlay should add an entry to gameLogic.");
        }

        [Test]
        public void IsSpell_DetectsSpellCardsCorrectly() {

            var spellCard = new CardData { CardType = "Spell" };
            var creatureCard = new CardData { CardType = "Creature" };

            bool spellCardResult = battleFunctions.IsSpell(spellCard);
            bool creatureCardResult = battleFunctions.IsSpell(creatureCard);

            Assert.IsTrue(spellCardResult, "IsSpell should return true for spell cards.");
            Assert.IsFalse(creatureCardResult, "IsSpell should return false for non-spell cards.");
        }

        [Test]
        public void FinalizeBattle_UpdatesGameLogic() {

            var initialCount = battleFunctions.gameLogic.Count;
            battleFunctions.FinalizeBattle(true, 5);

            Assert.Greater(battleFunctions.gameLogic.Count, initialCount, "FinalizeBattle should add a concluding message to gameLogic.");
        }

        [Test]
        public void CanContinueBattle_FalseWhenOneDeckIsEmpty() {

            battleFunctions.deckPlayerTwo.Clear();
            var result = battleFunctions.CanContinueBattle(10, false);
            Assert.IsFalse(result, "Battle should not continue if one of the decks is empty.");
        }

        [Test]
        public void SelectRandomCard_AlwaysSelectsValidCard() {

            CardData selectedCard = battleFunctions.SelectRandomCard(deckPlayerOne);
            Assert.IsNotNull(selectedCard, "Selected card should not be null.");
            Assert.IsTrue(deckPlayerOne.Contains(selectedCard), "Selected card should be from player one's deck.");
        }

        [Test]
        public void CalculatePlayersRating_ChangesRatingPoints() {

            int initialRatingPlayerOne = playerOne.RatingPoints;
            int initialRatingPlayerTwo = playerTwo.RatingPoints;

            battleFunctions.CalculatePlayersRating();

            bool ratingsChanged = playerOne.RatingPoints != initialRatingPlayerOne || playerTwo.RatingPoints != initialRatingPlayerTwo;
            Assert.IsTrue(ratingsChanged, "Player ratings should change after calculating new ratings.");
        }

        [Test]
        public void DeckSizeDecreasesAfterLosingCard() {

            var initialDeckSize = battleFunctions.deckPlayerOne.Count;
            battleFunctions.RecordRoundOutcome(playerTwo, playerOne, deckPlayerTwo[0], deckPlayerOne[0]);
            Assert.AreEqual(initialDeckSize - 1, battleFunctions.deckPlayerOne.Count, "Deck size should decrease after losing a card.");
        }

        [Test]
        public void DeckSizeIncreasesAfterWinningCard() {

            var initialDeckSize = battleFunctions.deckPlayerTwo.Count;
            battleFunctions.RecordRoundOutcome(playerTwo, playerOne, deckPlayerTwo[0], deckPlayerOne[0]);
            Assert.AreEqual(initialDeckSize + 1, battleFunctions.deckPlayerTwo.Count, "Deck size should increase after winning a card.");
        }

        [Test]
        public void UpdateBattleStats_ReflectsChangesInPlayerStats() {

            battleFunctions.deckPlayerTwo.Clear();
            battleFunctions.CalculatePlayersRating();

            bool statsChanged = playerOne.RatingPoints != 1000 || playerTwo.RatingPoints != 1000;
            Assert.IsTrue(statsChanged, "Player stats should reflect changes after a battle.");
        }

        [Test]
        public void AssessEncounterRules_GoblinVsDragon_ReturnsTrue() {

            var goblinCard = new CardData { CardName = "Goblin", ElementAttribute = "Earth", AttackPower = 5 };
            var dragonCard = new CardData { CardName = "Dragon", ElementAttribute = "Fire", AttackPower = 50 };

            bool result = battleFunctions.AssessEncounterRules(goblinCard, dragonCard);

            Assert.IsTrue(result, "Goblin facing Dragon should not initiate combat.");
        }

        [Test]
        public void CalculateElementEffect_WaterVsFire_ReturnsDoubleEffect() {

            double effect = battleFunctions.CalculateElementEffect("Water", "Fire");

            Assert.AreEqual(2.0, effect, "Water attacking Fire should double the effect.");
        }
    }
}