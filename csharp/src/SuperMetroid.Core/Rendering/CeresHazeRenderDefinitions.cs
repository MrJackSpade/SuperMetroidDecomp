using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rendering;

/// <summary>How the Ceres haze's colour bands are laid out down the screen.</summary>
internal static class CeresHazeRenderDefinitions
{
    /// <summary>Screen line 64 starts the brightening colour bands, after the flat region at the top.</summary>
    internal const int RampFirstLine = 64;
    /// <summary>The first 64 lines take the haze's first step: no colour, when fully faded in.</summary>
    internal const int InitialComponent = 0;
    /// <summary>The next eight lines take the second step: one step of colour, when fully faded in.</summary>
    internal const int RampFirstComponent = 1;
    /// <summary>Each later colour band spans eight screen lines.</summary>
    internal const int BandHeight = 8;
    /// <summary>The strongest step the haze reaches, at the end of its fade.</summary>
    internal const int MaximumComponent = CeresHazeDefinitions.FadeSteps - 1;
}
