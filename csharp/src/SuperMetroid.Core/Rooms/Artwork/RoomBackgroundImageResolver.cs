using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Resolves what a room's background image shows (#1392, #1466): the image, and for each 16x16 cell whose appearance
/// inputs no longer all show the room's load, that cell's picture in the block sheet for their values now.
/// </summary>
/// <param name="image">The background's image.</param>
/// <param name="sheet">The room tileset's block sheet, which holds the cells' pictures.</param>
public sealed class RoomBackgroundImageResolver(RoomBackgroundImage image, RoomBlockSheet sheet)
{
    /// <summary>The value each input of each changing cell took when last resolved, by changing cell.</summary>
    private readonly string[][] values = [.. image.Cells.Select(cell => new string[cell.Inputs.Length])];

    /// <summary>The cell pictures last resolved, null where the image shows.</summary>
    private SpriteSheetFrame?[] cells = new SpriteSheetFrame?[image.CellCount];

    /// <summary>Whether a resolution has been made.</summary>
    private bool resolved;

    /// <summary>The background's image.</summary>
    public RoomBackgroundImage Image { get; } = Ensure.NotNull(image);

    /// <summary>The background as BG2 shows it while the room's appearance inputs take the values <paramref name="appearance"/> reads.</summary>
    /// <exception cref="InvalidDataException">A cell whose inputs changed has no picture for their values.</exception>
    public RoomBackgroundImagePlane Resolve(RoomAppearanceView appearance)
    {
        SpriteSheetFrame?[]? next = resolved ? null : new SpriteSheetFrame?[Image.CellCount];
        for (int index = 0; index < Image.Cells.Count; index++)
        {
            RoomBackgroundCell cell = Image.Cells[index];
            string[] shown = values[index];
            bool changed = !resolved, rebound = false;
            for (int input = 0; input < cell.Inputs.Length; input++)
            {
                string now = appearance.ValueOf(cell.Inputs[input]);
                rebound |= now != RoomAppearanceValues.Loaded;
                changed |= now != shown[input];
                shown[input] = now;
            }
            if (!changed)
                continue;
            next ??= [.. cells];
            next[cell.Cell] = rebound ? sheet.Picture(AppearancePictureNames.Of(cell.Picture, cell.Inputs, shown)) : null;
        }
        resolved = true;
        if (next is not null)
            cells = next;
        return new RoomBackgroundImagePlane(Image, cells);
    }
}
