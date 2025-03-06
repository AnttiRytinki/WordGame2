namespace BrainStorm
{
    public class Engine
    {
        // Else in Online Mode
        public bool SoloMode { get; set; } = false;

        public State State { get; set; }
        public BoardHandler BoardHandler { get; set; }

        public int LastClickedX { get; set; } = -1;
        public int LastClickedY { get; set; } = -1;

        public bool ButtonsEnabled { get; set; } = false;
        public bool GameHasStarted { get; set; } = false;

        public Engine() 
        {
            State = new State();
            BoardHandler = new BoardHandler(State);
        }
    }
}
