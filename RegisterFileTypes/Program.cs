using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace RegisterFileTypes
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            var fileExtensions = new Dictionary<string, string>
                                 {
                                     { "MD5", ".md5" },
                                     { "SHA-1", ".sha1" },
                                     { "SHA-256", ".sha256" },
                                     { "SHA-384", ".sha384" },
                                     { "SHA-512", ".sha512" }
                                 };
            var exe = $@"{Directory.GetCurrentDirectory()}\Folders2Hash.exe";
            foreach (var fileExtension in fileExtensions)
            {
                CreateFileAssociation(exe, fileExtension);
                Console.WriteLine(fileExtension.Value);
            }
            Console.WriteLine("...");
            Console.ReadLine();
        }

        private static void CreateFileAssociation(string executable, KeyValuePair<string, string> extension)
        {
            if (executable == null)
            {
                throw new ArgumentNullException(nameof(executable));
            }

            var registryKey = Registry.ClassesRoot.CreateSubKey(extension.Value);
            registryKey?.SetValue("", extension.Key);
            var subKey = Registry.ClassesRoot.CreateSubKey($@"{extension.Key}\shell\open\command");
            subKey?.SetValue("", $@"""{executable}"" ""%1""");
        }
    }
}