using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameLibrary.Models
{
    using System;
    using System.Text.Json.Serialization;

    namespace MonsterTradingCardsGameLibrary.Models
    {
        public class TradingDeal
        {
            [JsonPropertyName("Id")]
            public string Id { get; set; }
            
            public string UserId { get; set; }

            [JsonPropertyName("CardToTrade")]
            public string CardId { get; set; }

            [JsonPropertyName("Type")]
            public string DesiredCardType { get; set; } 
            
            public int MinimumDamage { get; set; } 

            public TradingDeal(string tradingDealId, string userId, string cardId, string desiredCardType, int minimumDamage)
            {
                Id = tradingDealId;
                UserId = userId;
                CardId = cardId;
                DesiredCardType = desiredCardType;
                MinimumDamage = minimumDamage;
            }

            public TradingDeal()
            {

            }
        }
    }

}
