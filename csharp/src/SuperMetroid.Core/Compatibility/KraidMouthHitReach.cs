namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which later projectiles in the same update still reach Kraid's inner mouth after a projectile damages it: those whose
/// top lies at or below the row the damaging hit names (<see cref="HighestReachingRow"/>), read across the wrapping room space, so
/// the 32768 rows from that row on reach the mouth and the rows above it do not. In Kraid's 512-row, two-screen room a
/// missile lets a later shot in from row 257 down; a placed bomb or Power Bomb lets every row of the room in; a charged
/// beam, a Super Missile or a spread bomb lets no row of the room in, so no second shot follows a Super Missile in, where a
/// clean rectangle would let it in and double close-range damage.
/// </summary>
/// <remarks>
/// Consulted by Kraid's inner-mouth pass after each damaging hit, before the next projectile is tested; a hit that does no
/// damage leaves its dud's bounds instead (<see cref="KraidPassBounds.LeaveAfterDud"/>). Each row is kept exactly as the
/// game left it, past the room's edges too, so a projectile the movement window keeps alive below the room's floor tests
/// as it did: one whose top lies at row 513 still reaches the mouth after a Super Missile. Evidence: the original game
/// reused the working word that held the hitting projectile's identity as the mouth rectangle's bottom, and tested the
/// next projectile's top less one against it by the sign of their difference, so each kind of hit lets in the half of
/// the wrapping space that starts one row past its identity, half the space on; the Kraid mouth damage checks compare
/// each kind's rows with that test across the whole space, and the Kraid recorded movie confirms the charged beams',
/// missiles' and Super Missiles' outcomes.
/// </remarks>
public static partial class KraidMouthHitReach
{
    /// <summary>The kinds of projectile hit that damage Kraid's mouth.</summary>
    public enum DamagingHit
    {
        /// <summary>A fired charged beam; an uncharged beam or an echo never hits the mouth.</summary>
        ChargedBeam,
        /// <summary>An ordinary missile.</summary>
        Missile,
        /// <summary>A Super Missile or its collision link.</summary>
        SuperMissile,
        /// <summary>A Morph Ball bomb Samus placed.</summary>
        PlacedBomb,
        /// <summary>A bomb of a bomb spread.</summary>
        SpreadBomb,
        /// <summary>A Power Bomb Samus placed.</summary>
        PlacedPowerBomb,
    }

    /// <summary>The row a charged beam's hit leaves: far below the room, so no later projectile in the room reaches the mouth.</summary>
    private const int ChargedBeamRow = 4113;

    /// <summary>The row a missile's hit leaves: a later projectile whose top lies in the lower half of the room reaches the mouth.</summary>
    private const int MissileRow = 257;

    /// <summary>
    /// The row a Super Missile's hit leaves: the first row below Kraid's 512-row room, so no later projectile inside the room
    /// reaches the mouth, only one kept alive below the room's floor.
    /// </summary>
    private const int SuperMissileRow = 513;

    /// <summary>The row a placed bomb's hit leaves: far above the room, so a later projectile on every row of the room reaches the mouth.</summary>
    private const int PlacedBombRow = -31487;

    /// <summary>The row a spread bomb's hit leaves: below the room, so no later projectile in the room reaches the mouth.</summary>
    private const int SpreadBombRow = 1281;

    /// <summary>The row a placed Power Bomb's hit leaves: far above the room, so a later projectile on every row of the room reaches the mouth.</summary>
    private const int PlacedPowerBombRow = -31999;

    /// <summary>
    /// The room row from which a later projectile in the same pass reaches Kraid's mouth after <paramref name="hit"/>: one
    /// whose top lies at this row or at most 32767 rows below it, across the wrapping room space.
    /// </summary>
    /// <param name="hit">The kind of hit that damaged the mouth.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="hit"/> is undefined.</exception>
    public static int HighestReachingRow(DamagingHit hit) => hit switch
    {
        DamagingHit.ChargedBeam => ChargedBeamRow,
        DamagingHit.Missile => MissileRow,
        DamagingHit.SuperMissile => SuperMissileRow,
        DamagingHit.PlacedBomb => PlacedBombRow,
        DamagingHit.SpreadBomb => SpreadBombRow,
        DamagingHit.PlacedPowerBomb => PlacedPowerBombRow,
        _ => throw new ArgumentOutOfRangeException(nameof(hit), hit, "Undefined damaging mouth hit."),
    };
}
