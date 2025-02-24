using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdSpel2
{
    public class Word(int x, int y, string theWord, bool horizontal)
    {
        public Word() : this(0, 0, "", false) { }

        public int X { get; set; } = x;
        public int Y { get; set; } = y;
        public string TheWord { get; set; } = theWord;
        public bool IsRevealed { get; set; } = false;
        public bool Horizontal { get; set; } = horizontal;

        public List<string> ToStringList()
        {
            var list = new List<string>();

            list.Add(X.ToString());
            list.Add(Y.ToString());
            list.Add(TheWord);
            list.Add(IsRevealed.ToString());
            list.Add(Horizontal.ToString());

            return list;
        }

        public void FromStringList(List<string> list)
        {
            X = int.Parse(list[0]);
            Y = int.Parse(list[1]);
            TheWord = list[2];
            IsRevealed = bool.Parse(list[3]);
            Horizontal = bool.Parse(list[4]);
        }
    }
}
