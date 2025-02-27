namespace BrainStorm
{
    public class Helpers
    {
        public bool RandBool()
        {
            var random = new Random();
            return random.Next(2) == 1;
        }

        public string ReplaceAt(string input, int index, char newChar)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }

        public string StringBetween(string input, string str1, string str2)
        {
            int pFrom = input.IndexOf(str1) + str1.Length;
            int pTo = input.LastIndexOf(str2);

            return input.Substring(pFrom, pTo - pFrom);
        }
    }
}
