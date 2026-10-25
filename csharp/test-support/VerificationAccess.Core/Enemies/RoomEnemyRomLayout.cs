namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The banks verification's retail ROM oracle reads enemy headers, graphics sets and populations from (#1412).
/// Production code knows none of them: the import reads them through its own source catalogs.
/// </summary>
internal static class RoomEnemyRomLayout
{
    /// <summary>Bank containing 64-byte enemy definition records.</summary>
    public const int DefinitionBank = 0xa00000;

    /// <summary>Bank containing enemy graphics-set records.</summary>
    public const int TilesetBank = 0xb40000;

    /// <summary>Bank containing room enemy population records.</summary>
    public const int PopulationBank = 0xa10000;
}
