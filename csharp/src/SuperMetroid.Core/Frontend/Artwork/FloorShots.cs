using System.Runtime.CompilerServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Frontend.Artwork;

/// <summary>
/// Reads the turning floors the frontend's shots show (#1467): each floor is a whole-floor picture the import composed
/// once, installed as a frame of its scene's floor sheet under the name of the shot that shows it.
/// </summary>
public static class FloorShots
{
    /// <summary>Each installed floor picture's floor, once read, so a shot keeps one floor while its sheet stands.</summary>
    private static readonly ConditionalWeakTable<SpriteSheetFrame, AffineFloor> floors = new();

    /// <summary>The floor picture <paramref name="frame"/> of installed floor sheet <paramref name="sheet"/>.</summary>
    /// <exception cref="InvalidDataException">The sheet holds no such picture, or it is not a whole floor.</exception>
    public static AffineFloor Of(InstalledSpriteSheets sheets, SpriteSheetId sheet, string frame) =>
        floors.GetValue(Ensure.NotNull(sheets).Get(sheet).Frame(frame), picture => new AffineFloor(picture));
}
