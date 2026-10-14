using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="RoomFxRomData.Tables"/> that only verification reads.</summary>
internal static class RoomFxRomDataTablesConstants
{
    /// <summary>Bank-$83 pointer table at $AC46 for each area's palette-FX object list.</summary>
    public const int AreaPaletteFxObjectListPointers = 0x83ac46;

    /// <summary>Provides the area palette effect object-list pointer table for room FX checks.</summary>
    extension(RoomFxRomData.Tables)
    {
        /// <inheritdoc cref="RoomFxRomDataTablesConstants.AreaPaletteFxObjectListPointers"/>
        internal static int AreaPaletteFxObjectListPointers => RoomFxRomDataTablesConstants.AreaPaletteFxObjectListPointers;
    }
}
