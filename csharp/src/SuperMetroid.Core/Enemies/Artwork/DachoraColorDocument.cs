using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Editable RGB5 palette payload for ordinary Dachora enemy $E5FF; frame timing, speed thresholds, and shine-state transitions are not schema fields.</summary>
public sealed record DachoraColorDocument
{
    /// <summary>Schema revision required to equal <see cref="DachoraColorFormat.Version"/> during loading.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen default RGB5 colors, including transparent slot zero, corresponding to <c>Palette_Dachora</c> at $A7:F225.</summary>
    public required PaletteRgb5[] Normal { get; init; }
    /// <summary>Four ordered speed-boost frames of sixteen RGB5 colors each, corresponding to $A7:F245-$F2C4; native enemy behavior selects their cadence.</summary>
    public required PaletteRgb5[][] Speed { get; init; }
    /// <summary>Four ordered stored-shine frames of sixteen RGB5 colors each, corresponding to $A7:F2C5-$F344; the separate native shine loop selects the frame.</summary>
    public required PaletteRgb5[][] Shine { get; init; }
}
