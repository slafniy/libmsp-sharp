[![Publish NuGet package](https://github.com/slafniy/libmsp-sharp/actions/workflows/build_nuget.yml/badge.svg?branch=master&event=workflow_dispatch)](https://github.com/slafniy/libmsp-sharp/actions/workflows/build_nuget.yml)

### What it is
LibMSP is a C# wrapper for native [libmsp](https://github.com/slafniy/libmsp)

### How to use
Install from nuget.org:  
`dotnet add package slafniy.LibMSPSharp`

Use like this:
```csharp
using LibMSPSharp;

// "using" helps to free inner resources and do not wait object desctruction and GC
using var player = new LibMSP("./libmsp.so");

// Play a song
player.Play("./song.mp3");

// sleep to hear something, because LibMSP.Play() (and other calls too) calls background playback thread
// and does not block current thread
Thread.Sleep(5000); 
```