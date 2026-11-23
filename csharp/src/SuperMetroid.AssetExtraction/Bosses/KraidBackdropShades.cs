using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades of Kraid's room backdrop (#1512), which the import pre-colours his room's pictures in: his backdrop colours
/// over the room colour group his growth recolours, and black. His fades between them, and from the room's own colours, are
/// typed fades the encounter publishes over these shadings (#2215); the import pre-colours none of their colours partway.
/// </summary>
internal static partial class KraidBackdropShades
{
    /// <summary>The first ink of a room picture the backdrop recolours: the fifth tile colour group's first.</summary>
    private const int FirstInk = 96;

    /// <summary>The backdrop's shades of a room, to Kraid's <paramref name="kraid"/> colours and to black.</summary>
    internal static IReadOnlyList<SheetShade> Of(KraidColorCatalog kraid)
    {
        Ensure.NotNull(kraid);
        RgbColor[] backdrop = [.. Enumerable.Range(0, KraidPalettes.BandColors).Select(colour => kraid.Resolve(KraidPaletteSource.RoomBackdrop, colour))];
        return
        [
            SheetShade.Run(KraidShades.Backdrop, FirstInk, backdrop),
            SheetShade.Run(KraidShades.BackdropBlack, FirstInk, new RgbColor[KraidPalettes.BandColors]),
        ];
    }
}
