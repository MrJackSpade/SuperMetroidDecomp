using SuperMetroid.Core.Assets;

/// <summary>Cartridge values of <see cref="ChargeFlareSpriteDefinitions"/> that only verification reads.</summary>
internal static class ChargeFlareSpriteDefinitionsConstants
{
    /// <summary>$93:A1A1, FlareSpritemapPointers: the first 54 selectors are charge-flare components.</summary>
    public const int SelectorTable = 0x93A1A1;

    /// <summary>Provides the native selector table for checking charge flare sprite choices.</summary>
    extension(ChargeFlareSpriteDefinitions)
    {
        /// <inheritdoc cref="ChargeFlareSpriteDefinitionsConstants.SelectorTable"/>
        internal static int SelectorTable => ChargeFlareSpriteDefinitionsConstants.SelectorTable;
    }
}
