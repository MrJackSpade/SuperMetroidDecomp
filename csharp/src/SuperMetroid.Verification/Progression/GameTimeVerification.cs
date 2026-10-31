using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Verification.Progression;

/// <summary>Checks 60-frame time rollover, the 99:59:59 saturation limit, and save-snapshot time fields.</summary>
internal static class GameTimeVerification
{
    /// <summary>Checks `$82:DB69` rollover, saturation, and save-snapshot ownership.</summary>
    internal static void VerifyGameTimeState()
    {
        var time = new GameTimeState();
        for (int frame = 0; frame < 59; frame++)
            time.Step();
        AssertEqual(59, time.Frames, "game time frame 59");
        AssertEqual(0, time.Seconds, "game time before first second");

        time.Step();
        AssertEqual(0, time.Frames, "game time frame rollover");
        AssertEqual(1, time.Seconds, "game time second increment");

        time.Load(frames: 59, seconds: 59, minutes: 59, hours: 98);
        time.Step();
        AssertEqual(0, time.Frames, "game time hour rollover frames");
        AssertEqual(0, time.Seconds, "game time hour rollover seconds");
        AssertEqual(0, time.Minutes, "game time hour rollover minutes");
        AssertEqual(99, time.Hours, "game time hour rollover hours");

        time.Load(frames: 59, seconds: 59, minutes: 59, hours: 99);
        time.Step();
        AssertEqual(59, time.Frames, "game time saturation frames");
        AssertEqual(59, time.Seconds, "game time saturation seconds");
        AssertEqual(59, time.Minutes, "game time saturation minutes");
        AssertEqual(99, time.Hours, "game time saturation hours");

        SuperMetroidSaveSnapshot snapshot = SuperMetroidSaveSnapshot.Capture(new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), new SharedGameState(), new SaveSnapshotContext { Area = 0, SaveStation = 0, GameTime = time });
        AssertEqual(59, snapshot.GameTimeFrames, "save captures game-time frames");
        AssertEqual(59, snapshot.GameTimeSeconds, "save captures game-time seconds");
        AssertEqual(59, snapshot.GameTimeMinutes, "save captures game-time minutes");
        AssertEqual(99, snapshot.GameTimeHours, "save captures game-time hours");

        // A loaded save may hold any save-field value; the clock accepts it and steps it as the cartridge does.
        time.Load(frames: 65535, seconds: 0, minutes: 0, hours: 150);
        AssertEqual(65535, time.Frames, "game time loads a corrupt frame count unchanged");
        AssertEqual(150, time.Hours, "game time loads a corrupt hour unchanged");
        AssertThrows<ArgumentOutOfRangeException>(() => time.Load(frames: 65536, seconds: 0, minutes: 0, hours: 0),
            "game time rejects a component past a save field");
        AssertThrows<ArgumentOutOfRangeException>(() => time.Load(frames: 0, seconds: 0, minutes: 0, hours: -1),
            "game time rejects a negative component");
        time.Step();
        AssertEqual(59, time.Frames, "a corrupt hour saturates frames on the next step");
        AssertEqual(59, time.Seconds, "a corrupt hour saturates seconds on the next step");
        AssertEqual(59, time.Minutes, "a corrupt hour saturates minutes on the next step");
        AssertEqual(99, time.Hours, "a corrupt hour saturates to 99 on the next step");

        time.Load(frames: 65535, seconds: 7, minutes: 3, hours: 1);
        time.Step();
        AssertEqual(0, time.Frames, "65535 frames wrap to zero as a sixteen-bit increment");
        AssertEqual(7, time.Seconds, "a wrapped frame count is below 60, so no second is added");
        AssertEqual(3, time.Minutes, "a wrapped frame count leaves the minutes");
        AssertEqual(1, time.Hours, "a wrapped frame count leaves the hours");

        Console.WriteLine("  Game time: 60-Hz rollover, 99:59 saturation, and SRAM capture agree.");
    }
}
