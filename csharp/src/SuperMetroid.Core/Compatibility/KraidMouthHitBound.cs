using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The lower bound Kraid's inner mouth is left with after a projectile hits it. The mouth pass tests each later
/// projectile in the same pass against that bound with a wrapping signed comparison, so the value decides whether a
/// second shot in the same update also hits the mouth: a Super Missile's bound lets a shot low in the room through,
/// which doubles close-range damage. How the original game stored the projectile is not otherwise observable.
/// </summary>
/// <remarks>
/// Consulted by Kraid's inner-mouth pass after each accepted hit, before the next projectile is tested. The original
/// game reused the working word that held the hitting projectile's identity as the mouth rectangle's bottom; the Kraid
/// mouth damage checks and the Kraid recorded movie confirm the bound left by charged beams, missiles and Super Missiles.
/// Two bits the identity also held are not reproduced: a placed bomb's "already reacted to terrain" bit and the Charge
/// equipment bit an uncharged beam's explosion carries when Charge was equipped. Each moves the bound by at most one
/// pixel or 4096 pixels where it lies far below the bottom of Kraid's room, so no projectile in the room tests
/// differently.
/// </remarks>
[CompatibilityService("a second projectile in the same update is tested against the bound Kraid's mouth was left with by the first hit")]
[EncodingOwner("Kraid mouth bound left by a projectile hit")]
public static class KraidMouthHitBound
{
    /// <summary>The bound's step per projectile-family table row.</summary>
    private const int FamilyRowStep = 1 << 8;

    /// <summary>The bound's step for a charged beam, and for the Charge equipment every charged beam carries.</summary>
    private const int ChargedStep = (1 << 4) | (1 << 12);

    /// <summary>The bound's step for an echo.</summary>
    private const int EchoStep = 1 << 5;

    /// <summary>The bound's step for every projectile except a placed bomb or Power Bomb.</summary>
    private const int FiredStep = 1 << 15;

    /// <summary>The bound Kraid's mouth is left with after <paramref name="type"/> hits it.</summary>
    /// <param name="type">The projectile that hit the mouth.</param>
    /// <param name="isPlacedBomb">Whether the projectile is a placed bomb or Power Bomb rather than a fired or spread one.</param>
    /// <returns>The bound, read as a wrapping signed pixel row.</returns>
    public static int After(SamusProjectileType type, bool isPlacedBomb)
    {
        int bound = type.Family.TableRow * FamilyRowStep + type.BeamCombination.TableIndex;
        if (type.IsChargedBeam)
            bound += ChargedStep;
        if (type.Echo is not null)
            bound += EchoStep;
        if (!isPlacedBomb)
            bound += FiredStep;
        return bound;
    }
}
