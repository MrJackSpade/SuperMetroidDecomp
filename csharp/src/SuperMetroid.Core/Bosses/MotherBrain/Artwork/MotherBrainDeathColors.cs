using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Reads Mother Brain's installed death colours: the exploded door's colours and the identity of all four images.</summary>
internal static class MotherBrainDeathColors
{
    /// <summary>Returns a colour of the exploded escape door, from its edits or the reviewed paint.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The color index is outside 0..13.</exception>
    internal static RgbColor ExplodedDoorColor(RgbColor[]? explodedDoor, int color) =>
        (uint)color < MotherBrainExplodedDoorPaintDefinitions.ColorCount
            ? explodedDoor is null ? MotherBrainExplodedDoorPaint.Color(color) : explodedDoor[color] :
            throw new ArgumentOutOfRangeException(nameof(color));
}
