using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyEscapeTimerFloor()
    {
        AssertTrue(SuperMetroidGameOptionsIni.Parse("").EndingTimeOverrideMinutes is null, "ending override defaults off");
        AssertEqual((ushort?)0, SuperMetroidGameOptionsIni.Parse("[Game]\nEndingTimeOverrideMinutes=0").EndingTimeOverrideMinutes, "fastest ending override");
        AssertEqual((ushort?)5999, SuperMetroidGameOptionsIni.Parse("[Game]\nEndingTimeOverrideMinutes=5999").EndingTimeOverrideMinutes, "maximum ending time");
        AssertTrue(SuperMetroidGameOptionsIni.Parse("[Game]\nEndingTimeOverrideMinutes=None").EndingTimeOverrideMinutes is null, "ending override can be disabled");
        AssertThrows<InvalidDataException>(() => SuperMetroidGameOptionsIni.Parse("[Game]\nEndingTimeOverrideMinutes=6000"), "invalid ending time rejected");
        AssertTrue(!SuperMetroidGameOptionsIni.Parse("").PreventEscapeTimeout, "escape floor defaults off");
        AssertTrue(SuperMetroidGameOptionsIni.Parse("[Game]\nPreventEscapeTimeout=true").PreventEscapeTimeout, "escape floor INI enabled");
        AssertThrows<InvalidDataException>(() => SuperMetroidGameOptionsIni.Parse("[Game]\nPreventEscapeTimeout=maybe"), "escape floor rejects invalid boolean");

        foreach (bool ceres in new[] { true, false })
        {
            var guarded = new EscapeTimer();
            var retail = new EscapeTimer();
            if (ceres) { guarded.RequestCeresStart(); retail.RequestCeresStart(); }
            else { guarded.RequestMotherBrainStart(); retail.RequestMotherBrainStart(); }
            // Exercise the actual startup, movement and full countdown for both escapes.
            // Up to the floor, all timer fields must remain identical to retail behavior.
            bool reachedFloor = false;
            for (ushort frame = 0; frame < 12000; frame++)
            {
                bool expired = guarded.Process(frame, preventEscapeTimeout: true);
                retail.Process(frame);
                AssertTrue(!expired, "guarded escape never publishes expiration");
                AssertEqual(retail.RawStatus, guarded.RawStatus, "escape state unchanged by floor");
                AssertEqual(retail.XPositionFixed, guarded.XPositionFixed, "timer X animation unchanged");
                AssertEqual(retail.YPositionFixed, guarded.YPositionFixed, "timer Y animation unchanged");
                if (guarded.MinutesBcd == 0 && guarded.SecondsBcd == 1 && guarded.CentisecondsBcd == 0)
                    reachedFloor = true;
                if (reachedFloor)
                {
                    AssertEqual((byte)0, guarded.MinutesBcd, "floor minutes");
                    AssertEqual((byte)1, guarded.SecondsBcd, "floor seconds");
                    AssertEqual((byte)0, guarded.CentisecondsBcd, "floor centiseconds");
                }
                else
                {
                    AssertEqual(retail.MinutesBcd, guarded.MinutesBcd, "normal minutes countdown");
                    AssertEqual(retail.SecondsBcd, guarded.SecondsBcd, "normal seconds countdown");
                    AssertEqual(retail.CentisecondsBcd, guarded.CentisecondsBcd, "normal fractional countdown");
                }
            }
            AssertTrue(reachedFloor, "both escape starts reach one second");
            for (ushort frame = 0; frame < 128; frame++)
            {
                guarded.SetTime(0, 1, 1);
                AssertTrue(!guarded.Process(frame, true), "either correction crossing floor cannot expire");
                AssertEqual((byte)1, guarded.SecondsBcd, "crossing stops at one second");
                AssertEqual((byte)0, guarded.CentisecondsBcd, "crossing stops at zero fractional seconds");
            }
            bool didExpire = false;
            for (ushort frame = 0; frame < 100; frame++)
                didExpire |= guarded.Process(frame, false);
            AssertTrue(didExpire, "disabling option restores countdown and expiration");
            guarded.Clear();
            guarded.Process(0, true);
            AssertEqual((byte)0, guarded.SecondsBcd, "inactive timer remains cleared");
        }
        Console.WriteLine("Escape countdown floor verified for Ceres and Mother Brain.");
    }
}
