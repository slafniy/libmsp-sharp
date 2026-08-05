using LibMSPSharp;

using var player1 = new LibMSP();
using var player2 = new LibMSP();

player1.StatusChanged += status => {
    Console.WriteLine($"[player #1] STATUS CHANGED TO: {status}");
};
player2.StatusChanged += status => {
    Console.WriteLine($"[player #2] STATUS CHANGED TO: {status}");
}; 

foreach ((string k, string? v) in player1.GetMetadata(args[0], ["artist", "title", "album", "date"])) {
    Console.Write($"{k}: {v}; ");
}

Console.WriteLine();

Console.WriteLine($"Status: {player1.GetStatus()}");
player1.Play(args[0]);
player2.Play(args[0]);
Thread.Sleep(100);
Console.WriteLine($"Status: {player2.GetStatus()}");
Console.WriteLine($"Total duration: {player1.GetDurationMs()} ms");
Thread.Sleep(2000);
player1.TogglePause();
Thread.Sleep(2000);
Console.WriteLine($"Status: {player2.GetStatus()}");
player2.SetVolume(0.5f);
Thread.Sleep(2000);
player1.TogglePause();
player2.SetPosition(24032);
Console.WriteLine($"Current playback pos: {player1.GetPositionMs()} ms");
Thread.Sleep(3000);
Console.WriteLine($"Current playback pos: {player2.GetPositionMs()} ms");
player1.Stop();
Thread.Sleep(50);
Console.WriteLine($"Total duration: {player2.GetDurationMs()} ms");
Thread.Sleep(100);
player1.Play(args[0]);
player2.SetVolume(0.91f);
Thread.Sleep(3000);