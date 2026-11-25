using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.World;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The HUD's shades for a door's fades (#1512, #2289), which the import pre-colours a tileset's layer-3 pictures in: the HUD
/// with every colour the door does not keep lit black, which a door's fade runs a typed fade toward and back from. A door
/// keeps the minimap's explored and unexplored colours, the highlighted item's and the minimap's room highlight, and between
/// two common rooms the HUD's item and minimap backgrounds too, as <c>GameState_A_LoadingNextRoom</c> copies them into its
/// fade's target.
/// </summary>
internal static partial class DoorFadeHudShades
{
    /// <summary>The HUD's inks: its two colour sets of sixteen, as a layer-3 picture inks them.</summary>
    private const int HudInks = 32;

    /// <summary>The inks every door keeps lit: the minimap's explored and unexplored colours, the highlighted item's and the room highlight.</summary>
    internal static readonly ImmutableArray<int> Kept = [9, 10, 13, 14, 17, 18, 19, 29];

    /// <summary>The inks a door between two common rooms also keeps lit: the HUD's item backgrounds and the minimap's.</summary>
    internal static readonly ImmutableArray<int> KeptBetweenCommonRooms = [20, 21, 22, 23, 28];

    /// <summary>The HUD's two dark shades: every door's, and a door's between two common rooms.</summary>
    internal static IReadOnlyList<SheetShade> Of() =>
    [
        Dark(DoorFadeShades.HudDark, Faded(Kept)),
        Dark(DoorFadeShades.HudDarkBetweenCommonRooms, Faded([.. Kept, .. KeptBetweenCommonRooms])),
    ];

    /// <summary>The HUD inks a door fades: all but <paramref name="kept"/>, in rising order.</summary>
    private static int[] Faded(ImmutableArray<int> kept) => [.. Enumerable.Range(0, HudInks).Except(kept)];

    /// <summary><paramref name="inks"/> black, as a door's fade out leaves them.</summary>
    private static SheetShade Dark(PictureShade shade, int[] inks) => SheetShade.Of(shade, inks, _ => RgbColor.Black);
}
