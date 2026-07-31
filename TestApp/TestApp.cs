using LibMSPSharp;

using var player = new LibMSP(args[0]);

foreach ((string k, string? v) in player.GetMetadata(args[1],["artist", "title", "album", "date"])) {
    Console.Write($"{k}: {v}; ");
}

Console.WriteLine();


player.Play(args[1]);

Thread.Sleep(5000);