# Folders2Hash

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](LICENSE)

Tool to calculate hashsums for all files of a folder and its subfolders by a given folder or by drag and drop a file / folder to ui.
It's also possible to trigger this tool through command line / scheduled task to generate the checksums in an interval to provide them for ftp downloads for example.

## Silentmode (command line) configuration
### Configuration of "SilentConfiguration.json"
- KeepFileExtension (true/false): keep or remove file extensions for generated hash files (myfile.md5 / myfile.exe.md5)
- PathsToScan: list of paths to scan. "true" at the moment marks a path to be a directory, "false" would be a single file
- LoggingPath: directory where the result of a single calculation run will be stored as a "csv" file
- HashTypes: list of hash sum types to generate for each file. Currently supported: md5, sha1, sha256, sha384 and sha512. Maybe more to follow.

### Example:
```json
{
    "KeepFileExtension": true,
    "PathsToScan": {
        "C:\\Files": true
    },
    "LoggingPath": "C:\\Apps\\Folders2Hash\\Logs",
    "HashTypes": [
        "md5",
        "sha256"
    ]
}
```

### Execution
> 'Folders2Hash.exe -silent'

## Package Feeds

Default by `NuGet.config` is myget.org

| Feed                           | Feed Url                                                         |
| :----------------------------- | :--------------------------------------------------------------- |
| ![myget.org][myGetBadge]       | <https://www.myget.org/F/evilbaschdi/api/v3/index.json>          |
| ![codeberg.org][codebergBadge] | <https://codeberg.org/api/packages/evilbaschdi/nuget/index.json> |

## Quality & Activity

| Branch                          | Status & Activity                                                                                                                                      |
| :------------------------------ | :----------------------------------------------------------------------------------------------------------------------------------------------------- |
| ![Main Branch][mainBranchBadge] | [![CodeFactor][codeFactorMainBadge]][codeFactorMainOverview] ![Commit Activity Main][commitActivityMainBadge] ![Last Commit Main][lastCommitMainBadge] |

[myGetBadge]: https://img.shields.io/badge/MyGet.org-gray?style=for-the-badge&logo=myget
[codebergBadge]: https://img.shields.io/badge/Codeberg-gray?style=for-the-badge&logo=codeberg

[mainBranchBadge]: https://img.shields.io/badge/branch-main-brightgreen?style=for-the-badge&logo=git&logoColor=white&color=c9ff00
[developBranchBadge]: https://img.shields.io/badge/branch-develop-blue?style=for-the-badge&logo=git&logoColor=white&color=0080ff

[codeFactorMainBadge]: https://www.codefactor.io/repository/github/evilbaschdi/Folders2Hash/badge/main?style=for-the-badge
[codeFactorMainOverview]: https://www.codefactor.io/repository/github/evilbaschdi/Folders2Hash/overview/main
[commitActivityMainBadge]: https://img.shields.io/github/commit-activity/m/evilbaschdi/Folders2Hash/main?style=for-the-badge
[lastCommitMainBadge]: https://img.shields.io/github/last-commit/evilbaschdi/Folders2Hash/main?style=for-the-badge

[codeFactorDevelopBadge]: https://www.codefactor.io/repository/github/evilbaschdi/Folders2Hash/badge/develop?style=for-the-badge
[codeFactorDevelopOverview]: https://www.codefactor.io/repository/github/evilbaschdi/Folders2Hash/overview/develop
[commitActivityDevelopBadge]: https://img.shields.io/github/commit-activity/m/evilbaschdi/Folders2Hash/develop?style=for-the-badge
[lastCommitDevelopBadge]: https://img.shields.io/github/last-commit/evilbaschdi/Folders2Hash/develop?style=for-the-badge