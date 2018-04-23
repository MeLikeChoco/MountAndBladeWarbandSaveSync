﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MountAndBladeWarbandSaveSync
{
    class Program
    {

        static readonly string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        const string SaveGameFolder = "Mount&Blade Warband Savegames/";
        static readonly string SaveGamePath = Path.Combine(DocumentsPath, SaveGameFolder);

        static string[] _modules;

        static void Main(string[] args)
        {

            var drive = GetRemoveableDrive(args);
            var selection = GetModuleSelection();

            Console.WriteLine("Beginning sync...");

            if (selection == 0)
                for (int i = 1; i <= _modules.Length; i++)
                    Sync(drive, i);
            else
                Sync(drive, selection);

            Console.WriteLine("Sync finished.");
            Console.ReadKey();

        }

        static void Sync(string drive, int selection)
        {

            var firstPath = _modules[selection - 1];
            var firstLoc = new DirectoryInfo(firstPath);
            var secondPath = Path.Combine(drive, SaveGameFolder, firstLoc.Name);

            Console.WriteLine("Module save folder not detected in selected drive. Creating one...");
            Directory.CreateDirectory(secondPath);
            Console.WriteLine("Module save folder created.");

            var secondLoc = new DirectoryInfo(secondPath);
            var firstFiles = firstLoc.GetFiles();
            var secondFiles = secondLoc.GetFiles();

            Switcheroo(firstFiles, secondFiles, secondPath);
            Switcheroo(secondFiles, firstFiles, firstPath);

        }

        static void Switcheroo(FileInfo[] from, FileInfo[] to, string toDirectory)
        {

            foreach (var fromFile in from)
            {

                var toFile = to.FirstOrDefault(file => file.Name == fromFile.Name);

                if (toFile == null)
                {

                    var toFilePath = Path.Combine(toDirectory, fromFile.Name);
                    toFile = new FileInfo(toFilePath);

                    using (var stream = toFile.Create()) { }

                }

                if ((toFile != null && fromFile.LastWriteTimeUtc.CompareTo(toFile.LastWriteTimeUtc) > 0) || toFile == null)
                    fromFile.CopyTo(toFile.FullName, true);

            }

        }

        static int GetModuleSelection()
        {

            _modules = Directory.GetDirectories(SaveGamePath);

            if (!_modules.Any())
                EndProgramWithErrorMessage("There are no savegames currently available to sync!");

            Console.WriteLine("Select a module to sync");
            Console.WriteLine("0 - All");

            for (int i = 0; i < _modules.Length; i++)
                Console.WriteLine($"{i + 1} - {new DirectoryInfo(_modules[i]).Name}");

            string input;

            do
            {

                Console.Write("Enter a number that corresponds to your choice: ");

                input = Console.ReadLine();

            } while (!(int.TryParse(input, out var throwaway) && throwaway >= 0 && throwaway <= _modules.Length));

            return int.Parse(input);

        }

        static string GetRemoveableDrive(string[] args)
        {

            var drives = DriveInfo.GetDrives();

            if (!(args.Any() && char.TryParse(args.First(), out var drive)))
            {

                Console.Write("List of valid drives: ");
                Console.WriteLine(drives.Select(d => d.Name).Aggregate((current, next) => $"{current}, {next}"));

                string input;

                do
                {

                    Console.Write("Which drive to use? Use letter only. Drive: ");
                    input = Console.ReadLine();

                } while (!(char.TryParse(input, out var throwaway) && char.IsLetter(throwaway)));

                drive = char.Parse(input);

            }

            return drives.FirstOrDefault(d => d.Name.Contains(drive))?.Name;

        }

        static void EndProgramWithErrorMessage(string message)
        {

            Console.Write(message);
            Console.ReadKey();
            Environment.Exit(0);

        }

    }
}