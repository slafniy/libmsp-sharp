### What it is
LibMSP is a C# wrapper for native [libmsp](https://github.com/slafniy/libmsp)

### How to use
Copy [LibMSP.cs](LibMSPSharp/LibMSP.cs) to your project.
Build or get [libmsp.so](https://github.com/slafniy/libmsp) and place it somewhere.

Use like this:
```csharp
using LibMSPSharp;

// Initialization of the native lib - you MUST provide a path manually, it won't check PATH
using var player = new LibMSP("./libmsp.so");

// Play a song
player.Play(@"./song.mp3");

Thread.Sleep(5000);  // sleep to hear result, because LibMSP.Play() calls background playback thread 
```