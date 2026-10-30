using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The title's turning floor (#1467): one picture of the whole floor for each Baby Metroid animation page the floor
/// shows, which the import composes from the title's floor map and characters with the page loaded over the Baby's tiles.
/// </summary>
public static class TitleFloors
{
    /// <summary>The name of the floor's picture with Baby Metroid page <paramref name="babyPage"/> shown.</summary>
    public static string FrameName(int babyPage) => $"baby-{babyPage}";

    /// <summary>The floor with Baby Metroid page <paramref name="babyPage"/> shown.</summary>
    /// <exception cref="InvalidDataException">The installed title floor has no picture of the page.</exception>
    internal static AffineFloor Of(InstalledSpriteSheets sheets, int babyPage) =>
        FloorShots.Of(sheets, SpriteSheetId.TitleFloor, FrameName(babyPage));
}
