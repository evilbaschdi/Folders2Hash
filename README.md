# Folders2Md5

Tool to calculate hashsums for all files of a folder and its subfolders by a given folder or by drag and drop a file / folder to ui.
It's also possible to trigger this tool through command line / scheduled task to generate the checksums in an interval to provide them for ftp downloads for example.

## Silentmode (command line) configuration ##

### Configuration of "SilentConfiguration.json"

- KeepFileExtension (true/false): keep or remove file extensions for generated hash files (myfile.md5 / myfile.exe.md5)
- PathsToScan: list of paths to scan. "true" at the moment marks a path to be a directory, "false" would be a single file
- LoggingPath: directory where the result of a single calculation run will be stored as a "csv" file
- HashTypes: list of hash sum types to generate for each file. Currently supported: md5, sha1, sha256, sha384 and sha512. Maybe more to follow.

Example:
```json
{
    "KeepFileExtension": true,
    "PathsToScan": {
        "C:\\Files": true    
    },
    "LoggingPath": "C:\\Apps\\Folders2Md5\\Logs",
    "HashTypes": [
        "md5",
        "sha256"
    ]
}
```

### Execution
> 'Folders2Md5.exe -silent'

## Requirements ##

! https://github.com/evilbaschdi/EvilBaschdi.Core is required for this project !
This will be provided through nuget packages from myget.org

Now you can open and rebuild the Folders2Md5 project. The EvilBaschdi.Core project will be rebuild automatically as a pre-build step of Folders2Md5.
