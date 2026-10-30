using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The codec of the cartridge's Samus projectile words (#1470): the type word, the direction word and the missile
/// variable word. The game holds <see cref="SamusProjectileType"/>, a <see cref="SamusProjectileDirection"/> with a
/// spent flag, and a missile's <see cref="SamusMissileIgnition"/> and link slot; this is the one place their bit layout
/// is known for checks that compare the game's state with the cartridge's. The game never reads these words; tools that
/// write them use Tooling's <c>SamusProjectileWordEncoder</c>.
/// </summary>
[EncodingOwner("Samus projectile type, direction and missile variable words, as checks decode them")]
internal static class SamusProjectileWordCodec
{
    /// <summary>The beam combination bits of a type word.</summary>
    private const int BeamCombinationMask = (1 << 4) - 1;

    /// <summary>The charged-beam bit of a type word.</summary>
    private const int ChargedBit = 1 << 4;

    /// <summary>The echo bit of a type word.</summary>
    private const int EchoBit = 1 << 5;

    /// <summary>The position of a type word's family row.</summary>
    private const int FamilyShift = 8;

    /// <summary>The bits of a type word's family row.</summary>
    private const int FamilyRowMask = (1 << 4) - 1;

    /// <summary>The direction bits of a direction word.</summary>
    private const int DirectionMask = (1 << 4) - 1;

    /// <summary>The bits of a direction word's low byte that mark a projectile spent.</summary>
    private const int SpentMask = ((1 << 4) - 1) << 4;

    /// <summary>The bits of a missile variable word holding a reflected missile's head start.</summary>
    private const int ReflectionHeadStart = 240;

    /// <summary>The position of a missile variable word's ignition byte.</summary>
    private const int IgnitionShift = 8;

    /// <summary>The bits of a missile variable word's low byte: the link's slot, twice over, once ignited.</summary>
    private const int LowByteMask = (1 << 8) - 1;

    /// <summary>The families in the order of their rows in a type word.</summary>
    private static readonly SamusProjectileFamily?[] FamilyRows =
    [
        SamusProjectileFamily.Beam, SamusProjectileFamily.Missile, SamusProjectileFamily.SuperMissile,
        SamusProjectileFamily.PowerBomb, null, SamusProjectileFamily.Bomb, null, SamusProjectileFamily.BeamExplosion,
        SamusProjectileFamily.MissileExplosion,
    ];

    /// <summary>The projectile a type word records; zero is <see cref="SamusProjectileType.None"/>.</summary>
    /// <param name="word">A projectile type word.</param>
    /// <returns>The projectile. A placed bomb's "reacted to terrain" bit and an uncharged beam's Charge equipment bit are not part of it.</returns>
    /// <exception cref="InvalidDataException">The word's family row names no family, or its echo bits name no echo.</exception>
    public static SamusProjectileType DecodeType(int word)
    {
        if (word == 0)
            return SamusProjectileType.None;
        int row = (word >> FamilyShift) & FamilyRowMask;
        SamusProjectileFamily family = (row < FamilyRows.Length ? FamilyRows[row] : null)
            ?? throw new InvalidDataException($"Projectile type word ${word:X4} names no family.");
        int beamBits = word & BeamCombinationMask;
        if (family == SamusProjectileFamily.Bomb)
            beamBits = 0;
        SamusProjectileType beam;
        if ((word & EchoBit) != 0)
        {
            // Only two beam combinations name an echo; every other one is malformed data.
            SamusBeamCombination echoCombination = SamusBeamCombinations.FromTableIndex(beamBits);
            SamusBeamEcho echo = echoCombination == SamusBeamCombination.Spazer ? SamusBeamEcho.SpazerComboTrail
                : echoCombination == SamusBeamCombination.PlasmaWave ? SamusBeamEcho.ShinesparkEcho
                : throw new InvalidDataException($"Projectile type word ${word:X4} names no echo ({echoCombination}).");
            beam = SamusProjectileType.EchoOf(echo);
        }
        else
        {
            beam = SamusProjectileType.Beam(SamusBeamCombinations.FromTableIndex(beamBits), (word & ChargedBit) != 0);
        }
        return beam.AsFamily(family);
    }

    /// <summary>The direction a direction word records.</summary>
    /// <param name="word">A projectile direction word.</param>
    /// <returns>The direction.</returns>
    /// <exception cref="InvalidDataException">The word's direction bits name none of the ten directions.</exception>
    public static SamusProjectileDirection DecodeDirection(int word) =>
        SamusProjectileDirections.At((word & DirectionMask) is var index and < SamusProjectileDirectionCount
            ? index
            : throw new InvalidDataException($"Projectile direction word ${word:X4} names no direction."));

    /// <summary>Whether a direction word marks its projectile spent.</summary>
    /// <param name="word">A projectile direction word.</param>
    public static bool DecodeSpent(int word) => (word & SpentMask) != 0;

    /// <summary>The ignition a missile's variable word records.</summary>
    /// <param name="word">A missile or Super Missile's variable word.</param>
    public static SamusMissileIgnition DecodeMissileIgnition(int word) =>
        (word >> IgnitionShift) != 0 ? SamusMissileIgnition.Ignited
        : (word & LowByteMask) == ReflectionHeadStart ? SamusMissileIgnition.PendingAfterReflection
        : SamusMissileIgnition.Pending;

    /// <summary>
    /// The slot a Super Missile's variable word names as its link, or null when it names none; a word whose low byte
    /// is not a slot's position names none.
    /// </summary>
    /// <param name="word">A missile or Super Missile's variable word.</param>
    public static int? DecodeMissileLinkSlot(int word)
    {
        int offset = word & LowByteMask;
        return offset % 2 == 0 && offset / 2 < SamusProjectileSystem.SlotCount ? offset / 2 : null;
    }

    /// <summary>The number of directions a direction word names.</summary>
    private const int SamusProjectileDirectionCount = 10;
}
