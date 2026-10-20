using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>Checks optional escape-time floor behavior for Ceres and Mother Brain countdowns.</summary>
internal static class EscapeTimerFloorVerification
{
    /// <summary>Confirms configuration bounds and that the one-second floor preserves normal countdown state until reached, then prevents expiration.</summary>
    internal static void VerifyEscapeTimerFloor()
    {
        AssertTrue(SuperMetroidGameOptionsIni.Parse("").EndingTimeOverrideMinutes is null, "ending override defaults off");
        AssertEqual(0, SuperMetroidGameOptionsIni.Parse("[Game]\nEndingTimeOverrideMinutes=0").EndingTimeOverrideMinutes, "fastest ending override");
        AssertEqual(5999, SuperMetroidGameOptionsIni.Parse("[Game]\nEndingTimeOverrideMinutes=5999").EndingTimeOverrideMinutes, "maximum ending time");
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
                AssertEqual((retail.State, retail.IsActive), (guarded.State, guarded.IsActive), "escape state unchanged by floor");
                AssertEqual(retail.Position, guarded.Position, "timer animation unchanged");
                if (guarded.Minutes.Raw == 0 && guarded.Seconds.Raw == 1 && guarded.Centiseconds.Raw == 0)
                    reachedFloor = true;
                if (reachedFloor)
                {
                    AssertEqual((byte)0, guarded.Minutes.Raw, "floor minutes");
                    AssertEqual((byte)1, guarded.Seconds.Raw, "floor seconds");
                    AssertEqual((byte)0, guarded.Centiseconds.Raw, "floor centiseconds");
                }
                else
                {
                    AssertEqual(retail.Minutes.Raw, guarded.Minutes.Raw, "normal minutes countdown");
                    AssertEqual(retail.Seconds.Raw, guarded.Seconds.Raw, "normal seconds countdown");
                    AssertEqual(retail.Centiseconds.Raw, guarded.Centiseconds.Raw, "normal fractional countdown");
                }
            }
            AssertTrue(reachedFloor, "both escape starts reach one second");
            for (ushort frame = 0; frame < 128; frame++)
            {
                guarded.SetTime(PackedBcd.FromDecimal(0), PackedBcd.FromDecimal(1), PackedBcd.FromDecimal(1));
                AssertTrue(!guarded.Process(frame, true), "either correction crossing floor cannot expire");
                AssertEqual((byte)1, guarded.Seconds.Raw, "crossing stops at one second");
                AssertEqual((byte)0, guarded.Centiseconds.Raw, "crossing stops at zero fractional seconds");
            }
            bool didExpire = false;
            for (ushort frame = 0; frame < 100; frame++)
                didExpire |= guarded.Process(frame, false);
            AssertTrue(didExpire, "disabling option restores countdown and expiration");
            guarded.Clear();
            guarded.Process(0, true);
            AssertEqual((byte)0, guarded.Seconds.Raw, "inactive timer remains cleared");
        }
        Console.WriteLine("Escape countdown floor verified for Ceres and Mother Brain.");
    }
}
