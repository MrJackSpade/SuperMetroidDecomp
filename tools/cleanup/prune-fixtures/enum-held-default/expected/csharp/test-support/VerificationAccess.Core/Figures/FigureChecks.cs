using SuperMetroid.Core.Figures;

namespace SuperMetroid.Verification.Figures;

/// <summary>The presentations the checks compare.</summary>
internal static class FigureChecks
{
    /// <summary>Whether a new figure is presented in play rather than by a cinematic.</summary>
    internal static bool StartsInPlay => new FigurePresentation().Mode != FigurePresentationMode.Cinematic;
}
