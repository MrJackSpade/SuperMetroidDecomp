using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Samus;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the previous-position writes Samus applies before scrolling (#1627): a written X anywhere in the room's
/// coordinates replaces the checkpoint's whole pixel and keeps its fraction, a write past them is refused by the
/// coordinate itself and leaves the frame's writes pending, and the writes are consumed once.
/// </summary>
internal static class SamusPreviousPositionWritesVerification
{
    /// <summary>Checks the written X's range, the kept fractions and the single consumption.</summary>
    internal static void VerifySamusPreviousPositionWrites()
    {
        var checkpoint = FixedPosition<RoomPixels>.FromStoredParts(0x40, 0x1234, 0x80, 0xabcd);

        SamusState farthest = NewSamus();
        farthest.WritePreviousXPosition(ushort.MaxValue);
        AssertEqual(FixedPosition<RoomPixels>.FromStoredParts(ushort.MaxValue, 0x1234, 0x80, 0xabcd),
            farthest.ApplyPreviousPositionWrites(checkpoint), "a write at the room's last column replaces the whole pixel and keeps the fraction");
        AssertEqual(checkpoint, farthest.ApplyPreviousPositionWrites(checkpoint), "the write is consumed once");

        SamusState origin = NewSamus();
        origin.WritePreviousXPosition(0);
        AssertEqual(FixedPosition<RoomPixels>.FromStoredParts(0, 0x1234, 0x80, 0xabcd),
            origin.ApplyPreviousPositionWrites(checkpoint), "a write at the room's first column");

        SamusState past = NewSamus();
        past.WritePreviousXPosition(ushort.MaxValue + 1);
        AssertThrows<OverflowException>(() => past.ApplyPreviousPositionWrites(checkpoint),
            "a write past the room's coordinates is refused by the coordinate");
        AssertThrows<OverflowException>(() => past.ApplyPreviousPositionWrites(checkpoint),
            "a refused write stays pending rather than being consumed");

        SamusState negative = NewSamus();
        negative.WritePreviousXPosition(-1);
        AssertThrows<OverflowException>(() => negative.ApplyPreviousPositionWrites(checkpoint),
            "a write before the room's first column is refused by the coordinate");
        Console.WriteLine("Samus previous-position writes: any room column replaces the checkpoint's whole pixel once; writes past the coordinates are refused.");
    }

    /// <summary>A Samus with no writes recorded.</summary>
    private static SamusState NewSamus() =>
        new(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());

    /// <summary>Dispatches the previous-position write checks.</summary>
    [VerificationCommand("--samus-previous-position-writes")]
    internal static int SamusPreviousPositionWritesCommand()
    {
        Suite(nameof(VerifySamusPreviousPositionWrites), () => VerifySamusPreviousPositionWrites());
        return 0;
    }
}
