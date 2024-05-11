#undef TEST
//#define TEST

using System;
using System.IO;

namespace CompareSrc
{
    internal class Program
    {
        static string name;

        static void Main(string[] args)
        {
            name = Path.GetFileName(Environment.GetCommandLineArgs()[0]);

            if (args.Length > 0 && (args[0] == "-?" || args[0] == "/?"))
            {
                help();
                return;
            }

#if TEST
            Switch sw = new Switch()
            {
                color = true,
                mode = Mode.CompareDir,
                searchOption = SearchOption.AllDirectories,
                verbose = Verbose.Mismatched
            };

            Parameters p = new Parameters()
            { 
                path1 = "D:\\xbox\\public\\wsdk\\inc",
                path2 = "D:\\xbox\\private\\inc"
            };
#else
            if (!parseArgs(args, out Switch sw))
            {
                usage();
                return;
            }
#endif

            CompareSrc compareSrc = new CompareSrc(sw);

            switch (sw.mode)
            {
                case Mode.CompareDir:
                    compareSrc.compareDir();
                    break;
                case Mode.CompareFile:
                    compareSrc.compareFile();
                    break;
                default:
                    Console.Error.WriteLine("Error, unknown compare mode" + sw.mode);
                    return;
            }
        }

        static bool parseArgs(string[] args, out Switch sw)
        {
            sw = new Switch()
            {
                color = false,
                mode = Mode.CompareDir,
                searchOption = SearchOption.AllDirectories,
                verbose = Verbose.Base,
                includeFileExts = null,
                excludeFileExts = null,
                path1 = null,
                path2 = null
            };

            if (args.Length < 3) // "<mode> <path1> <path2> <options>"
            {
                Console.Error.WriteLine("Error, required arguments missing");
                return false;
            }

            // get the path arguments

            sw.path1 = args[1];
            sw.path2 = args[2];

            if (args[0].Length < 2) // arg 1 must be at least 2 characters long
            {
                Console.Error.WriteLine("Error, invalid mode argument");
                return false;
            }

            // get the mode argument
            string mode = args[0].Remove(0, 1);

            switch (mode)
            {
                case "f":
                    sw.mode = Mode.CompareFile;
                    break;

                case "d":
                    sw.mode = Mode.CompareDir;
                    break;
                default:
                    Console.Error.WriteLine("Error, unknown mode argument -" + mode);
                    return false;
            }

            string arg;
            // get the options
            for (int i = 3; i < args.Length; i++)
            {
                if (args[i].Length < 2)
                {
                    Console.Error.WriteLine("Error, invalid switch -" + args[i]);
                    return false;
                }

                arg = args[i].Remove(0, 1);

                switch (arg)
                {
                    case "t":
                        sw.searchOption = SearchOption.TopDirectoryOnly;
                        break;
                    case "c":
                        sw.color = true;
                        break;

                    case "ok":
                    case "match":
                        sw.verbose |= Verbose.Matched;
                        break;
                    case "hash":
                    case "mismatch":
                        sw.verbose |= Verbose.Hash_Err;
                        break;
                    case "file":
                    case "missing":
                        sw.verbose |= Verbose.File_Err;
                        break;

                    case "diff":
                        sw.verbose |= Verbose.Different;
                        break;
                    case "found":
                        sw.verbose |= Verbose.Found;
                        break;

                    case "noverb":
                        sw.verbose = Verbose.Off;
                        break;

                    case "excl":
                    case "exclude":
                        if (i + 1 >= args.Length)
                        {
                            Console.Error.WriteLine("Error, missing argument for switch -" + arg);
                            return false;
                        }

                        i++;

                        if (!parseExts(args[i], out sw.excludeFileExts))
                            return false;

                        break;
                    case "incl":
                    case "include":
                        if (i + 1 >= args.Length)
                        {
                            Console.Error.WriteLine("Error, missing argument for switch -" + arg);
                            return false;
                        }

                        i++;

                        if (!parseExts(args[i], out sw.includeFileExts))
                            return false;

                        break;

                    default:
                        Console.Error.WriteLine("Error, unknown switch -" + arg);
                        return false;
                }
            }

            if (sw.verbose == Verbose.Base)
                sw.verbose = Verbose.All;
            else if ((sw.verbose & Verbose.Off) == Verbose.Off)
                sw.verbose = Verbose.Off;

            return true;
        }

        static bool parseExts(string str, out string[] exts)
        {
            exts = str.Split(';');

            for (int j = 0; j < exts.Length; j++)
            {
                exts[j] = exts[j].ToLower().Replace("*", "");

                if (exts[j].Length < 2 || exts[j][0] != '.')
                {
                    Console.Error.WriteLine("Error, invalid file extension: " + exts[j]);
                    return false;
                }
            }
            return true;
        }

        static void usage()
        {
            Console.WriteLine($"Usage: {name} <cmp_mode> <path1> <path2> <switches>\n");
        }
        static void help() 
        {
            usage();

            Console.WriteLine(" <cmp_mode>                  - The compare mode");
            Console.WriteLine("             -d              - Compare directories");
            Console.WriteLine("             -f              - Compare files");
            //Console.WriteLine();
            Console.WriteLine(" <path1>                     - Path 1 to compare");
            Console.WriteLine(" <path2>                     - Path 2 to compare");
            Console.WriteLine();

            Console.WriteLine("Switches:");
            Console.WriteLine("             -c              - Display output in color. default: off");
            Console.WriteLine("             -t              - Search top directory only. default: off");
            Console.WriteLine("             -excl <ext> ... - Exclude file extension. Eg: -excl *.obj;*.pdb");
            Console.WriteLine("             -incl <ext> ... - Include file extension. Eg: -inc *.h;*.cpp");
            Console.WriteLine();
            
            Console.WriteLine("Output mode: (multiple output modes can be combined). default: all");
            Console.WriteLine("             -noverb - No verbose output");
            Console.WriteLine("             -ok     - Show only matched");
            Console.WriteLine("             -hash   - Show only hash mismatched");
            Console.WriteLine("             -file   - Show only missing");
            Console.WriteLine("             -diff   - Show only diff");
            Console.WriteLine("             -found  - Show only found");

        }
    }
}
