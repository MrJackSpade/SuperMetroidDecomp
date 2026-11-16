using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Fx.AreaBlockAnimationDefinitions;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Selects the block animation an area's FX bit enables (<see cref="AreaBlockAnimationDefinitions"/>): the shared
/// horizontal spikes for the first bit, then the area's named animations, or none (#1686). The import's decode
/// knowledge: the record codec resolves each record's bits through it, and the decoded record names the animations
/// (#1526).
/// </summary>
/// <remarks>
/// A bit that selects a still object starts nothing (#2054): the cartridge's vertical spikes (every area's second bit),
/// Crateria's lake, Brinstar's plant and the Wrecked Ship's screen have no animation loop and no pictures of their own, so
/// their blocks keep showing their own pictures and the decode drops the bit.
/// </remarks>
internal static class AreaBlockAnimations
{
    /// <summary>Direct area/bit behavior selection, shared by retail and audit views.</summary>
    /// <remarks>All eight areas' selections share the horizontal spikes for bit 0. Later bits select named area
    /// animations, a still object (<c>SelectsStillObject(int, int)</c>) or the empty object. This dispatch
    /// is independently verified against NTSC J/U v1.0 and pinned bank_83.asm
    /// (362be646929cf8e483f692b73a6561cfc2dc1d0d); no selection matrix remains.
    /// Callers retain their distinct area bounds and validation order. Area index 7,
    /// beyond the seven retail areas, selects only the shared spikes. A bit that selects none
    /// names a still object, the cartridge's empty object, or for Crateria's fourth bit a lava animation; no
    /// retail record sets either of the last two, so neither is an object of the game.</remarks>
    internal static BlockAnimation? SelectObject(int areaIndex, int bit) => bit switch
    {
        0 => BlockAnimation.HorizontalSpikes,
        _ => areaIndex < SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count
            ? SelectAreaObject(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Decode(areaIndex, "Block-animation area"), bit)
            : null,
    };

    /// <summary>Maps the area-specific FX bits after the shared horizontal spike selector.</summary>
    private static BlockAnimation? SelectAreaObject(AreaId area, int bit) => new AreaFxBit(area, bit) switch
    {
        (AreaId.WreckedShip, 2) => BlockAnimation.WreckedShipTreadmillRightwards,
        (AreaId.WreckedShip, 3) => BlockAnimation.WreckedShipTreadmillLeftwards,
        (AreaId.Maridia, 2) => BlockAnimation.MaridiaSandCeiling,
        (AreaId.Maridia, 3) => BlockAnimation.MaridiaSandFalling,
        _ => null,
    };

    /// <summary>Whether a retail area's FX bit selects a still object, which starts nothing.</summary>
    internal static bool SelectsStillObject(AreaId area, int bit) => new AreaFxBit(area, bit) switch
    {
        (_, 1) => true,
        (AreaId.Crateria, 2) => true,
        (AreaId.Brinstar, 2) => true,
        (AreaId.WreckedShip, 4) => true,
        _ => false,
    };

    /// <summary>Returns the object selected by one retail area/bit pair, or null for a bit that starts no animation.</summary>
    public static BlockAnimation? Read(AreaId area, int bit)
    {
        if ((uint)bit >= ObjectsPerArea)
        {
            throw new ArgumentOutOfRangeException(
                nameof(bit), bit, $"Block-animation bit must be 0..{ObjectsPerArea - 1}.");
        }

        return SelectObject(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(area), bit);
    }
}
