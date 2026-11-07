using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The shades of each room-FX palette blend (#1512), which the import pre-colours a tileset's layer-3 pictures in: a
/// blend loads its three colours over the HUD's effect colours' tenth through twelfth, and the empty blend clears only
/// the twelfth, as <c>LoadFXHeader</c> copies them.
/// </summary>
internal static class RoomFxBlendSheetShades
{
    /// <summary>The first blend ink of a tileset's layer-3 picture.</summary>
    private const int FirstInk = 25;

    /// <summary>The ink the empty blend clears: the third blend ink.</summary>
    private const int ClearedInk = FirstInk + 2;

    /// <summary>Every blend's shade, with the colours <paramref name="blends"/> installs for it.</summary>
    internal static IReadOnlyList<SheetShade> Of(RoomFxBlendInks blends)
    {
        Ensure.NotNull(blends);
        return [.. Enum.GetValues<RoomFxPaletteBlend>().Select(blend => SheetShade.Run(RoomFxBlendShading.Of(blend),
            blend == RoomFxPaletteBlend.None ? ClearedInk : FirstInk, [.. blends.Inks(blend)]))];
    }
}
