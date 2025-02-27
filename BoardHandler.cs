namespace BrainStorm
{
    public class BoardHandler
    {
        public Helpers Helpers { get; set; }
        public GameState GameState { get; set; }

        public BoardHandler(Helpers helpers, GameState gameState)
        {
            Helpers = helpers;
            GameState = gameState;
        }

        public bool CanAddToBoard(string text)
        {
            bool canAdd;

            for (int i = 0; i < 100; i++)
            {
                Random rnd = new Random();
                int x = rnd.Next(0, 9);
                int y = rnd.Next(0, 9);
                bool right = Helpers.RandBool();

                try
                {
                    canAdd = TryAddToBoard(x, y, right, text, true);
                }
                catch
                {
                    continue;
                }

                if (canAdd)
                {
                    return true;
                }
            }

            return false;
        }

        public bool AddToBoard(string text)
        {
            bool success;

            for (int i = 0; i < 100; i++)
            {
                Random rnd = new Random();
                int x = rnd.Next(0, 9);
                int y = rnd.Next(0, 9);
                bool right = Helpers.RandBool();

                try
                {
                    success = TryAddToBoard(x, y, right, text, false);
                }
                catch
                {
                    continue;
                }

                if (success)
                {
                    GameState.WordList.Add(new Word(x, y, text, right));

                    return true;
                }
            }

            return false;
        }

        public bool TryAddToBoard(int x, int y, bool right, string text, bool test)
        {
            if (right)
            {
                if (BoardSpaceRightIsEmpty(x, y, text.Length))
                {
                    if (!test)
                        PutStringInBoardRight(x, y, text);

                    return true;
                }

                else
                    return false;
            }

            else
            {
                if (BoardSpaceDownIsEmpty(x, y, text.Length))
                {
                    if (!test)
                        PutStringInBoardDown(x, y, text);

                    return true;
                }

                else
                    return false;
            }
        }

        private void PutStringInBoardRight(int x, int y, string text)
        {
            for (int X = x; X < (text.Length + x); X++)
            {
                GameState.Board[y] = Helpers.ReplaceAt(GameState.Board[y], X, text[X - x]);
            }
        }

        private bool BoardSpaceRightIsEmpty(int x, int y, int length)
        {
            for (int X = x; X < (length + x); X++)
            {
                if (GameState.Board[y][X] != (char)32)
                    return false;
            }

            return true;
        }

        private void PutStringInBoardDown(int x, int y, string text)
        {
            for (int Y = y; Y < (text.Length + y); Y++)
            {
                GameState.Board[Y] = Helpers.ReplaceAt(GameState.Board[Y], x, text[Y - y]);
            }
        }

        private bool BoardSpaceDownIsEmpty(int x, int y, int length)
        {
            for (int Y = y; Y < (length + y); Y++)
            {
                if (GameState.Board[Y][x] != (char)32)
                    return false;
            }

            return true;
        }
    }
}
