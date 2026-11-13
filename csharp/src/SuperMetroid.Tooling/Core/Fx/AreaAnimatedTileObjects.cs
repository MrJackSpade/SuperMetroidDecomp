using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Fx.AreaAnimatedTileObjectDefinitions;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Selects the animated-tile object an area's FX bit enables (<see cref="AreaAnimatedTileObjectDefinitions"/>): the shared spikes
/// for the first two bits, then the area's named effects or the empty object. The import's decode knowledge: the record
/// codec resolves each record's bits through it, and the decoded record names the objects (#1526).
/// </summary>
internal static class AreaAnimatedTileObjects
{
    /// <summary>Direct area/bit behavior selection, shared by retail and audit views.</summary>
    /// <remarks>All eight areas' selections share spike objects for
    /// bits0/1. Later bits select named area effects or the empty object. This dispatch
    /// is independently verified against NTSC J/U v1.0 and pinned bank_83.asm
    /// (362be646929cf8e483f692b73a6561cfc2dc1d0d); no selection matrix remains.
    /// Callers retain their distinct area bounds and validation order. Area index 7,
    /// beyond the seven retail areas, selects only the shared spikes.</remarks>
    internal static BlockAnimation SelectObject(int areaIndex, int bit) => bit switch
    {
        0 => BlockAnimation.HorizontalSpikes,
        1 => BlockAnimation.VerticalSpikes,
        _ => areaIndex < SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count
            ? SelectAreaObject(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Decode(areaIndex, "Animated-tile area"), bit)
            : BlockAnimation.Empty,
    };

    /// <summary>Maps the area-specific FX bits after the shared horizontal and vertical spike selectors.</summary>
    private static BlockAnimation SelectAreaObject(AreaId area, int bit) => new AreaFxBit(area, bit) switch
    {
        (AreaId.Crateria, 2) => BlockAnimation.CrateriaLake,
        (AreaId.Crateria, 3) => BlockAnimation.UnusedCrateriaLava,
        (AreaId.Brinstar, 2) => BlockAnimation.BrinstarPlant,
        (AreaId.WreckedShip, 2) => BlockAnimation.WreckedShipTreadmillRightwards,
        (AreaId.WreckedShip, 3) => BlockAnimation.WreckedShipTreadmillLeftwards,
        (AreaId.WreckedShip, 4) => BlockAnimation.WreckedShipScreen,
        (AreaId.Maridia, 2) => BlockAnimation.MaridiaSandCeiling,
        (AreaId.Maridia, 3) => BlockAnimation.MaridiaSandFalling,
        _ => BlockAnimation.Empty,
    };
    /// <summary>Returns the object selected by one retail area/bit pair.</summary>
    public static BlockAnimation Read(AreaId area, int bit)
    {
        if ((uint)bit >= ObjectsPerArea)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bit), bit, $"Animated-tile bit must be 0..{ObjectsPerArea - 1}.");
        }

        return SelectObject(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(area), bit);
    }
}
