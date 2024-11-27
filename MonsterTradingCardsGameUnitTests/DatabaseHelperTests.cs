using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonsterTradingCardsGame.Helpers;
using Moq;

namespace MyProject.Tests
{
    [TestClass]
    public class DatabaseHelperTests
    {
        private Mock<DatabaseHelper> _mockDatabaseHelper;

        [TestInitialize]
        public void Setup()
        {
            // Mock erstellen
            _mockDatabaseHelper = new Mock<DatabaseHelper>();
        }

        [TestMethod]
        public void Register_ShouldReturnTrue_WhenUserIsNew()
        {
            // Arrange
            var username = "newUser";
            var password = "password";

            // Das Verhalten des gemockten Objekts definieren
            _mockDatabaseHelper.Setup(m => m.Register(username, password)).Returns(true);

            var service = new UserService(_mockDatabaseHelper.Object); // Angenommene Klasse, die den Helper nutzt

            // Act
            var result = service.Register(username, password);

            // Assert
            Assert.IsTrue(result, "Die Registrierung sollte erfolgreich sein.");
        }

        [TestMethod]
        public void Register_ShouldReturnFalse_WhenUserExists()
        {
            // Arrange
            var username = "existingUser";
            var password = "password";

            _mockDatabaseHelper.Setup(m => m.Register(username, password)).Returns(false);

            var service = new UserService(_mockDatabaseHelper.Object); // Angenommene Klasse, die den Helper nutzt

            // Act
            var result = service.Register(username, password);

            // Assert
            Assert.IsFalse(result, "Die Registrierung sollte fehlgeschlagen sein.");
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Hier kannst du Cleanup-Code einfügen, falls erforderlich.
        }
    }
}
