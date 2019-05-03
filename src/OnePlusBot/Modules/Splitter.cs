using System.Collections.Generic;

namespace OnePlusBot.Modules
{
    public static class SplitThis
    {
        public static IEnumerable<string> SplitByLength(this string s, int length = 2000)
        {
            for (int i = 0; i < s.Length; i += length)
            {
                if (i + length <= s.Length)
                {
                    yield return s.Substring(i, length);
                }
                else
                {
                    yield return s.Substring(i);
                }
            }
        }
    }
}
