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

        SuperMetroidSaveSnapshot snapshot = SuperMetroidSaveSnapshot.Capture(new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), new SharedGameState(new SharedRandom()), new SaveSnapshotContext { Checkpoint = SuperMetroid.Core.Progression.LoadStationDefinitions.Gunship, GameTime = time });
        AssertEqual(59, snapshot.GameTimeFrames, "save captures game-time frames");
        AssertEqual(59, snapshot.GameTimeSeconds, "save captures game-time seconds");
        AssertEqual(59, snapshot.GameTimeMinutes, "save captures game-time minutes");
        AssertEqual(99, snapshot.GameTimeHours, "save captures game-time hours");

        // The clock holds its own domain (#1692): a component outside it is rejected, never stepped.
        foreach (OriginalClock outside in OutsideDomain)
        {
            string reading = $"{outside.Hours}:{outside.Minutes}:{outside.Seconds}.{outside.Frames}";
            AssertTrue(!GameTimeState.IsInDomain(outside.Frames, outside.Seconds, outside.Minutes, outside.Hours), $"{reading} is no clock reading");
            AssertThrows<ArgumentOutOfRangeException>(() => time.Load(outside.Frames, outside.Seconds, outside.Minutes, outside.Hours),
                $"game time rejects {reading}");
        }
        AssertEqual(59, time.Frames, "a rejected load leaves the clock unchanged");

        // Every reading of the clock steps as the original's clock does: from 0:00:00.00 the clock passes through each reading
        // once, in order, and saturates at the last.
        time.Load(frames: 0, seconds: 0, minutes: 0, hours: 0);
        var original = new OriginalClock(0, 0, 0, 0);
        int readings = 0;
        bool saturated = false;
        while (!saturated)
        {
            readings++;
            OriginalClock expected = OriginalClock.Step(original);
            time.Step();
            if (time.Frames != expected.Frames || time.Seconds != expected.Seconds ||
                time.Minutes != expected.Minutes || time.Hours != expected.Hours)
            {
                throw new InvalidOperationException(
                    $"{original} steps to {time.Hours}:{time.Minutes}:{time.Seconds}.{time.Frames}; the original's clock steps it to {expected}.");
            }
            saturated = expected.Frames == original.Frames && expected.Hours == original.Hours &&
                expected.Seconds == original.Seconds && expected.Minutes == original.Minutes;
            original = expected;
        }
        AssertTrue(GameTimeState.IsInDomain(original.Frames, original.Seconds, original.Minutes, original.Hours),
            "the clock saturates at a reading of its domain");
        AssertEqual(100 * 60 * 60 * 60, readings, "every reading of the clock was stepped");

        Console.WriteLine("  Game time: 60-Hz rollover, 99:59 saturation, domain rejection, every reading against the original's clock, and SRAM capture agree.");
    }

    /// <summary>Clock components just outside the clock's domain, one component at a time, and a save word's largest value.</summary>
    private static readonly OriginalClock[] OutsideDomain =
    [
        new(60, 0, 0, 0), new(0, 60, 0, 0), new(0, 0, 60, 0), new(0, 0, 0, 100),
        new(-1, 0, 0, 0), new(0, -1, 0, 0), new(0, 0, -1, 0), new(0, 0, 0, -1),
        new(ushort.MaxValue, 0, 0, 0), new(0, 0, 0, ushort.MaxValue),
    ];

    /// <summary>
    /// The four components of a clock reading, and a transcription of the original's clock step: each component a word,
    /// incremented and compared by its sign against 60, the hours saturating every component once they read 100 or more.
    /// </summary>
    private readonly record struct OriginalClock(int Frames, int Seconds, int Minutes, int Hours)
    {
        /// <summary>One step of the original's clock.</summary>
        internal static OriginalClock Step(OriginalClock clock)
        {
            ushort frames = (ushort)clock.Frames;
            ushort seconds = (ushort)clock.Seconds;
            ushort minutes = (ushort)clock.Minutes;
            ushort hours = (ushort)clock.Hours;
            if ((short)(++frames - 60) >= 0)
            {
                frames = 0;
                if ((short)(++seconds - 60) >= 0)
                {
                    seconds = 0;
                    if ((short)(++minutes - 60) >= 0)
                    {
                        minutes = 0;
                        ++hours;
                    }
                }
            }
            if ((short)(hours - 100) >= 0)
                return new OriginalClock(59, 59, 59, 99);
            return new OriginalClock(frames, seconds, minutes, hours);
        }
    }
}
