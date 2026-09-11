using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyHealthWarningNative(string path)
    {
        int cases = 0;
        foreach (string line in File.ReadLines(path).Skip(1))
        {
            int[] row = line.Split(',').Select(int.Parse).ToArray();
            var warning = new SamusHealthWarningState();
            var audio = new CartridgeAudioState();
            if (row[1] != 0) warning.Update(30, audio);
            audio.Reset();
            for (int i = 0; i < row[3]; i++) audio.QueueSound(SoundEffectLibrary3Sounds.SpeedBoosterEcho, 6);
            byte[] positions = (byte[])typeof(CartridgeAudioState).GetField("_soundWritePositions", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(audio)!;
            byte[,] queues = (byte[,])typeof(CartridgeAudioState).GetField("_soundQueues", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(audio)!;
            warning.Update((ushort)row[0], audio, soundSuppressed: row[2] != 0);
            AssertEqual(row[4] != 0, warning.IsActive, "native warning latch: " + line);
            AssertEqual(row[5], positions[2] - row[3], "native queue admission: " + line);
            AssertEqual(row[6], row[5] != 0 ? queues[2, row[3]] : 0, "native sound identity: " + line);
            audio.Reset(); warning.Update((ushort)row[0], audio);
            AssertEqual(row[7], positions[2], "native no-retry after suppression or full queue: " + line);
            cases++;
        }
        AssertEqual(198, cases, "complete original CPU warning matrix");
        string handlerPath = path.EndsWith(".warning.csv", StringComparison.Ordinal)
            ? path[..^".warning.csv".Length] + ".warning-handlers.csv"
            : throw new ArgumentException("Use the native probe's .warning.csv output.", nameof(path));
        var expectedHandlers = new Dictionary<string, int> { ["90E725"] = 1, ["90E8DC"] = 0,
            ["90E8D6"] = 0, ["90E8D9"] = 0, ["90E902"] = 1, ["90E8EC"] = 0 };
        foreach (string line in File.ReadLines(handlerPath).Skip(1))
        {
            string[] row = line.Split(',');
            AssertTrue(expectedHandlers.Remove(row[0], out int expected), "known unique native handler " + row[0]);
            AssertEqual(expected, int.Parse(row[1]), "original handler reaches low-health check " + row[0]);
        }
        AssertEqual(0, expectedHandlers.Count, "all six original handler cases present");
        Console.WriteLine("Original CPU warning comparison: 198 threshold/latch/suppression/queue cases and subsequent retry checks pass.");
    }

    private static void VerifyHealthWarning()
    {
        var warning = new SamusHealthWarningState();
        var audio = new CartridgeAudioState();
        byte[] positions = (byte[])typeof(CartridgeAudioState).GetField("_soundWritePositions", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(audio)!;
        byte[,] queues = (byte[,])typeof(CartridgeAudioState).GetField("_soundQueues", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(audio)!;
        foreach (var sample in new[] { (99, 0), (30, 2), (0, 0), (30, 0), (31, 1), (32, 0), (30, 2), (31, 1) })
        {
            audio.Reset();
            warning.Update((ushort)sample.Item1, audio);
            AssertEqual(sample.Item2 == 0 ? 0 : 1, positions[2], "warning queues only threshold transitions");
            if (sample.Item2 != 0) AssertEqual(sample.Item2, queues[2, 0], "warning native start/stop ID");
        }
        foreach (bool suppressed in new[] { false, true })
        {
            audio.Reset();
            if (!suppressed) for (int i = 0; i < 6; i++) audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, 6);
            warning.Update(30, audio, suppressed);
            AssertTrue(warning.IsActive, "rejected start still sets native latch");
            AssertEqual(suppressed ? 0 : 6, positions[2], "rejected start does not add a command");
            audio.Reset(); warning.Update(30, audio);
            AssertEqual(0, positions[2], "lost start is not retried at unchanged critical health");
            warning.Update(31, audio, soundSuppressed: true);
            AssertTrue(!warning.IsActive, "suppressed stop still clears latch");
            warning.Update(31, audio);
            AssertEqual(0, positions[2], "lost stop is not retried");
        }
        warning.Update(30, audio);
        using var saved = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(saved, warning); saved.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SamusHealthWarningState>(saved);
        audio.Reset(); restored.Update(30, audio);
        AssertTrue(restored.IsActive && positions[2] == 0, "saved active latch does not restart warning");
        Console.WriteLine("Low-health warning owner: threshold edges, native commands, rejection/suppression latch and serialization pass; frontend integration pending.");
    }
}
