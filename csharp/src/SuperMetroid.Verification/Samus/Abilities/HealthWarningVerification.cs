using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Audio;

/// <summary>Checks the low-health warning latch and sound-queue threshold edges.</summary>
internal static class HealthWarningVerification
{
    /// <summary>Confirms start and stop IDs, suppression behavior, and debugger restoration of the active latch.</summary>
    internal static void VerifyHealthWarning()
    {
        var warning = new SamusHealthWarningState();
        var audio = new AudioQueues();
        byte[] positions = audio.SoundWritePositions;
        SoundEffectId?[,] queues = audio.SoundQueues;
        foreach (var sample in new[] { new VerifyHealthWarningCase(99, 0), new VerifyHealthWarningCase(30, 2), new VerifyHealthWarningCase(0, 0), new VerifyHealthWarningCase(30, 0), new VerifyHealthWarningCase(31, 1), new VerifyHealthWarningCase(32, 0), new VerifyHealthWarningCase(30, 2), new VerifyHealthWarningCase(31, 1) })
        {
            audio.Reset();
            warning.Update((ushort)sample.First, audio);
            AssertEqual(sample.Second == 0 ? 0 : 1, positions[2], "warning queues only threshold transitions");
            if (sample.Second != 0) AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Library3, sample.Second), queues[2, 0], "warning native start/stop ID");
        }
        foreach (bool suppressed in new[] { false, true })
        {
            audio.Reset();
            if (!suppressed) for (int i = 0; i < 6; i++) audio.QueueSound(Library3Sound.Silence, SoundQueueLimit.Max6);
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

    /// <summary>Runs the focused low-health warning owner check.</summary>
    [VerificationCommand("--health-warning")]
    internal static int HealthWarningCommand()
    {
        Suite(nameof(VerifyHealthWarning), () => VerifyHealthWarning());
        return 0;
    }
}
