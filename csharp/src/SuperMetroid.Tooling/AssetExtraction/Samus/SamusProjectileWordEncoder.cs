using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Writes Samus projectile state as the cartridge's bomb type and direction words (#1470), for tools that export native
/// traces beside the cartridge's. The game never reads these words.
/// </summary>
public static class SamusProjectileWordEncoder
{
    /// <summary>The position of a type word's family row.</summary>
    private const int FamilyShift = 8;

    /// <summary>
    /// The type word of a bomb slot's projectile, for tools that export bomb state beside the cartridge's: placed bombs
    /// and Power Bombs carry no live marker, spread bombs do, and a bomb that reacted to terrain sets bit zero.
    /// </summary>
    /// <param name="bomb">The bomb slot.</param>
    /// <returns>The type word; zero for a slot no bomb holds.</returns>
    public static int EncodeBombType(SamusBomb bomb)
    {
        Ensure.NotNull(bomb);
        if (bomb.Occupant is not { } placed)
            return EmptySlotTypeWord;
        int word = FamilyRow(placed.Type.Family) << FamilyShift;
        if (bomb.IsBombSpread)
            word |= LiveMarker;
        if (bomb.HasReactedToTerrain)
            word |= ReactedToTerrainBit;
        return word;
    }

    /// <summary>The type word the cartridge clears a slot no projectile holds to.</summary>
    private const int EmptySlotTypeWord = 0;

    /// <summary>The row a type word gives <paramref name="family"/>; rows four and six hold no family.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="family"/> is undefined.</exception>
    private static int FamilyRow(SamusProjectileFamily family) => family switch
    {
        SamusProjectileFamily.Beam => 0,
        SamusProjectileFamily.Missile => 1,
        SamusProjectileFamily.SuperMissile => 2,
        SamusProjectileFamily.PowerBomb => 3,
        SamusProjectileFamily.Bomb => 5,
        SamusProjectileFamily.BeamExplosion => 7,
        SamusProjectileFamily.MissileExplosion => 8,
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "Undefined projectile family."),
    };

    /// <summary>The direction word of a projectile's direction and spent flag.</summary>
    /// <param name="direction">The projectile's direction.</param>
    /// <param name="isSpent">Whether the projectile is spent.</param>
    public static int EncodeDirection(SamusProjectileDirection direction, bool isSpent) =>
        direction.ToAimIndex() | (isSpent ? SpentBit : 0);

    /// <summary>The live marker of a fired projectile's type word.</summary>
    private const int LiveMarker = 1 << 15;

    /// <summary>A placed bomb's "reacted to terrain" bit.</summary>
    private const int ReactedToTerrainBit = 1;

    /// <summary>The spent bit the game sets in a direction word.</summary>
    private const int SpentBit = 1 << 4;
}
