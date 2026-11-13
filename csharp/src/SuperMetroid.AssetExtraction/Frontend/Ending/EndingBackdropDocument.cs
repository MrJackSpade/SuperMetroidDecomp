using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>The installed ending scene backdrops (#1544): the solid colour each scene shows behind every layer.</summary>
public sealed record EndingBackdropDocument
{
    /// <summary>Schema revision; loading requires <see cref="EndingBackdropFiles.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>The cartridge the backdrops were imported from.</summary>
    public required string SourceCartridgeSha256 { get; init; }

    /// <summary>The escape scenes' backdrop.</summary>
    public required PaletteRgb5 Escape { get; init; }

    /// <summary>The planet explosion's and the flyaway's backdrop.</summary>
    public required PaletteRgb5 Explosion { get; init; }

    /// <summary>The staff credits' backdrop.</summary>
    public required PaletteRgb5 Credits { get; init; }

    /// <summary>The post-credits scenes' backdrop.</summary>
    public required PaletteRgb5 PostCredits { get; init; }

    /// <summary>
    /// The backdrop the space behind the exploding planet shows at each frame of its colour effect, first frame first
    /// (#1583, #1637).
    /// </summary>
    public required PaletteRgb5[] SpaceBehindExplodingPlanet { get; init; }
}
