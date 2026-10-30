using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Writes Samus projectile state as the cartridge's bomb type and direction words (#1470), for tools that export native
/// traces beside the cartridge's. The game never reads these words.
/// </summary>
[EncodingOwner("Samus bomb type and projectile direction words, as tools write them")]
public static class SamusProjectileWordEncoder
{
    /// <summary>The position of a type word's family row.</summary>
    private const int FamilyShift = 8;

    /// <summary>
    /// The type word of a bomb slot's projectile, for tools that export bomb state beside the cartridge's: placed bombs
    /// and Power Bombs carry no live marker, spread bombs do, and a bomb that reacted to terrain sets bit zero.
    /// </summary>
    /// <param name="type">The bomb slot's projectile.</param>
    /// <param name="isBombSpread">Whether the bomb is one of a bomb spread's five.</param>
    /// <param name="hasReactedToTerrain">Whether the bomb's explosion has reacted to the terrain around it.</param>
    /// <returns>The type word; zero for <see cref="SamusProjectileType.None"/>.</returns>
    public static int EncodeBombType(SamusProjectileType type, bool isBombSpread, bool hasReactedToTerrain)
    {
        if (type.IsNone)
            return 0;
        int word = type.Family.TableRow << FamilyShift;
        if (isBombSpread)
            word |= LiveMarker;
        if (hasReactedToTerrain)
            word |= ReactedToTerrainBit;
        return word;
    }

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
