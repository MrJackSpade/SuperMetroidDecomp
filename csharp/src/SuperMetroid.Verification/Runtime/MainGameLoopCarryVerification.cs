using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Saves;
namespace SuperMetroid.Verification.Runtime;
using SuperMetroid.Verification.Harness;

/// <summary>Checks how the save file found at power-on lengthens the earthquake rumble's delay (#1475).</summary>
internal static class MainGameLoopCarryVerification
{
    /// <summary>
    /// Confirms each power-on case of <see cref="PowerOnEarthquakeSoundDelay"/> (no valid save, a valid save after a
    /// power-on that found none, a valid save without either, a completed game) and that the rumble's delay reload
    /// adds its frame, preserving the native rumble cadence.
    /// </summary>
    internal static void VerifyMainGameLoopCarry()
    {
        var saveRam = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        AssertEqual(1, saveRam.EarthquakeSoundDelayAtPowerOn().ExtraFrames, "no valid save: power-on adds a frame");
        _ = saveRam.DetermineCompletionDemoSetUnlocked();
        saveRam.SaveSlot(SaveSlot.A, SaveSnapshotFixtures.NewGame);
        AssertEqual(1, saveRam.EarthquakeSoundDelayAtPowerOn().ExtraFrames,
            "a valid save after a power-on that found none adds a frame, as the kept marker does (#2140)");
        saveRam.RestoreHistory(SaveFileHistory.None);
        AssertEqual(0, saveRam.EarthquakeSoundDelayAtPowerOn().ExtraFrames, "a valid save without either history adds none");
        saveRam.CompleteGame();
        AssertEqual(1, saveRam.EarthquakeSoundDelayAtPowerOn().ExtraFrames, "a completed game adds a frame");

        foreach (PowerOnEarthquakeSoundDelay delay in new[] { PowerOnEarthquakeSoundDelay.OneFrame, PowerOnEarthquakeSoundDelay.None })
        {
            var state = new RoomFxOverlayState();
            state.BindAudioRequests(GameplayAudioRequests.Standalone());
            // A new effect's quake sound is due on its next count and starts its rhythm from the first delay.
            // Native update 330722: the first entry (base 1) with RNG & 3 = 0 reloads to 2 when the carry is set.
            state.PublishStatueEarthquakeSound(0xac0c, delay);
            AssertEqual((ushort)(1 + delay.ExtraFrames), state.EarthquakeSoundTimer.RawValue,
                $"earthquake reload with {delay.ExtraFrames} power-on frames");
        }
        Console.WriteLine("Power-on rumble delay: the save file decides it, and the earthquake timer reload adds it.");
    }

    /// <summary>Runs the focused power-on rumble delay check.</summary>
    [VerificationCommand("--main-game-loop-carry")]
    internal static int MainGameLoopCarryCommand()
    {
        VerifyMainGameLoopCarry();
        return 0;
    }
}
