using static BrainStorm.Enums;

namespace BrainStorm
{
    public class State
    {
        public List<string> Board { get; set; } = new List<string>();
        public List<string> RevealedBoard { get; set; } = new List<string>();
        public List<Word> WordList { get; set; } = new List<Word>();
        public string Phase { get; set; } = "[PHASE1]";

        public int PointsPlayerA { get; set; } = 0;
        public int PointsPlayerB { get; set; } = 0;

        public bool LetterWasRevealed { get; set; } = false;
        public bool WordWasRevealed { get; set; } = false;

        public State()
        {
            Init();
        }

        public void Init()
        {
            for (int i = 0; i < 10; i++)
            {
                Board.Add("          ");
                RevealedBoard.Add("          ");
            }
        }

        public override string ToString()
        {
            string gameState = "[BEGINGAMESTATE]";

            gameState += Phase;

            gameState += "[POINTS]";

            gameState += PointsPlayerA.ToString() + ";";
            gameState += PointsPlayerB.ToString() + ";";

            gameState += "[LETTERWASREVEALED]";

            gameState += LetterWasRevealed.ToString();

            gameState += "[WORDWASREVEALED]";

            gameState += WordWasRevealed.ToString();

            gameState += "[BOARD]";

            foreach (string str in Board)
            {
                gameState += str + ";";
            }

            gameState += "[REVEALEDBOARD]";

            foreach (string str in RevealedBoard)
            {
                gameState += str + ";";
            }

            gameState += "[WORDLIST]";

            foreach (Word word in WordList)
            {
                List<string> wordProps = word.ToStringList();

                for (int i = 0; i < wordProps.Count; i++)
                {
                    gameState += wordProps[i] + ";";
                }
            }

            gameState += "[ENDGAMESTATE]";

            return gameState;
        }

        public void FromString(string state)
        {
            Phase = Helpers.StringBetween(state, "[BEGINGAMESTATE]", "[POINTS]");

            var pointsStrings = Helpers.StringBetween(state, "[POINTS]", "[LETTERWASREVEALED]").Split(";");

            PointsPlayerA = int.Parse(pointsStrings[0]);
            PointsPlayerB = int.Parse(pointsStrings[1]);

            var letterWasRevealedString = Helpers.StringBetween(state, "[LETTERWASREVEALED]", "[WORDWASREVEALED]");

            LetterWasRevealed = bool.Parse(letterWasRevealedString);

            var wordWasRevealedString = Helpers.StringBetween(state, "[WORDWASREVEALED]", "[BOARD]");

            WordWasRevealed = bool.Parse(wordWasRevealedString);

            string boardString = Helpers.StringBetween(state, "[BOARD]", "[REVEALEDBOARD]");
            var boardStrings = boardString.Split(";");

            for (int i = 0; i < 10; i++)
                Board[i] = boardStrings[i];

            string revealedBoardString = Helpers.StringBetween(state, "[REVEALEDBOARD]", "[WORDLIST]");
            var revealedBoardStrings = revealedBoardString.Split(";");

            for (int i = 0; i < 10; i++)
                RevealedBoard[i] = revealedBoardStrings[i];

            string wordPropsString = Helpers.StringBetween(state, "[WORDLIST]", "[ENDGAMESTATE]");
            var wordPropsStrings = wordPropsString.Split(";");

            string[] newWordPropsStrings = new string[wordPropsStrings.Length - 1];

            for (int i = 0; i < newWordPropsStrings.Length; i++)
                newWordPropsStrings[i] = wordPropsStrings[i];

            WordList = new List<Word>();

            for (int i = 0; i < newWordPropsStrings.Length / 5; i++)
            {
                var word = new Word();
                word.FromStringList(
                    new List<string>()
                    {
                        newWordPropsStrings[i*5],
                        newWordPropsStrings[i*5 + 1],
                        newWordPropsStrings[i*5 + 2],
                        newWordPropsStrings[i*5 + 3],
                        newWordPropsStrings[i*5 + 4],
                    });

                WordList.Add(word);
            }
        }

        public void RevealAll()
        {
            RevealedBoard = Board;
        }

        public bool Reveal(int x, int y, out bool wordWasRevealed)
        {
            if (Board[y][x] == ' ')
            {
                RevealedBoard[y] = Helpers.ReplaceAt(RevealedBoard[y], x, '#');
                wordWasRevealed = false;
                WordWasRevealed = false;

                return false;
            }

            else
            {
                LetterWasRevealed = true;
                RevealedBoard[y] = Helpers.ReplaceAt(RevealedBoard[y], x, Board[y][x]);

                wordWasRevealed = true;
                WordWasRevealed = true;
                var word = GetWord(x, y);

                if (word.Direction == Direction.Horizontal)
                {
                    for (int xx = word.X; xx < word.X + word.TheWord.Length; xx++)
                    {
                        if (RevealedBoard[y][xx] != word.TheWord[xx - word.X])
                        {
                            wordWasRevealed = false;
                            WordWasRevealed = false;
                        }
                    }
                }

                if (word.Direction == Direction.Vertical)
                {
                    for (int yy = word.Y; yy < word.Y + word.TheWord.Length; yy++)
                    {
                        if (RevealedBoard[yy][x] != word.TheWord[yy - word.Y])
                        {
                            wordWasRevealed = false;
                            WordWasRevealed = false;
                        }
                    }
                }

                return true;
            }
        }

        public void RevealWord(int x, int y)
        {
            Word? foundWord = null;

            foreach (Word word in WordList)
            {
                if (word.Direction == Direction.Horizontal)
                {
                    for (int xx = word.X; xx < (word.X) + word.TheWord.Length; xx++)
                    {
                        if (xx == x && word.Y == y)
                        {
                            word.IsRevealed = true;
                            foundWord = word;
                        }
                    }
                }

                if (word.Direction == Direction.Vertical)
                {
                    for (int yy = word.Y; yy < (word.Y) + word.TheWord.Length; yy++)
                    {
                        if (word.X == x && yy == y)
                        {
                            word.IsRevealed = true;
                            foundWord = word;
                        }
                    }
                }
            }

            if (foundWord != null)
            {
                if (foundWord.Direction == Direction.Horizontal)
                {
                    for (int xx = foundWord.X; xx < foundWord.X + foundWord.TheWord.Length; xx++)
                        Reveal(xx, y, out bool b);
                }

                if (foundWord.Direction == Direction.Vertical)
                {
                    for (int yy = foundWord.Y; yy < foundWord.Y + foundWord.TheWord.Length; yy++)
                        Reveal(x, yy, out bool b);
                }
            }
        }

        public Word GetWord(int x, int y)
        {
            foreach (Word word in WordList)
            {
                if (word.Direction == Direction.Horizontal)
                {
                    for (int xx = word.X; xx < (word.X) + word.TheWord.Length; xx++)
                    {
                        if (xx == x && word.Y == y)
                            return word;
                    }
                }

                if (word.Direction == Direction.Vertical)
                {
                    for (int yy = word.Y; yy < (word.Y) + word.TheWord.Length; yy++)
                    {
                        if (word.X == x && yy == y)
                            return word;
                    }
                }
            }

            return new Word();
        }
    }
}
