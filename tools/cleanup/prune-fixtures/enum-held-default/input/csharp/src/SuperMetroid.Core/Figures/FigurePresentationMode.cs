namespace SuperMetroid.Core.Figures;

/// <summary>How a figure is presented: in play, or as a cinematic shows it.</summary>
public enum FigurePresentationMode
{
    /// <summary>In play: every presentation holds it from the start, and no code names it.</summary>
    Gameplay,

    /// <summary>A mode nothing uses or holds, which the prune removes; the mode after it keeps its value.</summary>
    Unused,

    /// <summary>A figure a cinematic shows.</summary>
    Cinematic,
}
