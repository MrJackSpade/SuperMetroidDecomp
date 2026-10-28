using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Where the cartridge places each encounter's characters among the sprite characters (#1392, #1406).</summary>
internal static class EncounterCharacterPlacementDefinitions
{
    /// <summary>The character the gunship's first liftoff page begins at, $160, from the destination list at $A2:AC11.</summary>
    internal const int GunshipFirstCharacter = 0x160;

    /// <summary>The character the Ceres door's characters begin at, $100.</summary>
    internal const int CeresDoorFirstCharacter = 0x100;

    /// <summary>The character the Evir tiles Draygon's opening shows begin at: sixteen past the enemy tile area's start.</summary>
    internal const int EvirFirstCharacter = EnemyGraphicsSet.EnemyTileAreaFirstCharacter + 0x10;

    /// <summary>The bytes of the Evir tiles Draygon's opening copies ($A5:871B): the Evir's whole graphics-set sheet.</summary>
    internal const int EvirByteCount = 0x0600;

    /// <summary>The character Crocomire's skeleton's first character region begins at, $160 ($A4:99CB).</summary>
    internal const int SkeletonFirstRegionCharacter = 0x160;

    /// <summary>The rows of the skeleton's first region ($A4:99CB..99D1); the rest go to its second.</summary>
    internal const int SkeletonFirstRegionRows = 4;

    /// <summary>The character the skeleton's second character region begins at, $1E0 ($A4:99D3).</summary>
    internal const int SkeletonSecondRegionCharacter = 0x1e0;

    /// <summary>The characters from one skeleton row to the next.</summary>
    internal const int SkeletonCharactersPerRow = 16;

    /// <summary>The character each Torizo swap's characters begin at, in swap order ($AA, bank-$A0 $814B destinations).</summary>
    internal static readonly TorizoSwapDestination[] TorizoDestinations =
    [
        new(0x130), new(0x140), new(0x1e7), new(0x1f7), new(0x1e5), new(0x1f5), new(0x1c8), new(0x1ca),
        new(0x1d8), new(0x1da), new(0x1e8), new(0x1f8), new(0x19f), new(0x1d8), new(0x1d8), new(0x1d8),
        new(0x1d8), new(0x1d8), new(0x1d8), new(0x1d8), new(0x1d8), new(0x0d0), new(0x1d8), new(0x1d8),
        new(0x1d8), new(0x1d8), new(0x1d8), new(0x1d8), new(0x1d8), new(0x1d8), new(0x130), new(0x140),
        new(0x130), new(0x140), new(0x130), new(0x140), new(0x130), new(0x140), new(0x130), new(0x140),
        new(0x130), new(0x140), new(0x130), new(0x140), new(0x1d8), new(0x1d8), new(0x1d8), new(0x1d8),
        new(0x1d8), new(0x1d8), new(0x1d8), new(0x1d8),
    ];
}
