using System;
using System.IO;
using System.Security.Cryptography;

namespace CompareSrc
{
    [Flags]
    public enum FileStatus
    {
        Match    = 1 << 1, // 2
        File_Err = 1 << 2, // 4
        Hash_Err = 1 << 3, // 8
    }

    [Flags]
    public enum Verbose
    {
        Base = 0,
        Off = 1 << 0,                   // 1
        Matched =        FileStatus.Match,      // 2
        File_Err =       FileStatus.File_Err,   // 4
        Hash_Err =       FileStatus.Hash_Err,   // 8

        Found =          Matched | Hash_Err,    // 10
        Different =      File_Err | Hash_Err,   // 12

        Matched_Or_Missing = Matched | File_Err, // 6

        All = Matched | File_Err | Hash_Err,    // 14

    }

    public struct Switch
    {
        public bool color;
        public Mode mode;
        public SearchOption searchOption;
        public Verbose verbose;

        public string[] excludeFileExts;
        public string[] includeFileExts;

        public string path1;
        public string path2;
    }

    public struct FileData
    {
        private bool _hashComputed;

        private string _path;
        private string _name;

        private byte[] _data;
        private byte[] _hash;

        public string path => _path;
        public string name => _name;

        const int HASH_SIZE = 256 / 8; // 256 bits = 32 bytes

        public FileData(string file)
        {
            _hashComputed = false;

            _path = file;
            _data = null;
            _hash = null;

            _name = Path.GetFileName(file);
        }

        public byte[] getHash()
        {
            if (!_hashComputed)
                return null;
            return _hash;
        }
        public byte[] getData()
        {
            if (!_hashComputed)
                return null;
            return _data;
        }

        public void computeHash(SHA256 sha)
        {
            _data = File.ReadAllBytes(_path);

            if (_data == null || sha == null)
                return;

            _hash = sha.ComputeHash(_data);
        }

        public void computeHash(SHA256 sha, FileStream stream)
        {
            _data = File.ReadAllBytes(_path);

            if (_data == null || sha == null)
                return;

            _hash = sha.ComputeHash(_data);
        }

        public bool checkHash(FileData data)
        {
            if (_hash == null || data._hash == null)
                return false;

            if (_hash.Length != HASH_SIZE || data._hash.Length != HASH_SIZE)
                return false;

            for (int i = 0; i < _hash.Length; i++)
            {
                if (_hash[i] != data._hash[i])
                    return false;
            }

            return true;
        }
    }
}
