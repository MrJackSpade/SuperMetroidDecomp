namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The lower bound Kraid's inner mouth is left with after a projectile hits it. The mouth pass tests each later
/// projectile in the same pass against that bound with a wrapping signed comparison, so the value decides whether a
/// second shot in the same update also hits the mouth: a missile's bound lets through a later shot whose top lies at
/// row 257 or lower in the room, while a Super Missile's bound lets through only a top at row 513 or lower, beneath the
/// 512 rows of Kraid's two-screen room, so no second shot follows a Super Missile in; a clean rectangle would let it in
/// and double close-range damage. How the game stored the projectile is not otherwise observable.
/// </summary>
/// <remarks>
/// Consulted by Kraid's inner-mouth pass after each accepted hit, before the next projectile is tested. Evidence: the
/// original game reused the working word that held the hitting projectile's identity as the mouth rectangle's bottom; the Kraid
/// mouth damage checks and the Kraid recorded movie confirm the bound left by charged beams, missiles and Super Missiles.
/// Three parts the identity also held are not reproduced. The beam combination moved the bound by at most fifteen
/// pixels: no missile or bomb carries beams, every beam that hits the mouth is charged, and every beam's bound, its
/// explosion's included, lies thousands of pixels outside the room. A placed bomb's "already reacted to terrain" bit and the Charge equipment bit an
/// uncharged beam's explosion carries when Charge was equipped each move the bound by at most one pixel or 4096 pixels
/// where it lies far below the bottom of Kraid's room. So no projectile in the room tests differently.
/// </remarks>
[CompatibilityService("a second projectile in the same update is tested against the bound Kraid's mouth was left with by the first hit")]
[EncodingOwner("Kraid mouth bound left by a projectile hit")]
public static class KraidMouthHitBound
{
    /// <summary>The family of the projectile that hit the mouth.</summary>
    public enum Family
    {
        /// <summary>A beam shot, charged or an echo.</summary>
        Beam,
        /// <summary>An ordinary missile.</summary>
        Missile,
        /// <summary>A Super Missile or its collision link.</summary>
        SuperMissile,
        /// <summary>A Power Bomb.</summary>
        PowerBomb,
        /// <summary>A Morph Ball bomb.</summary>
        Bomb,
        /// <summary>The explosion a beam leaves.</summary>
        BeamExplosion,
        /// <summary>The explosion a missile or Super Missile leaves.</summary>
        MissileExplosion,
    }

    /// <summary>How the projectile that hit the mouth was delivered.</summary>
    public enum Hit
    {
        /// <summary>A placed bomb or Power Bomb.</summary>
        PlacedBomb,
        /// <summary>A fired or spread projectile that is neither charged nor an echo.</summary>
        Fired,
        /// <summary>A fired charged beam.</summary>
        FiredChargedBeam,
        /// <summary>A fired echo.</summary>
        FiredEcho,
        /// <summary>A fired charged echo.</summary>
        FiredChargedEcho,
    }

    /// <summary>The bound's step per place of the family.</summary>
    private const int FamilyRowStep = 1 << 8;

    /// <summary>The bound's step for a charged beam, and for the Charge equipment every charged beam carries.</summary>
    private const int ChargedStep = (1 << 4) | (1 << 12);

    /// <summary>The bound's step for an echo.</summary>
    private const int EchoStep = 1 << 5;

    /// <summary>The bound's step for every projectile except a placed bomb or Power Bomb.</summary>
    private const int FiredStep = 1 << 15;

    /// <summary>The bound Kraid's mouth is left with after a projectile hits it.</summary>
    /// <param name="family">The family of the projectile that hit the mouth.</param>
    /// <param name="hit">How the projectile was delivered: placed, or fired and whether charged or an echo.</param>
    /// <returns>The bound, read as a wrapping signed pixel row.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="family"/> or <paramref name="hit"/> is undefined.</exception>
    public static int After(Family family, Hit hit) => Place(family) * FamilyRowStep + hit switch
    {
        Hit.PlacedBomb => 0,
        Hit.Fired => FiredStep,
        Hit.FiredChargedBeam => FiredStep + ChargedStep,
        Hit.FiredEcho => FiredStep + EchoStep,
        Hit.FiredChargedEcho => FiredStep + ChargedStep + EchoStep,
        _ => throw new ArgumentOutOfRangeException(nameof(hit), hit, "Undefined mouth hit."),
    };

    /// <summary>Each family's place in the identity the game left as the bound; places four and six hold no family.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="family"/> is undefined.</exception>
    private static int Place(Family family) => family switch
    {
        Family.Beam => 0,
        Family.Missile => 1,
        Family.SuperMissile => 2,
        Family.PowerBomb => 3,
        Family.Bomb => 5,
        Family.BeamExplosion => 7,
        Family.MissileExplosion => 8,
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "Undefined mouth-hit family."),
    };
}
