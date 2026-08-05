using LibMSPSharp;

// Using file paths provided as arguments for this example.
// E.g. 
// ./ExampleApp "/mnt/data/Music/Shylmagoghnar/2014 - Emergence/01. I Am the Abyss.mp3" \
// "/mnt/data/Music/Be'lakor/2021 - Coherence/01 Locus.mp3"
string song1 = args[0];
string song2 = args[1];

PrintMeta(song1); // see PrintMeta() code below, note is uses a static LibMSP.GetMetadata() method
PrintMeta(song2);
/* Expected output:
artist: Shylmagoghnar; title: I Am the Abyss; album: Emergence; date: 2014;
artist: Be'lakor; title: Locus; album: Coherence; date: 2021;
 */

// To use playback functionality, you need an instance of library.
// Use "using" to help native backend free itself earlier, otherwise it won't do it till GC collects the object.
using var player = new LibMSP();

// you can create several instances of the library, and they will work independently:
// using var player2 = new LibMSP();

// to start playback just call
player.Play(song1);

// and wait some time to actually hear the song, because Play() does not block current thread while playing,
// it just asks the background thread to open file and start playback.
Thread.Sleep(2000);

// to seek to some particular position use
player.SetPosition(35000); // moves to the 35 sec point
Thread.Sleep(2000);

// if you want to move to some particular % of the song, get its duration first
uint? duration = player.GetDurationMs();
// and then use it like this, e.g. you want to seek to 55%:
if (duration != null) { // it can be null e.g. if nothing's currently open
    player.SetPosition((uint)(duration * 0.55));
}

// you can always check the status
Console.WriteLine(player.GetStatus());  // expected "Playing"

// but there's more convenient way - use status events to react on status change
// to subscribe on events:
player.StatusChanged += status => {
    Console.WriteLine($">>> player changed status to {status}");
}; 

// check it:
player.TogglePause();
player.TogglePause();
player.Stop();
player.Play(song2);
player.SetPosition(12345);
/* Expecting this
>>> player changed status to Paused
>>> player changed status to Playing
>>> player changed status to Idle
>>> player changed status to Playing
>>> player changed status to Idle
 */

Thread.Sleep(2000);

// to stop the playback (also closes the file) call this
player.Stop();

return;

void PrintMeta(string songPath) {
    // Don't need any initialization to extract metadata from the file
    // There is no standard for keys, but the most common should work for every music file.
    foreach ((string k, string? v) in LibMSP.GetMetadata(songPath, ["artist", "title", "album", "date"])) {
        Console.Write($"{k}: {v}; ");
    }

    Console.WriteLine();
}
