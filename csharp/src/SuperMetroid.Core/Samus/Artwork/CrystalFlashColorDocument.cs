using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Editable JSON payload containing Crystal Flash body and bubble RGB5 frames; native playback timing is not part of this schema.</summary>
public sealed record CrystalFlashColorDocument
{
    /// <summary>Schema revision, required to equal <see cref="CrystalFlashColorFormat.Version"/> during loading.</summary>
    public required int Version { get; init; }
    /// <summary>Ten native body records in playback order, ten colors each.</summary>
    public required PaletteRgb5[][] Body { get; init; }
    /// <summary>Six independent bubble frames in playback order, six colors each.</summary>
    public required PaletteRgb5[][] Bubble { get; init; }
}
