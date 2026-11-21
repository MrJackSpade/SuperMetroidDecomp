namespace SuperMetroid.Core.Figures;

/// <summary>How a figure is presented: in play, or as a cinematic shows it.</summary>
public enum FigurePresentationMode
{
    /// <summary>In play: every presentation holds it from the start, and no code names it.</summary>
    Gameplay,

    /// <summary>A figure a cinematic shows.</summary>
    Cinematic = 2,
}
