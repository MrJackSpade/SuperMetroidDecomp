using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.World;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>
/// The escape timer's shade for a door's fades (#1512, #2289), which the import pre-colours the timer's sheet in: the timer
/// with every colour of its row the door does not keep lit black, which a door's fade runs a typed fade toward and back
/// from. A door keeps the timer's colours one, two, four and thirteen, as <c>GameState_A_LoadingNextRoom</c>'s third copy
/// into its fade's target keeps them.
/// </summary>
internal static partial class EscapeTimerDoorFadeShades
{
    /// <summary>Colours in the timer's sprite row.</summary>
    private const int RowColours = 16;

    /// <summary>The timer's colours a door keeps lit.</summary>
    internal static readonly ImmutableArray<int> Kept = [1, 2, 4, 13];

    /// <summary>The door fade's dark shade of the timer drawn in sprite row <paramref name="row"/>.</summary>
    /// <param name="row">The sprite row the timer is drawn in.</param>
    internal static IReadOnlyList<SheetShade> Of(int row)
    {
        IEnumerable<int> faded = Enumerable.Range(0, RowColours).Except(Kept).Select(colour => row * RowColours + colour);
        return [SheetShade.Of(DoorFadeShades.EscapeTimerDark, faded, _ => RgbColor.Black)];
    }
}
