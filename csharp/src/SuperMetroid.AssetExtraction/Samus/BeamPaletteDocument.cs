using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable JSON schema for ordinary beam-combination colors; palette selection and charge/Hyper Beam animation are engine-owned rather than document data.</summary>
public sealed record BeamPaletteDocument
{
    /// <summary>Schema revision, which must equal <see cref="BeamPaletteDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>All twelve selections keyed by <see cref="BeamPaletteSources.Key"/>, each with sixteen RGB5 colors in OBJ palette order, including color zero; combinations may independently edit colors even when their stock native pointers alias.</summary>
    public required Dictionary<string, PaletteRgb5[]> Palettes { get; init; }
}
