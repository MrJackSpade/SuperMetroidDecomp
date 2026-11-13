using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>
/// The installed colours of Mother Brain's rainbow beam backdrop (#1637), as a player edits them in place: the colour the
/// backdrop shows as the beam starts, and the colours it cycles through while the beam fires. Her rainbow, drain, revival
/// and restoration colours are import data, which only pre-colours her pictures.
/// </summary>
public sealed record MotherBrainBeamColourDocument
{
    /// <summary>Schema revision; loading requires <see cref="MotherBrainBeamColourFile.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>The colour the backdrop shows on the beam's first frame, before its cycle starts.</summary>
    public required PaletteRgb5 First { get; init; }

    /// <summary>The colours the backdrop cycles through while the beam fires, in order; after the last, the cycle starts again.</summary>
    public required PaletteRgb5[] Cycle { get; init; }
}
