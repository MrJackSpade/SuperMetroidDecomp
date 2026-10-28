namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Title tube shading and display paint bindings at $8D:C800-C873.</summary>
/// <remarks>The initial palette remains the independent paint owner. The selected three-unit
/// shade scale, rounded channel interpolation and dim-paint roles describe this animation;
/// no isolated green-channel exception or duplicate stock endpoint is stored.</remarks>
internal static class TitleAmbientColorDefinitions
{
    /// <summary>$8D:C800-C85B: nominal three-unit RGB5 dimming step, with shorter channels distributed to black.</summary>
    internal const int ShadeStep = 3;
    /// <summary>$8D:C870: dim warm display paint reuses title palette slot3 ($8C:E1EF).</summary>
    internal const int DimWarmPaint = 3;
    /// <summary>$8D:C872: dim green display paint reuses title palette slot19 ($8C:E20F).</summary>
    internal const int DimGreenPaint = 19;
}
