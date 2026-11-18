namespace SuperMetroid.Verification.Bosses.Kraid;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the rows from which a later projectile in the same pass still reaches Kraid's mouth after each kind of damaging
/// hit (#2507): in Kraid's 512-row room a missile lets later shots in from row 257 down, a placed bomb or Power Bomb lets
/// every row in, and a charged beam, a Super Missile or a spread bomb lets no row in; across the whole wrapping room
/// space each kind's bounds test a later shot's top exactly as the original's signed comparison with the hitting
/// projectile's identity did.
/// </summary>
internal static class KraidMouthHitReachVerification
{
    /// <summary>The rows of Kraid's two-screen room.</summary>
    private const int RoomRows = 512;

    /// <summary>
    /// The identity the original left as the mouth's bottom for each kind of damaging hit, read independently of the
    /// production service: the family's row in the upper byte, the fired mark above it for everything but a placed bomb,
    /// and for a charged Power Beam the charged and Charge equipment marks.
    /// </summary>
    private static readonly (KraidMouthHitReach.DamagingHit Hit, int OriginalBottom)[] OriginalBottoms =
    [
        (KraidMouthHitReach.DamagingHit.ChargedBeam, 0x9010),
        (KraidMouthHitReach.DamagingHit.Missile, 0x8100),
        (KraidMouthHitReach.DamagingHit.SuperMissile, 0x8200),
        (KraidMouthHitReach.DamagingHit.PlacedBomb, 0x0500),
        (KraidMouthHitReach.DamagingHit.SpreadBomb, 0x8500),
        (KraidMouthHitReach.DamagingHit.PlacedPowerBomb, 0x0300),
    ];

    /// <summary>
    /// Each kind of damaging hit's outcome inside Kraid's room, and its bounds against the original's comparison for every
    /// top a later shot can have across the wrapping room space.
    /// </summary>
    internal static void VerifyKraidMouthHitReach()
    {
        AssertEqual(Enum.GetValues<KraidMouthHitReach.DamagingHit>().Length, OriginalBottoms.Length, "every kind of damaging hit has its original bottom");
        foreach ((KraidMouthHitReach.DamagingHit hit, int originalBottom) in OriginalBottoms)
        {
            KraidPassBounds bounds = KraidPassBounds.Placed(left: 0, top: 0, bottom: 0);
            bounds.LeaveAfterMouthHit(KraidMouthHitReach.RowAfter(hit));
            int? firstDifference = null;
            for (int top = short.MinValue; top <= short.MaxValue && firstDifference is null; top++)
            {
                bool original = unchecked((short)(top - 1 - originalBottom)) < 0;
                if (bounds.IsAboveBottom(top) != original)
                    firstDifference = top;
            }
            AssertEqual<int?>(null, firstDifference, $"{hit}: every later top tests as the original's comparison with its identity");
            AssertTrue(bounds.ReachesTopAndLeft(shotBottom: -RoomRows, shotRight: 0), $"{hit}: the top it leaves lies far above the room");
        }

        AssertEqual(RoomRows - 257, RowsReached(KraidMouthHitReach.DamagingHit.Missile, 0), "a missile lets in exactly the shots from row 257 down");
        AssertTrue(!Reaches(KraidMouthHitReach.DamagingHit.Missile, 256) && Reaches(KraidMouthHitReach.DamagingHit.Missile, 257),
            "a missile's reach starts at row 257");
        foreach (KraidMouthHitReach.DamagingHit none in new[]
            { KraidMouthHitReach.DamagingHit.ChargedBeam, KraidMouthHitReach.DamagingHit.SuperMissile, KraidMouthHitReach.DamagingHit.SpreadBomb })
            AssertEqual(0, RowsReached(none, 0), $"{none} lets no row of the room in");
        AssertTrue(!Reaches(KraidMouthHitReach.DamagingHit.SuperMissile, RoomRows) && Reaches(KraidMouthHitReach.DamagingHit.SuperMissile, RoomRows + 1),
            "a Super Missile still lets in a shot below the room's floor from row 513");
        foreach (KraidMouthHitReach.DamagingHit every in new[]
            { KraidMouthHitReach.DamagingHit.PlacedBomb, KraidMouthHitReach.DamagingHit.PlacedPowerBomb })
        {
            AssertEqual(RoomRows, RowsReached(every, 0), $"{every} lets every row of the room in");
            AssertTrue(Reaches(every, -64), $"{every} lets in a shot above the room's top");
        }
        Console.WriteLine("Kraid mouth reach: every kind of damaging hit's rows agree with the original's comparison across the room space.");
    }

    /// <summary>How many of the room's rows from <paramref name="first"/> on let a later shot reach the mouth after <paramref name="hit"/>.</summary>
    private static int RowsReached(KraidMouthHitReach.DamagingHit hit, int first) =>
        Enumerable.Range(first, RoomRows - first).Count(top => Reaches(hit, top));

    /// <summary>Whether a later shot whose top lies at <paramref name="top"/> reaches the mouth after <paramref name="hit"/>.</summary>
    private static bool Reaches(KraidMouthHitReach.DamagingHit hit, int top)
    {
        KraidPassBounds bounds = KraidPassBounds.Placed(left: 0, top: 0, bottom: 0);
        bounds.LeaveAfterMouthHit(KraidMouthHitReach.RowAfter(hit));
        return bounds.IsAboveBottom(top);
    }

    /// <summary>Runs the focused Kraid mouth reach check.</summary>
    [VerificationCommand("--kraid-mouth-hit-reach")]
    internal static int KraidMouthHitReachCommand()
    {
        SuiteTimingVerification.Suite(nameof(VerifyKraidMouthHitReach), VerifyKraidMouthHitReach);
        return 0;
    }
}
