namespace SuperMetroid.Core.Assets;

/// <summary>Defines all six editable opening-narration pages.</summary>
public sealed record IntroNarrationDocument
{
    /// <summary>Narration document schema revision checked before page compilation.</summary>
    public required int Version { get; init; }
    /// <summary>Six pages keyed by <see cref="IntroNarrationPageId"/> name.</summary>
    public required Dictionary<string, IntroNarrationPage> Pages { get; init; }
}
