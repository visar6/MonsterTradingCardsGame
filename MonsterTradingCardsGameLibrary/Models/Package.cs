using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameLibrary.Models
{
    public class Package
    {
        public string Id { get; set; }

        public int Price { get; set; }

        public List<Card> Cards { get; set; }

        public Package()
        {
            
        }
    }
}