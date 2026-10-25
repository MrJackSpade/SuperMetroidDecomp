namespace SuperMetroid.Core.Fx;

/// <summary>How far and how fast a tidal liquid surface rises and falls (#1415).</summary>
public static class LiquidTideDefinitions
{
    /// <summary>Small-tide sine scale before the byte-shifted fixed add.</summary>
    public const int SmallTideScale = 8;

    /// <summary>Large-tide sine scale before the byte-shifted fixed add.</summary>
    public const int LargeTideScale = 32;

    /// <summary>Small-tide phase delta for a nonnegative sine sample.</summary>
    public const ushort SmallTidePositivePhaseDelta = 288;

    /// <summary>Small-tide phase delta for a negative sine sample.</summary>
    public const ushort SmallTideNegativePhaseDelta = 192;

    /// <summary>Large-tide phase delta for a nonnegative sine sample.</summary>
    public const ushort LargeTidePositivePhaseDelta = 224;

    /// <summary>Large-tide phase delta for a negative sine sample.</summary>
    public const ushort LargeTideNegativePhaseDelta = 128;
}
