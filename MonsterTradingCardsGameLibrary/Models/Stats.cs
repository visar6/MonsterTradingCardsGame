﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGameLibrary.Models
{
    public class Stats
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int Elo { get; set; }

        public int Wins { get; set; }

        public int Losses { get; set; }
    }
}