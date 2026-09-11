using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Desktop;

internal static partial class Program
{
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
