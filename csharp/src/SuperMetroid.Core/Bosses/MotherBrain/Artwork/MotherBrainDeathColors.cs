using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Reads Mother Brain's installed death colours: the exploded door's colours and the identity of all four images.</summary>
internal static class MotherBrainDeathColors
{
    /// <summary>Hashes the exploded door, then the body, leg and corpse fades.</summary>
    internal static string ContentIdentity(MotherBrainDeathColorCatalog colors) =>
        SelectedPresentationHash.Create("MotherBrainDeathColorCatalog-v1", content =>
        {
            Span<RgbColor> door = stackalloc RgbColor[MotherBrainExplodedDoorPaintDefinitions.ColorCount];
            for (int color = 0; color < door.Length; color++) door[color] = colors.ExplodedDoorColor(color);
            content.AppendColors("explodedDoor", door);
            colors.bodyFade.AppendIdentity(content, "bodyFade");
            colors.legFade.AppendIdentity(content, "legFade");
            colors.corpseFade.AppendIdentity(content, "corpseFade");
        });

    /// <summary>The supplied door colours when they are edits, or null when they match the reviewed paint.</summary>
    internal static RgbColor[]? EditedDoor(RgbColor[] explodedDoor) =>
        MotherBrainExplodedDoorPaint.Matches(explodedDoor) ? null : explodedDoor;

    /// <summary>Returns a colour of the exploded escape door, from its edits or the reviewed paint.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The color index is outside 0..13.</exception>
    internal static RgbColor ExplodedDoorColor(RgbColor[]? explodedDoor, int color) =>
        (uint)color < MotherBrainExplodedDoorPaintDefinitions.ColorCount
            ? explodedDoor is null ? MotherBrainExplodedDoorPaint.Color(color) : explodedDoor[color] :
            throw new ArgumentOutOfRangeException(nameof(color));
}
