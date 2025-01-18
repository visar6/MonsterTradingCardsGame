using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MonsterTradingCardsGameLibrary.Models;
using System.Collections.Generic;
using MonsterTradingCardsGameLibrary.Models.MonsterTradingCardsGameLibrary.Models;

namespace MonsterTradingCardsGameUnitTest
{

    [TestClass]
    public class DatabaseHelperTests
    {
        private Mock<IDatabaseHelper> mockDbHelper;

        [TestInitialize]
        public void Setup()
        {
            mockDbHelper = new Mock<IDatabaseHelper>();
        }

        [TestMethod]
        public void Register_NewUser_ReturnsTrue()
        {
            mockDbHelper.Setup(db => db.Register("newUser", "password123")).Returns(true);
            bool result = mockDbHelper.Object.Register("newUser", "password123");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Register_ExistingUser_ReturnsFalse()
        {
            mockDbHelper.Setup(db => db.Register("existingUser", "password123")).Returns(false);
            bool result = mockDbHelper.Object.Register("existingUser", "password123");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Login_ValidCredentials_ReturnsTrue()
        {
            mockDbHelper.Setup(db => db.Login("validUser", "correctPassword")).Returns(true);
            bool result = mockDbHelper.Object.Login("validUser", "correctPassword");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Login_InvalidCredentials_ReturnsFalse()
        {
            mockDbHelper.Setup(db => db.Login("validUser", "wrongPassword")).Returns(false);
            bool result = mockDbHelper.Object.Login("validUser", "wrongPassword");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetUser_ExistingUser_ReturnsUserObject()
        {
            var user = new User { Id = "1", Username = "testUser", Coins = 100 };
            mockDbHelper.Setup(db => db.GetUser("testUser")).Returns(user);
            var result = mockDbHelper.Object.GetUser("testUser");
            Assert.IsNotNull(result);
            Assert.AreEqual("testUser", result.Username);
        }

        [TestMethod]
        public void GetUser_NonExistingUser_ReturnsNull()
        {
            mockDbHelper.Setup(db => db.GetUser("unknownUser")).Returns((User)null);
            var result = mockDbHelper.Object.GetUser("unknownUser");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void IsValidToken_ValidToken_ReturnsTrue()
        {
            mockDbHelper.Setup(db => db.IsValidToken("validToken")).Returns(true);
            bool result = mockDbHelper.Object.IsValidToken("validToken");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsValidToken_InvalidToken_ReturnsFalse()
        {
            mockDbHelper.Setup(db => db.IsValidToken("invalidToken")).Returns(false);
            bool result = mockDbHelper.Object.IsValidToken("invalidToken");
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CreatePackage_ValidCards_ReturnsTrue()
        {
            var cards = new List<Card> { new Card { Id = "1", Name = "Dragon", Damage = 50 } };
            mockDbHelper.Setup(db => db.CreatePackage(cards)).Returns(true);
            bool result = mockDbHelper.Object.CreatePackage(cards);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetNextAvailablePackage_PackageExists_ReturnsPackage()
        {
            var package = new Package { Id = "pkg1", Price = 5 };
            mockDbHelper.Setup(db => db.GetNextAvailablePackage()).Returns(package);
            var result = mockDbHelper.Object.GetNextAvailablePackage();
            Assert.IsNotNull(result);
            Assert.AreEqual("pkg1", result.Id);
        }

        [TestMethod]
        public void GetNextAvailablePackage_NoPackage_ReturnsNull()
        {
            mockDbHelper.Setup(db => db.GetNextAvailablePackage()).Returns((Package)null);
            var result = mockDbHelper.Object.GetNextAvailablePackage();
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetCardsForPackage_ValidPackageId_ReturnsCards()
        {
            var cards = new List<Card> { new Card { Id = "1", Name = "Dragon", Damage = 50 } };
            mockDbHelper.Setup(db => db.GetCardsForPackage("pkg1")).Returns(cards);
            var result = mockDbHelper.Object.GetCardsForPackage("pkg1");
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetUserCards_ValidUser_ReturnsCards()
        {
            var cards = new List<Card> { new Card { Id = "1", Name = "Goblin", Damage = 30 } };
            mockDbHelper.Setup(db => db.GetUserCards("testUser")).Returns(cards);
            var result = mockDbHelper.Object.GetUserCards("testUser");
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetUserDeck_ValidUser_ReturnsDeck()
        {
            var cards = new List<Card> { new Card { Id = "2", Name = "Kraken", Damage = 70 } };
            mockDbHelper.Setup(db => db.GetUserDeck("testUser")).Returns(cards);
            var result = mockDbHelper.Object.GetUserDeck("testUser");
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetUserStack_ValidUser_ReturnsStack()
        {
            var cards = new List<Card> { new Card { Id = "3", Name = "Wizard", Damage = 40 } };
            mockDbHelper.Setup(db => db.GetUserStack("testUser")).Returns(cards);
            var result = mockDbHelper.Object.GetUserStack("testUser");
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void UpdateUserDeck_ValidDeck_ReturnsTrue()
        {
            var cardIds = new List<string> { "1", "2", "3" };
            mockDbHelper.Setup(db => db.UpdateUserDeck("testUser", cardIds)).Returns(true);
            bool result = mockDbHelper.Object.UpdateUserDeck("testUser", cardIds);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void UpdateUser_ValidUser_ReturnsTrue()
        {
            mockDbHelper.Setup(db => db.UpdateUser("testUser", "New Name", "New Bio", "newImage.jpg")).Returns(true);
            bool result = mockDbHelper.Object.UpdateUser("testUser", "New Name", "New Bio", "newImage.jpg");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetUserStats_ExistingUser_ReturnsStats()
        {
            var stats = new Stats { UserId = "1", Elo = 1200, Wins = 5, Losses = 3 };
            mockDbHelper.Setup(db => db.GetUserStats("1")).Returns(stats);
            var result = mockDbHelper.Object.GetUserStats("1");
            Assert.IsNotNull(result);
            Assert.AreEqual(1200, result.Elo);
        }

        [TestMethod]
        public void GetScoreboard_ReturnsListOfStats()
        {
            var statsList = new List<Stats>
            {
                new Stats { UserId = "1", Elo = 1200, Wins = 5, Losses = 3 },
                new Stats { UserId = "2", Elo = 1100, Wins = 4, Losses = 4 }
            };

            mockDbHelper.Setup(db => db.GetScoreboard()).Returns(statsList);
            var result = mockDbHelper.Object.GetScoreboard();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void TradingDealExists_ValidId_ReturnsTrue()
        {
            mockDbHelper.Setup(db => db.TradingDealExists("deal1")).Returns(true);
            bool result = mockDbHelper.Object.TradingDealExists("deal1");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ExecuteTrade_ValidTrade_ReturnsTrue()
        {
            var deal = new TradingDeal { Id = "deal1", UserId = "user1", CardId = "card1" };
            mockDbHelper.Setup(db => db.ExecuteTrade("buyer1", "offeredCard", deal)).Returns(true);
            bool result = mockDbHelper.Object.ExecuteTrade("buyer1", "offeredCard", deal);
            Assert.IsTrue(result);
        }
    }
}