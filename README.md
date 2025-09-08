# VirtuWP
VM manager for Windows Phone <br>
NOTE: VirtuWP does not run real OS'es, it runs a recreation of them in C# and compatible with Windows Phone <br>
Requirements: <br>
OS: Windows Phone 8 or newer <br>
RAM: 512 MB or more <br>
CPU: Snapdragon 200 or better <br>
For iOS and Android, use Limbo for Android and UTM for iOS <br>
VirtuWP can run these OS's: <br>
MS-DOS <br>
Linux <br>
Per-VM max size limit: 25MB <br>
Total VM size limit: 50MB <br>
You can't have 2 25MB VMs without modifying the source code <br>
# FOR DEVS
VirtuWP saves all VMs to the Isolated storage of the XAP, as {vmname}.wpvm <br>
A example WPVM looks like this (-- won't get ignored in the WPVM reader): <br>
```os=msdos``` --sets the os, is msdos for MS-DOS and linux for Linux <br>
```newdirs="Dir1", "Dir1/Dir2"``` --specifies the folders the user has created <br>
```newfiles="Dir1/test.txt"``` --specifies the files the user has created <Br>
```Dir1/test.txt="Content of test.txt"``` --specifies the content of a file <br>
