using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.HTTP
{
    public interface IHandler
    {
        public bool Handle(HttpServerEventArgs e);
    }
}
