using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The import's record of where among the cartridge's colours it coloured an installed sheet's colours from (#1512,
/// #1544): the half of the colour table the sheet was composed against, the palette bits a sprite sheet's draws OR into
/// each part's own row, the gameplay background set each of a gameplay background sheet's groups was installed from, and
/// the place in that half each of the image's own colours came from. The import's content identity and its checks against
/// the cartridge read it; the game draws the image from its own colours alone.
/// </summary>
public sealed record InstalledSheetSource
{
    /// <summary>The half of the colour table the sheet was composed against.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<SpriteImageColours>))]
    public required SpriteImageColours Palette { get; init; }

    /// <summary>The palette bits a sprite sheet's draws OR into each part's own row; zero for every other sheet.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int PaletteBits { get; init; }

    /// <summary>
    /// The gameplay background set each of a gameplay background sheet's colour groups was installed from, group after group
    /// (#1464); absent for an actor's sheet and a frontend screen's.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InstalledBackgroundSet[]? BackgroundSets { get; init; }

    /// <summary>
    /// For each of the image's own colours, in their order, its composed palette index: its colour group's place in the
    /// half times sixteen plus its colour.
    /// </summary>
    public required int[] Inks { get; init; }
}
