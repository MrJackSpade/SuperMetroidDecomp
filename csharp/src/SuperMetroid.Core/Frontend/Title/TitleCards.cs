namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Selects the typed steps of the title card a title phase shows.</summary>
internal static class TitleCards
{
    /// <summary>The steps of the card <paramref name="phase"/> shows.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> shows no title card.</exception>
    internal static IReadOnlyList<TitleCardStep> Steps(TitleSequencePhase phase) => phase switch
    {
        TitleSequencePhase.YearText => TitleCardDefinitions.Year,
        TitleSequencePhase.NintendoText => TitleCardDefinitions.Nintendo,
        TitleSequencePhase.PresentsText => TitleCardDefinitions.Presents,
        TitleSequencePhase.MetroidThreeText => TitleCardDefinitions.MetroidThree,
        TitleSequencePhase.SceneZeroPan or
            TitleSequencePhase.SceneOnePan or
            TitleSequencePhase.SceneTwoPan or
            TitleSequencePhase.SceneThreeZoom or
            TitleSequencePhase.TitleLogoFade or
            TitleSequencePhase.CopyrightFade or
            TitleSequencePhase.SkipFadeOut or
            TitleSequencePhase.TitleScreenFadeIn or
            TitleSequencePhase.TitleScreen or
            TitleSequencePhase.TitleScreenFadeOut => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Phase shows no title card."),
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Phase shows no title card."),
    };
}
