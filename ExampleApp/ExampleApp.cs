using LibMSPSharp;

using var player = new LibMSP(args[0]);

foreach ((string k, string? v) in player.GetMetadata(args[1], ["artist", "title", "album", "date"])) {
    Console.Write($"{k}: {v}; ");
}

Console.WriteLine();

player.Play(args[1]);
Thread.Sleep(2000);
player.TogglePause();
Thread.Sleep(2000);
player.SetVolume(0.5f);
Thread.Sleep(2000);
player.TogglePause();
Thread.Sleep(2000);
player.Stop();
player.Play(args[1]);
player.SetVolume(0.91f);
Thread.Sleep(3000);