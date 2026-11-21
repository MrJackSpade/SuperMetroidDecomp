namespace SuperMetroid.Core.Figures;

/// <summary>How a figure is presented: in play until a cinematic shows it.</summary>
public sealed class FigurePresentation
{
    /// <summary>The mode, which starts at the default every new presentation holds.</summary>
    public FigurePresentationMode Mode { get; private set; }

    /// <summary>Presents the figure as a cinematic's from now on.</summary>
    public void ShowInCinematic() => Mode = FigurePresentationMode.Cinematic;
}
