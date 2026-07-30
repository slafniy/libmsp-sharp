using LibMSPSharp;

using var player = new LibMSP("/mnt/data/code/libmsp/cmake-build-release/libmsp.so");

foreach ((string k, string? v) in player.GetMetadata(
             @"/mnt/data/Music/Avatar/2023 - Dance Devil Dance/01. Dance Devil Dance.mp3",
             ["artist", "title", "album", "date"]
         )) {
    Console.Write($"{k}: {v}; ");
}

Console.WriteLine();


player.Play(@"/mnt/data/Music/Avatar/2023 - Dance Devil Dance/01. Dance Devil Dance.mp3");

Thread.Sleep(5000);