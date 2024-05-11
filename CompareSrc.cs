using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;

namespace CompareSrc
{
    public class CompareSrc
    {
        // Author, tommojphillips, 09.05.2024
        // Github, https://github.com/tommojphillips

        private readonly Dictionary<FileStatus, string> errors = new Dictionary<FileStatus, string>()
        {
            { FileStatus.Match,    "MATCH    " },
            { FileStatus.Hash_Err, "MISMATCH " },
            { FileStatus.File_Err, "NOT FOUND" }
        };

        private Switch sw;

        private int numListed;
        private int numOk;
        private int numHashErr;
        private int numFileErr;

        private int colWidth;

        private int windowWidth;
        private int dirWidth;

        public CompareSrc(Switch sw)
        {
            this.sw = sw;

            windowWidth = Console.WindowWidth;
        }

        public bool compareDir()
        {
            string dir1 = sw.path1;
            string dir2 = sw.path2;

            string[] filePaths1;
            string[] filePaths2;

            FileData file1;
            FileData file2;

            bool isMatch;

            FileStatus status;

            reset();
            
            if (!checkDir(dir1, dir2))
                return false;

            if (!getFiles(dir1, out filePaths1))
                return false;

            if (!getFiles(dir2, out filePaths2))
                return false;

            string tmp = "";

            if (sw.verbose != Verbose.Off)
                tmp = "Listing " + sw.verbose.toString().ToLower().Replace("_", " ") + " files\n";

            tmp += "Searching " + (sw.searchOption == SearchOption.TopDirectoryOnly ? "top directory" : "directories");

            if (sw.includeFileExts != null)
                tmp += " for *" + string.Join(", *", sw.includeFileExts) + " files";
            else
                tmp += " for all files";

            if (sw.excludeFileExts != null)
                tmp += " excluding *" + string.Join(", *", sw.excludeFileExts);

            Console.WriteLine(tmp);

            // swap dirs if dir2 has more files
            if (filePaths1.Length < filePaths2.Length)
            {
                tmp = dir1;
                dir1 = dir2;
                dir2 = tmp;
                
                sw.path1 = dir1;
                sw.path2 = dir2;

                string[] tempFiles = filePaths1;
                filePaths1 = filePaths2;
                filePaths2 = tempFiles;
            }

            List<FileData> files1 = new List<FileData>();
            List<FileData> files2 = new List<FileData>();

            for (int i = 0; i < filePaths1.Length; i++)
            {
                // cal the column width for the status message
                int len = filePaths1[i].Length - dir1.Length - 14 + 1;
                if (len > colWidth)
                    colWidth = len;

                tmp = Path.GetExtension(filePaths1[i]).ToLower();

                // whitelist file extensions
                if (sw.includeFileExts != null)
                { 
                    if (Array.IndexOf(sw.includeFileExts, tmp) == -1)
                        continue;
                }
                // blacklist file extensions
                if (sw.excludeFileExts != null)
                {
                    if (Array.IndexOf(sw.excludeFileExts, tmp) != -1)
                        continue;
                }

                files1.Add(new FileData(filePaths1[i]));

                if (i < filePaths2.Length) // files2 is always equal or less than files1
                {
                    files2.Add(new FileData(filePaths2[i]));
                }
            }

            int padding = files1.Count.ToString().Length;

            Console.WriteLine("\nFound " + files1.Count.padl(padding) + " of " + filePaths1.Length.padl(padding) + " files in " + dir1 +
                              "\nFound " + files2.Count.padl(padding) + " of " + filePaths2.Length.padl(padding) + " files in " + dir2 + "\n");

            if (files1.Count == 0 | files2.Count == 0)
            {
                Console.WriteLine("No files to compare");
                return false;
            }

            isMatch = true;

            FileData fileDataEmpty = new FileData(null);

            using (SHA256 sha = SHA256.Create())
            {
                for (int i = 0; i < files1.Count; i++)
                {
                    file1 = files1[i];
                    file2 = fileDataEmpty;

                    // find the file in files2
                    for (int j = 0; j < files2.Count; j++)
                    {
                        if (file1.name == files2[j].name)
                        {
                            file2 = files2[j];
                            break;
                        }
                    }

                    if (file2.path == null) // file not found
                    {
                        isMatch = false;
                        status = FileStatus.File_Err;
                        numFileErr++;
                    }
                    else // file found
                    {
                        if (!compareFile(sha, ref file1, ref file2)) // hash mismatch
                        {
                            isMatch = false;
                            status = FileStatus.Hash_Err;
                            numHashErr++;
                        }
                        else // hash match
                        {
                            status = FileStatus.Match;
                            numOk++;
                        }
                    }

                    // check if the file_status flag is set in verbose
                    if ((status & (FileStatus)sw.verbose) == 0)
                        continue;

                    writeStatus(status, file1, file2);
                }
            }

            writeResult(files1.Count);

            return isMatch;
        }

        public bool compareFile()
        {
            reset();

            if (!checkFile(sw.path1, sw.path2))
                return false;

            FileData file1 = new FileData(sw.path1);
            FileData file2 = new FileData(sw.path2);

            FileStatus status;

            using (SHA256 sha = SHA256.Create())
            {
                if (!compareFile(sha, ref file1, ref file2))
                {
                    status = FileStatus.Hash_Err;
                    numHashErr++;
                }
                else
                {
                    status = FileStatus.Match;
                    numOk++;
                }

                writeStatus(status, file1, file2);
            }

            return true;
        }

        private bool compareFile(SHA256 sha, ref FileData file1, ref FileData file2)
        {
            file1.computeHash(sha);
            file2.computeHash(sha);

            return file1.checkHash(file2);
        }

        private bool checkFile(string file1, string file2)
        {
            if (!File.Exists(file1))
            {
                Console.WriteLine("file1 doesn't exist, " + file1);
                return false;
            }
            if (!File.Exists(file2))
            {
                Console.WriteLine("file2 doesn't exist, " + file2);
                return false;
            }

            return true;
        }

        private bool checkDir(string dir1, string dir2)
        {
            if (!Directory.Exists(dir1))
            {
                Console.Error.WriteLine("Error, dir1 doesn't exist, " + dir1);
                return false;
            }
            if (!Directory.Exists(dir2))
            {
                Console.Error.WriteLine("Error, dir2 doesn't exist, " + dir2);
                return false;
            }

            if (dir1 == dir2)
            {
                Console.Error.WriteLine("Error, cannot compare the same directory");
                return false;
            }

            return true;
        }

        private bool getFiles(string dir, out string[] files)
        {
            files = Directory.GetFiles(dir, "*.*", sw.searchOption);

            if (files.Length == 0)
            {
                Console.WriteLine("No files found");
                return false;
            }
            
            return true;
        }

        private void writeStatus(FileStatus status, FileData file1, FileData file2)
        {
            ConsoleColor defColor = Console.ForegroundColor;

            string msg;

            numListed++;

            if (status == (FileStatus)sw.verbose)
                msg = errors[status].TrimEnd();
            else
                msg = errors[status];

            if (sw.color)
                Console.ForegroundColor = status.toConsoleColor();

            Console.Write(msg);

            if (sw.color)
                Console.ForegroundColor = defColor;


            dirWidth = (windowWidth - msg.Length - (10 * 2)) / 2;

            msg = file1.path.Replace(sw.path1, "");

            if (msg.Length > dirWidth)
            {
                // find index of the last directory separator
                int index = msg.LastIndexOf('\\', msg.Length - dirWidth);

                msg = msg.Substring(index);
            }
                        
            msg = " -> .." + msg.PadRight(dirWidth, ' ');

            switch (status)
            {
                case FileStatus.Match:
                case FileStatus.Hash_Err:
                    msg += " -> .." + file2.path.Replace(sw.path2, "");
                    break;
            }

            Console.WriteLine(msg);
        }

        private void writeResult(int len)
        {
            if (len == 0)
            {
                Console.WriteLine("No files were compared");
                return;
            }

            int numHashErrPercent = numHashErr * 100 / len;
            int numFileErrPercent = numFileErr * 100 / len;
            int numOkPercent = 100 - numHashErrPercent - numFileErrPercent;
            int numCmp = numOk + numHashErr + numFileErr;
            int numCmpPercent = numCmp * 100 / len;
            int numListedPercent = numListed * 100 / len;

            string str;

            str = $"Matched:    {numOk.padl(5)} {numOkPercent.ToString("00")}%\n" +
                  $"Mismatched: {numHashErr.padl(5)} {numHashErrPercent.ToString("00")}%\n" +
                  $"Not found:  {numFileErr.padl(5)} {numFileErrPercent.ToString("00")}%\n" +
                  $"Compared:   {numCmp.padl(5)} {numCmpPercent.ToString("00")}%\n";

            if (sw.verbose != Verbose.Off)
                str = $"\nListed: {numListed.padl(5)} {numListedPercent.ToString("00")}%\n\n" + str;

            Console.WriteLine(str);

            /*Console.WriteLine($"\n Listed:     {numListed.padl(5)} {numListedPercent.ToString("00")}%\n\n" +
                                $" Matched:    {numOk.padl(5)} {numOkPercent.ToString("00")}%\n" +
                                $" Mismatched: {numHashErr.padl(5)} {numHashErrPercent.ToString("00")}%\n" +
                                $" Not found:  {numFileErr.padl(5)} {numFileErrPercent.ToString("00")}%\n");*/
        }

        private void reset()
        {
            numListed = 0;
            numOk = 0;
            numHashErr = 0;
            numFileErr = 0;
            colWidth = 0;
        }
    }
}
