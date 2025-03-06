namespace BrainStorm
{
    public class GameEngine
    {
        // Else in Online Mode
        public bool SoloMode { get; set; } = false;

        public State State { get; set; }
        public BoardHandler BoardHandler { get; set; }

        public GameEngine() 
        {
            State = new State();
            BoardHandler = new BoardHandler(State);
        }
    }
}
