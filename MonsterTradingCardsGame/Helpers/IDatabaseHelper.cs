using MonsterTradingCardsGameLibrary.Models.MonsterTradingCardsGameLibrary.Models;
using MonsterTradingCardsGameLibrary.Models;

public interface IDatabaseHelper
{
    // Benutzerverwaltung
    bool Register(string username, string password);
    
    bool Login(string username, string enteredPassword);
    
    User? GetUser(string selectedUsername);
    
    User? GetUserByUsername(string username);
    
    bool UpdateUser(string username, string name, string bio, string image);
    
    string? GetUsernameFromToken(string token);
    
    bool IsValidToken(string token);

    // Kartenverwaltung & Deck-Management
    List<Card> GetUserCards(string username);
    
    List<Card> GetUserStack(string username);
    
    List<Card>? GetUserDeck(string username);
    
    bool UpdateUserDeck(string username, List<string> cardIds);
    
    bool AddCardsToUserStack(string userId, List<Card> cards);

    // Paket- & Trade-Funktionen
    bool CreatePackage(List<Card> cards);

    Package? GetNextAvailablePackage();

    List<Card> GetCardsForPackage(string packageId);

    void DeletePackage(string packageId);

    void UpdateUserCoins(string userId, int newCoins);

    // Kampfverwaltung
    void SaveBattle(Battle battle, string? winnerId);

    void TransferCardsToWinner(string winnerId, string loserId);

    void UpdatePlayerStats(string userId, bool isWinner);

    // Statistiken & Rangliste
    Stats? GetUserStats(string userId);

    List<Stats> GetScoreboard();

    // Handels-System
    bool CreateTradingDeal(TradingDeal deal);
    
    bool DeleteTradingDeal(string tradingDealId, string userId);
    
    List<TradingDeal> GetAllTradingDeals();
    
    bool TradingDealExists(string tradingDealId);
    
    bool UserOwnsTradingDeal(string userId, string tradingDealId);
    
    bool UserOwnsCard(string userId, string cardId);
    
    bool CardMeetsTradingRequirements(string offeredCardId, TradingDeal deal);
    
    TradingDeal? GetTradingDeal(string tradingDealId);
    
    bool ExecuteTrade(string buyerId, string offeredCardId, TradingDeal deal);
}
