namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values and helpers of <see cref="DeadTorizoArtworkDefinitions"/> that only verification reads.</summary>
internal static class DeadTorizoArtworkDefinitionsConstants
{
    /// <summary>$A9, the bank containing Dead Torizo's private corpse OAM map.</summary>
    internal const byte SpritemapBank = 0xa9;
    /// <summary>$A9:D761, the fixed corpse map drawn by the $A9:D39A hook.</summary>
    internal const ushort HookSpritemap = 0xd761;
    /// <summary>$A9:D6E2, the selected 25-part stationary corpse OAM composition.</summary>
    internal const ushort StationarySpritemap = 0xd6e2;
}
