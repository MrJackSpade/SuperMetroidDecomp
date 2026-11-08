using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The import's record of where one holder of a picture composed its colours from, where that differs from the record of
/// the sheet the picture is held on (#1544). Spawned-object art is held once by what it shows
/// (<see cref="SheetPictureLook"/>), so a picture several graphics sets draw alike is installed once, painted and sourced as
/// the first set holding it composed it; another set may have composed the same colours from other rows or palette bits,
/// as its colours lie elsewhere in its colour table. The loader keeps it beside that set's frame of the picture
/// (<see cref="InstalledPictureColours.FrameSourceOf"/>); the import's content identity and its checks against the cartridge
/// read it, the checks to find each colour of that set's draws where that set's colours lie. The game draws the picture
/// from its own colours alone.
/// </summary>
internal sealed record InstalledFrameSource
{
    /// <summary>The palette bits the holder's draws OR into each part's own row.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int PaletteBits { get; init; }

    /// <summary>
    /// The composed palette index, as the record of the sheet holding the picture names it, of each colour the picture shows,
    /// in the order the picture first shows them.
    /// </summary>
    public required int[] SheetInks { get; init; }

    /// <summary>The composed palette index the holder composed each of <see cref="SheetInks"/>' colours from, place for place.</summary>
    public required int[] Inks { get; init; }
}
