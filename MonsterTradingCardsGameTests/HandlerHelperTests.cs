using MonsterTradingCardsGame.Helpers;
using MonsterTradingCardsGame.HTTP;

[TestClass]
public class AuthorizationTests // public ist hier erforderlich
{
    [TestMethod]
    public void ValidateAuthorization_NoAuthorizationHeader_ShouldReturnFalse()
    {
        // Arrange
        var eventArgs = new HttpServerEventArgs
        {
            Headers = new List<HttpHeader>
        {
            new HttpHeader("test")
        }.ToArray()
        };

        // Act
        bool result = HandlerHelper.ValidateAuthorization(eventArgs);

        // Assert
        Assert.IsFalse(result); // Es wird false erwartet
    }
}