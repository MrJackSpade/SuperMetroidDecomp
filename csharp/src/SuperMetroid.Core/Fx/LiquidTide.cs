namespace SuperMetroid.Core.Fx;

/// <summary>How a room's liquid surface rises and falls about its base line (#1695).</summary>
/// <remarks>The values are arbitrary. <see cref="LiquidTideDefinitions"/> holds each tide's reach and pace.</remarks>
public enum LiquidTide
{
    /// <summary>The surface stays on its base line.</summary>
    None,

    /// <summary>The surface rises and falls by up to <see cref="LiquidTideDefinitions.SmallTidePeakPixels"/>.</summary>
    Small,

    /// <summary>The surface rises and falls by up to <see cref="LiquidTideDefinitions.LargeTidePeakPixels"/>.</summary>
    Large,
}
