# Folders2Md5
A little tool I wrote to generate checksum file of type md5 for each file in a folder and it's subdirectories.

It's also possible to trigger this tool through command line / scheduled task to generate the checksums in an interval to provide them for ftp downloads for example.

## Configuration of command line arguments: ##

- g '*path*' to set the path the checksums should be created for.
- l '*path*' to set the path you logging file should be stored at.</br>
Folders2Md5.exe g 'F:\Setup\Images' l 'C:\Temp'

*(optional)*

- k to keep the file extension of the file you created a checksum for. For example: backup.zip => backup.zip.md5 </br>
Folders2Md5.exe g 'F:\Setup\Images' l 'C:\Temp' k
