using SuperMetroid.Core.Game;

internal static partial class Program
{
    /// <summary>Checks `$82:DB69` rollover, saturation, and save-snapshot ownership.</summary>
    static void VerifyGameTimeState()
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

        SuperMetroidSaveSnapshot snapshot = SuperMetroidSaveSnapshot.Capture(
            new SamusState(),
            new Bank80SystemState(),
            area: 0,
            saveStation: 0,
            gameTime: time);
        AssertEqual(59, snapshot.GameTimeFrames, "save captures game-time frames");
        AssertEqual(59, snapshot.GameTimeSeconds, "save captures game-time seconds");
        AssertEqual(59, snapshot.GameTimeMinutes, "save captures game-time minutes");
        AssertEqual(99, snapshot.GameTimeHours, "save captures game-time hours");

        Console.WriteLine("  Game time: 60-Hz rollover, 99:59 saturation, and SRAM capture agree.");
    }
}
