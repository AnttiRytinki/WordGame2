using OrdSpel2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainStorm
{
    public class GameEngine
    {
        // Else in Online Mode
        public bool SoloMode { get; set; } = false;

        public Helpers Helpers { get; set; } = new Helpers();
        public GameState GameState { get; set; }
        public BoardHandler BoardHandler { get; set; }

        public GameEngine() 
        {
            GameState = new GameState(Helpers);
            BoardHandler = new BoardHandler(Helpers, GameState);
        }
    }
}
