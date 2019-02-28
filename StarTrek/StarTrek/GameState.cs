using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarTrek
{
    public class GameState
    {
        public int TurnCount { get; set; } = 0;
        public int Klingons { get; set; } = 30;
        public string StarMap { get; set; } = "";
        
    }
}
