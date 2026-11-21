using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>The editable JSON schema for color samples used by room and cinematic palette-FX programs.</summary>
/// <remarks>Each program's frames are ordered by native timed record, then by that record's emitted colors. RGB channels range from zero through thirty-one; record durations, skipped palette entries, destinations, and control side effects are compiled mechanics.</remarks>
public sealed record RoomPaletteFxPresentationDocument
{
    /// <summary>Gets the schema version; the reader accepts only <see cref="SuperMetroid.AssetExtraction.Fx.Artwork.RoomPaletteFxPresentationFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Each colour program's frames, under the program's key (<see cref="RoomPaletteFxPrograms"/>) (#2557).</summary>
    public required Dictionary<string, PaletteRgb5[][]> Programs { get; init; }
}
