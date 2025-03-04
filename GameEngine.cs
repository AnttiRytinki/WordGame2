namespace BrainStorm
{
    public class GameEngine
    {
        // Else in Online Mode
        public bool SoloMode { get; set; } = false;

        public GameState GameState { get; set; }
        public BoardHandler BoardHandler { get; set; }

        public GameEngine() 
        {
            GameState = new GameState();
            BoardHandler = new BoardHandler(GameState);
        }
    }
}
