namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable bank-$83 area-to-animated-tile-object selection metadata consumed by
/// <c>LoadFxHeader</c>.
/// </summary>
/// <remarks>
/// Each area owns eight bank-$87 object-header pointers, selected by the eight bits in
/// an FX record's animated-tile bitset. These pointers choose object behavior; the
/// selected objects' character-frame operands and graphics remain presentation data.
/// </remarks>
internal static class AreaAnimatedTileObjectDefinitions
{

    /// <summary>Eight object-header words are addressable by one FX bitset.</summary>
    public const int ObjectsPerArea = 8;

    /// <summary>Direct area/bit behavior selection, shared by retail and native audit views.</summary>
    /// <remarks>All eight original lists at $83:AC76..AD65 share spike objects for
    /// bits0/1. Later bits select named area effects or the empty object. This dispatch
    /// is independently verified against NTSC J/U v1.0 and pinned bank_83.asm
    /// (362be646929cf8e483f692b73a6561cfc2dc1d0d); no selection matrix remains.
    /// Callers retain their distinct area bounds and validation order. Native area index 7,
    /// beyond the seven retail areas, selects only the shared spikes.</remarks>
    private static AnimatedTileObject SelectObject(int nativeArea, int bit) => bit switch
    {
        0 => AnimatedTileObject.HorizontalSpikes,
        1 => AnimatedTileObject.VerticalSpikes,
        _ => nativeArea < AreaIds.RetailCount
            ? SelectAreaObject(AreaIds.FromCartridge((byte)nativeArea, "Animated-tile native area"), bit)
            : AnimatedTileObject.Empty,
    };

    private static AnimatedTileObject SelectAreaObject(AreaId area, int bit) => (area, bit) switch
    {
        (AreaId.Crateria, 2) => AnimatedTileObject.CrateriaLake,
        (AreaId.Crateria, 3) => AnimatedTileObject.UnusedCrateriaLava,
        (AreaId.Brinstar, 2) => AnimatedTileObject.BrinstarPlant,
        (AreaId.WreckedShip, 2) => AnimatedTileObject.WreckedShipTreadmillRightwards,
        (AreaId.WreckedShip, 3) => AnimatedTileObject.WreckedShipTreadmillLeftwards,
        (AreaId.WreckedShip, 4) => AnimatedTileObject.WreckedShipScreen,
        (AreaId.Maridia, 2) => AnimatedTileObject.MaridiaSandCeiling,
        (AreaId.Maridia, 3) => AnimatedTileObject.MaridiaSandFalling,
        _ => AnimatedTileObject.Empty,
    };
    /// <summary>Returns the bank-$87 object header selected by one retail area/bit pair.</summary>
    public static AnimatedTileObject Read(AreaId area, int bit)
    {
        if ((uint)bit >= ObjectsPerArea)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bit), bit, $"Animated-tile bit must be 0..{ObjectsPerArea - 1}.");
        }

        return SelectObject(AreaIds.ToIndex(area), bit);
    }
}
