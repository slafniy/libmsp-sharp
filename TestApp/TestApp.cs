using LibMSPSharp;



if (!LibMSPGlue.Init()) {
    throw new Exception("Cannot init native msplib");
}

LibMSPGlue.Play(@"/mnt/data/Music/Avatar/2023 - Dance Devil Dance/01. Dance Devil Dance.mp3");

Thread.Sleep(3000);

LibMSPGlue.Deinit();