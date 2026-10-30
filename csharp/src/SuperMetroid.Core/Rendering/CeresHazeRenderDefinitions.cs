using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rendering;

/// <summary>How the Ceres haze's colour bands are laid out down the screen.</summary>
internal static class CeresHazeRenderDefinitions
{
    /// <summary>Screen line 64 starts the brightening colour bands, after the flat, uncoloured region at the top.</summary>
    internal const int RampFirstLine = 64;
    /// <summary>Each colour band spans eight screen lines, one step stronger than the band above.</summary>
    internal const int BandHeight = 8;
    /// <summary>The strongest step the haze reaches, at the end of its fade: the steps from no colour to the whole tint.</summary>
    internal const int MaximumComponent = CeresHazeDefinitions.FadeSteps - 1;
}
