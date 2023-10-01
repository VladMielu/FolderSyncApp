using System;
using System.IO;
using System.Threading.Tasks;

class FolderSync
{
    static void Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.WriteLine("Usage: FolderSync <source folder> <destination folder> <sync interval in seconds>");
            return;
        }

        string sourceFolder = args[0];
        string destinationFolder = args[1];

        if (!Directory.Exists(sourceFolder))
        {
            Console.WriteLine($"Source folder '{sourceFolder}' does not exist.");
            return;
        }

        if (!Directory.Exists(destinationFolder))
        {
            Console.WriteLine($"Destination folder '{destinationFolder}' does not exist. Creating it...");
            Directory.CreateDirectory(destinationFolder);
        }

        if (!int.TryParse(args[2], out int syncIntervalSeconds) || syncIntervalSeconds <= 0)
        {
            Console.WriteLine("Invalid sync interval. Please provide a positive integer as the sync interval.");
            return;
        }

        string logFilePath = "synclog.txt"; // Log file path
        using (StreamWriter logWriter = File.AppendText(logFilePath))
        {
            Console.WriteLine($"Synchronization started. Sync interval: {syncIntervalSeconds} seconds");

            Timer syncTimer = new Timer(state => SyncFoldersAsync(sourceFolder, destinationFolder, logWriter, true).Wait(), null, 1, syncIntervalSeconds * 1000);

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }

        Console.WriteLine("Synchronization complete. Log written to 'synclog.txt'.");
    }

    static async Task SyncFoldersAsync(string sourceFolder, string destinationFolder, StreamWriter logWriter, bool mainSync)
    {
        string syncMessage = $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} - Synchronization started...";
        if (mainSync) 
        {
            Console.WriteLine(syncMessage);
            logWriter.WriteLine(syncMessage);
        }
        
        foreach (string sourceFilePath in Directory.GetFiles(sourceFolder))
        {
            string fileName = Path.GetFileName(sourceFilePath);
            string destinationFilePath = Path.Combine(destinationFolder, fileName);

            if (!File.Exists(destinationFilePath))
            {
                string logMessage = $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} - Copying '{fileName}'...";
                Console.WriteLine(logMessage);
                logWriter.WriteLine(logMessage);
                await CopyFileAsync(sourceFilePath, destinationFilePath);
            }
            else
            {
                // Compare file contents and update if they are different
                if (!AreFilesEqual(sourceFilePath, destinationFilePath))
                {
                    string logMessage = $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} - Updating '{fileName}'...";
                    Console.WriteLine(logMessage);
                    logWriter.WriteLine(logMessage);
                    await CopyFileAsync(sourceFilePath, destinationFilePath);
                }
            }
        }

        foreach (string destinationFilePath in Directory.GetFiles(destinationFolder))
        {
            string fileName = Path.GetFileName(destinationFilePath);
            string sourceFilePath = Path.Combine(sourceFolder, fileName);

            if (!File.Exists(sourceFilePath))
            {
                string logMessage = $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} - Deleting '{fileName}'...";
                Console.WriteLine(logMessage);
                logWriter.WriteLine(logMessage);
                File.Delete(destinationFilePath);
            }
        }

        foreach (string subdirectory in Directory.GetDirectories(sourceFolder))
        {
            string subdirectoryName = Path.GetFileName(subdirectory);
            string destinationSubdirectory = Path.Combine(destinationFolder, subdirectoryName);

            if (!Directory.Exists(destinationSubdirectory))
            {
                string logMessage = $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} - Creating directory '{subdirectoryName}' in the destination folder.";
                Console.WriteLine(logMessage);
                logWriter.WriteLine(logMessage);
                Directory.CreateDirectory(destinationSubdirectory);
            }

            await SyncFoldersAsync(subdirectory, destinationSubdirectory, logWriter, false);
        }

        syncMessage = $"{DateTime.Now:dd-MM-yyyy HH:mm:ss} - Synchronization completed.";
        if (mainSync)
        {
            Console.WriteLine(syncMessage);
            logWriter.WriteLine(syncMessage);
        }
    }

    static async Task CopyFileAsync(string sourcePath, string destinationPath)
    {
        using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
        using (FileStream destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
        {
            await sourceStream.CopyToAsync(destinationStream);
        }
    }

    static bool AreFilesEqual(string filePath1, string filePath2)
    {
        byte[] file1 = File.ReadAllBytes(filePath1);
        byte[] file2 = File.ReadAllBytes(filePath2);

        if (file1.Length != file2.Length)
        {
            return false;
        }

        for (int i = 0; i < file1.Length; i++)
        {
            if (file1[i] != file2[i])
            {
                return false;
            }
        }

        return true;
    }
}