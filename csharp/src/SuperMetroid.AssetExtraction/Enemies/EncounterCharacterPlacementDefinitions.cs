namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Where the cartridge places each encounter's characters among the sprite characters (#1392, #1406).</summary>
internal static class EncounterCharacterPlacementDefinitions
{
    /// <summary>The sprite character the enemy tile area begins at: the first after the standard sprites' area.</summary>
    internal const int EnemyTileAreaFirstCharacter = 192;

    /// <summary>The character the gunship's first liftoff page begins at, $160, from the destination list at $A2:AC11.</summary>
    internal const int GunshipFirstCharacter = 352;

    /// <summary>The character the Ceres door's characters begin at, $100.</summary>
    internal const int CeresDoorFirstCharacter = 256;

    /// <summary>The character the Evir tiles Draygon's opening shows begin at: sixteen past the enemy tile area's start.</summary>
    internal const int EvirFirstCharacter = EnemyTileAreaFirstCharacter + 16;

    /// <summary>The bytes of the Evir tiles Draygon's opening copies ($A5:871B): the Evir's whole graphics-set sheet.</summary>
    internal const int EvirByteCount = 1536;

    /// <summary>The character Crocomire's skeleton's first character region begins at, $160 ($A4:99CB).</summary>
    internal const int SkeletonFirstRegionCharacter = 352;

    /// <summary>The rows of the skeleton's first region ($A4:99CB..99D1); the rest go to its second.</summary>
    internal const int SkeletonFirstRegionRows = 4;

    /// <summary>The character the skeleton's second character region begins at, $1E0 ($A4:99D3).</summary>
    internal const int SkeletonSecondRegionCharacter = 480;

    /// <summary>The characters from one skeleton row to the next.</summary>
    internal const int SkeletonCharactersPerRow = 16;
}
