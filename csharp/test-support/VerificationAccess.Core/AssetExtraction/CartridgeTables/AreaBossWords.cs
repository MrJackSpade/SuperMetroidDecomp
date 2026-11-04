using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// The boss identities an enemy header's +$10 word holds (#1472), decoded once here; zero names no boss.
/// </summary>
internal static class AreaBossWords
{
    /// <summary>The word of each area boss.</summary>
    internal static readonly NativeWordIdentities<AreaBoss> Bosses = new(new Dictionary<AreaBoss, ushort>
    {
        [AreaBoss.CeresRidley] = 1,
        [AreaBoss.Torizo] = 2,
        [AreaBoss.Kraid] = 3,
        [AreaBoss.SporeSpawn] = 4,
        [AreaBoss.Ridley] = 5,
        [AreaBoss.Crocomire] = 6,
        [AreaBoss.Phantoon] = 7,
        [AreaBoss.Draygon] = 8,
        [AreaBoss.Botwoon] = 9,
        [AreaBoss.MotherBrain] = 10,
    });

    /// <summary>The boss a header word names, or null for zero.</summary>
    /// <exception cref="InvalidDataException">The word names no boss.</exception>
    internal static AreaBoss? Decode(int word) => word == 0 ? null : Bosses.Decode(word, "area boss");
}
