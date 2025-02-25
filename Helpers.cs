namespace OrdSpel2
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
    }
}
