using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>Replays #478's recorded enemy requests through the production audio queues.</summary>
internal sealed class RidleyDeathAudioSequence
{
    private readonly SuperMetroidAddressSpace bus;
    private readonly CartridgeAudioState audio = new();
    private readonly Dictionary<int, string[][]> rows;
    private CartridgeAudioAcknowledgements acknowledgements;
    private readonly int warmupFrames;
    public int FrameCount { get; }

    public RidleyDeathAudioSequence(string rom, string trace, int warmupFrames)
    {
        this.warmupFrames = warmupFrames;
        bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        string[] lines = File.ReadAllLines(trace);
        if (lines.Length < 2 || lines[0] != "frame,phase,kind,library,command,queueLimitOrDelayFrames")
            throw new InvalidDataException("Expected current Ridley death audio trace.");
        var parsed = lines.Skip(1).Select(line => line.Split(',')).ToArray();
        if (parsed.Any(row => row.Length != 6)) throw new InvalidDataException("Malformed audio row.");
        rows = parsed.GroupBy(row => int.Parse(row[0])).ToDictionary(group => group.Key, group => group.ToArray());
        FrameCount = rows.Keys.Max() + 1;
        for (int frame = 0; frame < FrameCount; frame++)
            if (!rows.TryGetValue(frame, out var frameRows) || frameRows.Count(row => row[2] == "frame") != 1)
                throw new InvalidDataException($"Missing/duplicate Ridley frame marker {frame}.");
        var room = CartridgeRoomHeader.Load(bus, NorfairRidleyAudit.RoomPointer);
        audio.QueueRoomMusic(room.State.MusicDataIndex, 5); // Reveal's native active-fight track.
        Console.WriteLine($"Ridley audio bank={room.State.MusicDataIndex:X2}, fight track=5; {FrameCount} captured frames.");
    }

    public void Acknowledge(byte[] ports) => acknowledgements = new(ports[0], ports[1], ports[2], ports[3]);

    public CartridgeAudioCommand[] Step(int frame)
    {
        // Let music upload/start settle before the captured death interval. This does
        // not reproduce the player's exact song phase or preceding battle sound history.
        int deathFrame = frame - warmupFrames;
        if (rows.TryGetValue(deathFrame, out var frameRows))
        foreach (var row in frameRows)
        {
            switch (row[2])
            {
                case "frame": break;
                case "sound":
                    audio.QueueSound(new SoundEffectId(Enum.Parse<SoundEffectLibrary>(row[3]), byte.Parse(row[4])), byte.Parse(row[5]));
                    break;
                case "music":
                    audio.QueueMusicDelayed(MusicCommand.FromCartridge(ushort.Parse(row[4])), MusicCommandDelay.FromEffectiveFrames(ushort.Parse(row[5])));
                    break;
                default: throw new InvalidDataException($"Unknown audio event {row[2]}.");
            }
        }
        var commands = audio.AdvanceFrame(bus, acknowledgements).ToList();
        if (frame == 0) commands.Insert(0, CartridgeAudioCommand.Upload(AudioUploadAddresses.SpcEngine));
        return commands.ToArray();
    }
}
