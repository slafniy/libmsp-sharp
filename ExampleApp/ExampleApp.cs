using LibMSPSharp;

using var player = new LibMSP(args[0]);

foreach ((string k, string? v) in player.GetMetadata(args[1], ["artist", "title", "album", "date"])) {
    Console.Write($"{k}: {v}; ");
}

Console.WriteLine();

Console.WriteLine($"Status: {player.GetStatus()}");
player.Play(args[1]);
Thread.Sleep(100);
Console.WriteLine($"Status: {player.GetStatus()}");
Console.WriteLine($"Total duration: {player.GetDurationMs()} ms");
Thread.Sleep(2000);
player.TogglePause();
Thread.Sleep(2000);
Console.WriteLine($"Status: {player.GetStatus()}");
player.SetVolume(0.5f);
Thread.Sleep(2000);
player.TogglePause();
player.SetPosition(24032);
Console.WriteLine($"Current playback pos: {player.GetPositionMs()} ms");
Thread.Sleep(3000);
Console.WriteLine($"Current playback pos: {player.GetPositionMs()} ms");
player.Stop();
Thread.Sleep(50);
Console.WriteLine($"Total duration: {player.GetDurationMs()} ms");
Thread.Sleep(100);
player.Play(args[1]);
player.SetVolume(0.91f);
Thread.Sleep(3000);