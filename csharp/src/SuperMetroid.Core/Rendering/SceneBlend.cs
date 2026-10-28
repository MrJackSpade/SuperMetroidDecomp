using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// How one gameplay composition blends (#1407): which layers blend and how, whether they blend with the layers behind
/// them or with a single colour, and that colour.
/// </summary>
public readonly record struct SceneBlend
{
    /// <summary>The layers that blend, and whether the blend subtracts and halves.</summary>
    public required ColorBlend Layers { get; init; }

    /// <summary>Whether the layers blend with the winning layer behind them rather than with <see cref="FixedColor"/>.</summary>
    public required bool BlendsWithLayersBehind { get; init; }

    /// <summary>The colour the layers blend with when not with the layers behind, and wherever nothing shows behind them.</summary>
    public required RgbColor FixedColor { get; init; }
}
