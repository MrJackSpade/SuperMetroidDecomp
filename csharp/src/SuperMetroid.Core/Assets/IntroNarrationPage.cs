namespace SuperMetroid.Core.Assets;

/// <summary>Defines the ordered lines displayed on one opening-narration page.</summary>
public sealed record IntroNarrationPage
{
    /// <summary>Nonempty lines in ascending tilemap-row order.</summary>
    public required IntroNarrationLine[] Lines { get; init; }
}
