using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Original-CPU sound-ring and handshake comparison for #422.</summary>
internal static class DoorSoundQueueComparison
{
    public static int Run(string rom, string nativeCsv)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var lines = File.ReadAllLines(nativeCsv);
        if (lines.Length != 2305 || lines[0] != "library,count,start,lag,frame,read,write,state,current,delay,port,queued")
            throw new InvalidDataException("Unexpected original-CPU queue trace shape.");
        int row = 1;
        for (int library = 0; library < 3; library++)
        for (int count = 0; count <= 3; count++)
        foreach (int start in new[] { 0, 14 })
        for (int lag = 0; lag <= 2; lag++)
        {
            var audio = new CartridgeAudioState();
            audio.AdvanceFrame(bus, default);
            byte[] Field(string name) => (byte[])typeof(CartridgeAudioState)
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(audio)!;
            byte[] read = Field("_soundReadPositions"), write = Field("_soundWritePositions"),
                state = Field("_soundStates"), current = Field("_currentSounds"), delay = Field("_soundClearDelays");
            read[library] = write[library] = checked((byte)start);
            for (int i = 0; i < count; i++)
                audio.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibraries.FromCartridge((byte)(library + 1), "queue audit"), (byte)(i + 1)), 3);
            byte[] history = new byte[32];
            for (int frame = 0; frame < 32; frame++)
            {
                byte[] ack = new byte[4];
                ack[library + 1] = frame > lag ? history[frame - lag - 1] : (byte)0;
                var commands = audio.AdvanceFrame(bus, new(ack[0], ack[1], ack[2], ack[3]));
                var writes = commands.Where(c => c.Kind == CartridgeAudioCommandKind.WritePort && c.Port == library + 1).ToArray();
                if (writes.Length > 1) throw new InvalidDataException("Multiple queue writes in one dispatcher frame.");
                byte port = writes.Length == 0 ? byte.MaxValue : writes[0].Value;
                history[frame] = port != byte.MaxValue ? port : frame != 0 ? history[frame - 1] : (byte)0;
                string actual = $"{library},{count},{start},{lag},{frame},{read[library]},{write[library]},{state[library]},{current[library]},{delay[library]},{port},{(audio.HasQueuedSounds ? 1 : 0)}";
                if (actual != lines[row])
                    throw new InvalidDataException($"Sound queue mismatch row {row}: managed={actual}; original={lines[row]}.");
                row++;
            }
        }
        Console.WriteLine("Original 65816 queue parity: all 2304 rows match; three libraries, 0..3 requests, ring wrap and three acknowledgement lags.");
        return 0;
    }
}
