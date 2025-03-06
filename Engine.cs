namespace BrainStorm
{
    public class Engine
    {
        // Else in Online Mode
        public bool SoloMode { get; set; } = false;

        public State State { get; set; }
        public BoardHandler BoardHandler { get; set; }

        public Engine() 
        {
            State = new State();
            BoardHandler = new BoardHandler(State);
        }
    }
}
