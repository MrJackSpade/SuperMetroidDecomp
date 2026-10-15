namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>Cartridge values of <see cref="SporeSpawnDeathColorDefinitions"/> that only verification reads.</summary>
internal static class SporeSpawnDeathColorDefinitionsConstants
{
    /// <summary>BG palette7 occupies decoded bytes$E0..FF in Palettes_6_GreenBlueBrinstar.</summary>
    public const int BackgroundPaletteByteOffset = 7 * 16 * sizeof(ushort);
    /// <summary>BG palette4 occupies decoded bytes$80..9F in Palettes_6_GreenBlueBrinstar.</summary>
    public const int LevelPaletteByteOffset = 4 * 16 * sizeof(ushort);
    /// <summary>$C2:B264, Palettes_6_GreenBlueBrinstar; selected by both Spore Spawn room states at $8F:9DD9/$9DF3.</summary>
    public const int OriginalRoomPaletteSource = 0xc2b264;

    /// <summary>Provides Spore Spawn death palette offsets and original room palette source for color checks.</summary>
    extension(SporeSpawnDeathColorDefinitions)
    {
        /// <inheritdoc cref="SporeSpawnDeathColorDefinitionsConstants.BackgroundPaletteByteOffset"/>
        internal static int BackgroundPaletteByteOffset => SporeSpawnDeathColorDefinitionsConstants.BackgroundPaletteByteOffset;
        /// <inheritdoc cref="SporeSpawnDeathColorDefinitionsConstants.LevelPaletteByteOffset"/>
        internal static int LevelPaletteByteOffset => SporeSpawnDeathColorDefinitionsConstants.LevelPaletteByteOffset;
        /// <inheritdoc cref="SporeSpawnDeathColorDefinitionsConstants.OriginalRoomPaletteSource"/>
        internal static int OriginalRoomPaletteSource => SporeSpawnDeathColorDefinitionsConstants.OriginalRoomPaletteSource;
    }
}
