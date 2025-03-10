﻿using static BrainStorm.Enums;

namespace BrainStorm
{
    public static class Helpers
    {
        public static bool RandBool()
        {
            var random = new Random();

            return random.Next(2) == 1;
        }

        public static Direction RandDirection()
        {
            if (RandBool() == true)
                return Direction.Horizontal;
            else
                return Direction.Vertical;
        }

        public static bool DirectionToBool(Direction direction)     // Works?
        {
            return direction == Direction.Horizontal;
        }

        public static Direction BoolToDirection(bool direction)     // Works?
        {
            if (direction)
                return Direction.Horizontal;
            else
                return Direction.Vertical;
        }

        public static string ReplaceAt(string input, int index, char newChar)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            char[] chars = input.ToCharArray();
            chars[index] = newChar;

            return new string(chars);
        }

        public static string StringBetween(string input, string str1, string str2)
        {
            int pFrom = input.IndexOf(str1) + str1.Length;
            int pTo = input.LastIndexOf(str2);

            return input.Substring(pFrom, pTo - pFrom);
        }

        public static bool IsValidIP(string ip)
        {
            System.Net.IPAddress? ipAddr;

            return System.Net.IPAddress.TryParse(ip, out ipAddr);
        }

        /// <summary>
        /// Returns true if the game board coverage becomes above n letters if text would be added
        /// </summary>
        public static bool WillBoardCoverageBeAbove(GameState state, int n, string text)
        {
            int z = 0;

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    if (state.Board[y][x] != ' ')
                        ++z;
                }
            }

            if ((z + text.Length) > n)
                return true;

            else
                return false;
        }
    }
}
