using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Fx.AreaAnimatedTileObjectDefinitions;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Selects the animated-tile object an area's FX bit enables (<see cref="AreaAnimatedTileObjectDefinitions"/>): the shared spikes
/// for the first two bits, then the area's named effects or the empty object.
/// </summary>
internal static class AreaAnimatedTileObjects
{
    /// <summary>Direct area/bit behavior selection, shared by retail and native audit views.</summary>
    /// <remarks>All eight original lists at $83:AC76..AD65 share spike objects for
    /// bits0/1. Later bits select named area effects or the empty object. This dispatch
    /// is independently verified against NTSC J/U v1.0 and pinned bank_83.asm
    /// (362be646929cf8e483f692b73a6561cfc2dc1d0d); no selection matrix remains.
    /// Callers retain their distinct area bounds and validation order. Native area index 7,
    /// beyond the seven retail areas, selects only the shared spikes.</remarks>
    internal static AnimatedTileObject SelectObject(int areaIndex, int bit) => bit switch
    {
        0 => AnimatedTileObject.HorizontalSpikes,
        1 => AnimatedTileObject.VerticalSpikes,
        _ => areaIndex < AreaIds.RetailCount
            ? SelectAreaObject(AreaIds.FromIndex(WordWrap.LowByte(areaIndex), "Animated-tile area"), bit)
            : AnimatedTileObject.Empty,
    };

    /// <summary>Maps the area-specific FX bits after the shared horizontal and vertical spike selectors.</summary>
    private static AnimatedTileObject SelectAreaObject(AreaId area, int bit) => new AreaFxBit(area, bit) switch
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
