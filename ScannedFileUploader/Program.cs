using System;
using System.IO;

namespace ScannedFileUploader
{
    class Program
    {
        private static Client _client;

        private static string _dstPath;

        private static FileSystemWatcher _watcher;

        static void Main(string[] args)
        {
            //_client = new Client(args[0]);
            _dstPath = args[2];

            _watcher = new FileSystemWatcher(args[1], "*.*");
            _watcher.NotifyFilter = NotifyFilters.LastAccess
                | NotifyFilters.LastWrite
                | NotifyFilters.FileName;

            _watcher.Created += (_, e) => Console.WriteLine($"Created {e.Name}");
            _watcher.Changed += (_, e) => Console.WriteLine($"Changed {e.Name}");
            _watcher.Deleted += (_, e) => Console.WriteLine($"Deleted {e.Name}");
            _watcher.Renamed += (_, e) => Console.WriteLine($"Renamed {e.OldName} -> {e.Name}");

            _watcher.EnableRaisingEvents = true;
            Console.ReadLine();

            _watcher.Dispose();
            //_client.Dispose();
        }
    }
}
