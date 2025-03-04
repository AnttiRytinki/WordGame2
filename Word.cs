using static BrainStorm.Enums;
using static BrainStorm.Helpers;

namespace BrainStorm
{
    public class Word
    {
        public int X { get; set; } = -1;
        public int Y { get; set; } = -1;
        public string TheWord { get; set; } = "";
        public bool IsRevealed { get; set; } = false;
        public Direction Direction { get; set; } = Direction.Horizontal;

        public Word() 
        {
        }

        public Word(int x, int y, string theWord, Direction direction)
        {
            X = x;
            Y = y;
            TheWord = theWord;
            IsRevealed = false;
            Direction = direction;
        }

        public List<string> ToStringList()
        {
            var list = new List<string>();

            list.Add(X.ToString());
            list.Add(Y.ToString());
            list.Add(TheWord);
            list.Add(IsRevealed.ToString());
            list.Add(DirectionToBool(Direction).ToString());

            return list;
        }

        public void FromStringList(List<string> list)
        {
            X = int.Parse(list[0]);
            Y = int.Parse(list[1]);
            TheWord = list[2];
            IsRevealed = bool.Parse(list[3]);
            Direction = BoolToDirection(bool.Parse(list[4]));
        }
    }
}
