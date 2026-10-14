namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>Cartridge layout of the Mama Turtle family headers that only verification reads.</summary>
internal static class MamaTurtleEnemyDefinitionCatalogConstants
{
    /// <summary>First source byte occupied by the two contiguous native headers.</summary>
    public const int SourceAddress = 0xa0cf3f;
    /// <summary>Total byte length of the two native 64-byte headers.</summary>
    public const int SourceByteLength = 128;
}
