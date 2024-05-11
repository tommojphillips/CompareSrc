using System;

namespace CompareSrc
{
    public static class Extensions
    {
        public static ConsoleColor toConsoleColor(this FileStatus status)
        {
            switch (status)
            {
                case FileStatus.Match:
                    return ConsoleColor.Green;
                case FileStatus.File_Err:
                    return ConsoleColor.Red;
                case FileStatus.Hash_Err:
                    return ConsoleColor.Yellow;
                default:
                    return ConsoleColor.Gray;
            }
        }

        public static string toString(this Verbose verbose)
        {
            switch (verbose)
            {
                case Verbose.Matched:
                    return "matched";
                case Verbose.File_Err:
                    return "missing";
                case Verbose.Hash_Err:
                    return "hash mismatched";

                case Verbose.Found:
                    return "matched or mismatched";

                case Verbose.Different:
                    return "different";

                case Verbose.Matched_Or_Missing:
                    return "matched or missing";

                case Verbose.All:
                    return "all";

                default:
                    return verbose.ToString();
            }
        }

        public static string padl(this int str, int len)
        {
            return str.ToString().PadLeft(len, ' ');
        }

        public static string padr(this int str, int len)
        {
            return str.ToString().PadRight(len, ' ');
        }
    }   
}
