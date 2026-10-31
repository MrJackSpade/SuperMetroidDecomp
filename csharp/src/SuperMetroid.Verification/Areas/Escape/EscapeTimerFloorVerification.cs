using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>Checks optional escape-time floor behavior for Ceres and Mother Brain countdowns.</summary>
internal static class EscapeTimerFloorVerification
{
    /// <summary>Confirms configuration bounds and that the one-second floor preserves normal countdown state until reached, then prevents expiration.</summary>
    internal static void VerifyEscapeTimerFloor()
    {
        AssertTrue(HostOptionsIni.Parse("").Game.EndingTimeOverrideMinutes is null, "ending override defaults off");
        AssertEqual(0, HostOptionsIni.Parse("[Game]\nEndingTimeOverrideMinutes=0").Game.EndingTimeOverrideMinutes, "fastest ending override");
        AssertEqual(5999, HostOptionsIni.Parse("[Game]\nEndingTimeOverrideMinutes=5999").Game.EndingTimeOverrideMinutes, "maximum ending time");
        AssertTrue(HostOptionsIni.Parse("[Game]\nEndingTimeOverrideMinutes=None").Game.EndingTimeOverrideMinutes is null, "ending override can be disabled");
        AssertThrows<InvalidDataException>(() => HostOptionsIni.Parse("[Game]\nEndingTimeOverrideMinutes=6000"), "invalid ending time rejected");
        AssertTrue(!HostOptionsIni.Parse("").Game.PreventEscapeTimeout, "escape floor defaults off");
        AssertTrue(HostOptionsIni.Parse("[Game]\nPreventEscapeTimeout=true").Game.PreventEscapeTimeout, "escape floor INI enabled");
        AssertThrows<InvalidDataException>(() => HostOptionsIni.Parse("[Game]\nPreventEscapeTimeout=maybe"), "escape floor rejects invalid boolean");

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
                AssertEqual(new VerifyEscapeTimerFloorCase(retail.Phase, retail.IsActive), new VerifyEscapeTimerFloorCase(guarded.Phase, guarded.IsActive), "escape phase unchanged by floor");
                AssertEqual(retail.Position, guarded.Position, "timer animation unchanged");
                if (guarded.RemainingCentiseconds == EscapeTimerClock.Time(0, 1, 0))
                    reachedFloor = true;
                if (reachedFloor)
                    AssertEqual(EscapeTimerClock.Time(0, 1, 0), guarded.RemainingCentiseconds, "floor holds one second");
                else
                    AssertEqual(retail.RemainingCentiseconds, guarded.RemainingCentiseconds, "normal countdown");
            }
            AssertTrue(reachedFloor, "both escape starts reach one second");
            for (ushort frame = 0; frame < 128; frame++)
            {
                guarded.SetRemaining(EscapeTimerClock.Time(0, 1, 1));
                AssertTrue(!guarded.Process(frame, true), "either correction crossing floor cannot expire");
                AssertEqual(EscapeTimerClock.Time(0, 1, 0), guarded.RemainingCentiseconds, "crossing stops at one second");
            }
            bool didExpire = false;
            for (ushort frame = 0; frame < 100; frame++)
                didExpire |= guarded.Process(frame, false);
            AssertTrue(didExpire, "disabling option restores countdown and expiration");
            guarded.Clear();
            guarded.Process(0, true);
            AssertEqual(0, guarded.RemainingCentiseconds, "inactive timer remains cleared");
        }
        Console.WriteLine("Escape countdown floor verified for Ceres and Mother Brain.");
    }
}
