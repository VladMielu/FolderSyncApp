# FolderSyncApp
This C# console application, named FolderSync, performs file and folder synchronization from a source directory to a destination directory at specified time intervals. Key functionality and components include:

Command-line arguments are used to specify the source folder, destination folder, and synchronization interval in seconds.

```
dotnet build
dotnet run -- "source folder path" "destination folder path" interval
```
It verifies argument count, folder existence, and interval validity.

Logs synchronization activities to a file named "synclog.txt".

Periodically triggers the SyncFoldersAsync method using a timer.

Copies new or updated files from source to destination, deletes files not in the source.

Provides a basic file comparison function to check if two files have the same content.

This code is intended to facilitate simple folder synchronization and log the actions performed during the process.
