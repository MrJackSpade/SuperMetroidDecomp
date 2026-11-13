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
        var audio = AudioQueues.Standalone();
        var requests = GameplayAudioRequests.Reaching(audio);
        // The standalone queues are never advanced, so the room library's queue keeps every request since each reset.
        IReadOnlyList<SoundEffectId> roomRequests = audio.WaitingSounds(SoundEffectLibrary.Room);
        foreach (var sample in new[] { new VerifyHealthWarningCase(99, 0), new VerifyHealthWarningCase(30, 2), new VerifyHealthWarningCase(0, 0), new VerifyHealthWarningCase(30, 0), new VerifyHealthWarningCase(31, 1), new VerifyHealthWarningCase(32, 0), new VerifyHealthWarningCase(30, 2), new VerifyHealthWarningCase(31, 1) })
        {
            audio.Reset();
            warning.Update((ushort)sample.First, requests);
            AssertEqual(sample.Second == 0 ? 0 : 1, roomRequests.Count, "warning queues only threshold transitions");
            if (sample.Second != 0) AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Room, sample.Second), roomRequests[0], "warning native start/stop ID");
        }
        foreach (bool suppressed in new[] { false, true })
        {
            audio.Reset();
            if (!suppressed) for (int i = 0; i < 6; i++) audio.QueueSound(RoomSound.Silence, SoundRequestPriority.Normal);
            warning.Update(30, requests, suppressed);
            AssertTrue(warning.IsActive, "rejected start still sets native latch");
            AssertEqual(suppressed ? 0 : 6, roomRequests.Count, "rejected start does not add a command");
            audio.Reset(); warning.Update(30, requests);
            AssertEqual(0, roomRequests.Count, "lost start is not retried at unchanged critical health");
            warning.Update(31, requests, soundSuppressed: true);
            AssertTrue(!warning.IsActive, "suppressed stop still clears latch");
            warning.Update(31, requests);
            AssertEqual(0, roomRequests.Count, "lost stop is not retried");
        }
        warning.Update(30, requests);
        using var saved = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(saved, warning); saved.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SamusHealthWarningState>(saved);
        audio.Reset(); restored.Update(30, requests);
        AssertTrue(restored.IsActive && roomRequests.Count == 0, "saved active latch does not restart warning");
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
