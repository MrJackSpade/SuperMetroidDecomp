using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.World;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The background's shades for a door's fades (#1512, #2289, #3036), which the import pre-colours every background picture
/// a door shows in: the room's blocks, backgrounds and floor, the pictures written over it, its effect overlay, a boss body
/// and the HUD, each with every background colour the door does not keep lit black, which a door's fade runs a typed fade
/// toward and back from. <c>GameState_A_LoadingNextRoom</c> clears the whole target palette and copies back the minimap's
/// explored and unexplored colours, the highlighted item's and the minimap's room highlight, and unless either room is
/// isolated the HUD's item and minimap backgrounds; those are colours of the background palette, not of the HUD, so every
/// background picture drawn in them, a room's doors among them, stays lit with them while the door fades and scrolls.
/// </summary>
internal static partial class DoorFadeBackgroundShades
{
    /// <summary>The background's inks: its eight colour sets of sixteen, as a background picture inks them.</summary>
    private const int BackgroundInks = 8 * 16;

    /// <summary>The inks every door keeps lit: the minimap's explored and unexplored colours, the highlighted item's and the room highlight.</summary>
    private static readonly ImmutableArray<int> Kept = [9, 10, 13, 14, 17, 18, 19, 29];

    /// <summary>The inks a door between two common rooms also keeps lit: the HUD's item backgrounds and the minimap's.</summary>
    private static readonly ImmutableArray<int> KeptBetweenCommonRooms = [20, 21, 22, 23, 28];

    /// <summary>The background's two dark shades: every door's, and a door's between two common rooms.</summary>
    internal static IReadOnlyList<SheetShade> Of() =>
    [
        Dark(DoorFadeShades.BackgroundDark, Faded(Kept)),
        Dark(DoorFadeShades.BackgroundDarkBetweenCommonRooms, Faded([.. Kept, .. KeptBetweenCommonRooms])),
    ];

    /// <summary>The background inks a door fades: all but <paramref name="kept"/>, in rising order.</summary>
    private static int[] Faded(ImmutableArray<int> kept) => [.. Enumerable.Range(0, BackgroundInks).Except(kept)];

    /// <summary><paramref name="inks"/> black, as a door's fade out leaves them.</summary>
    private static SheetShade Dark(PictureShade shade, int[] inks) => SheetShade.Of(shade, inks, _ => RgbColor.Black);
}
