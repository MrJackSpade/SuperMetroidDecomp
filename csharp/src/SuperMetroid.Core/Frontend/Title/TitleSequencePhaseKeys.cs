namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The installed key of each <see cref="TitleSequencePhase"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class TitleSequencePhaseKeys
{
    /// <summary>Each TitleSequencePhase member's installed key.</summary>
    internal static InstalledKeys<TitleSequencePhase> Installed { get; } = new(new Dictionary<TitleSequencePhase, string>
    {
        [TitleSequencePhase.YearText] = "YearText",
        [TitleSequencePhase.LowerPan] = "LowerPan",
        [TitleSequencePhase.NintendoText] = "NintendoText",
        [TitleSequencePhase.UpperPan] = "UpperPan",
        [TitleSequencePhase.PresentsText] = "PresentsText",
        [TitleSequencePhase.DownwardPan] = "DownwardPan",
        [TitleSequencePhase.MetroidThreeText] = "MetroidThreeText",
        [TitleSequencePhase.FinalZoom] = "FinalZoom",
        [TitleSequencePhase.TitleLogoFade] = "TitleLogoFade",
        [TitleSequencePhase.CopyrightFade] = "CopyrightFade",
        [TitleSequencePhase.TitleScreenFadeIn] = "TitleScreenFadeIn",
        [TitleSequencePhase.TitleScreen] = "TitleScreen",
        [TitleSequencePhase.TitleScreenFadeOutToFileSelect] = "TitleScreenFadeOutToFileSelect",
        [TitleSequencePhase.TitleScreenFadeOutToDemo] = "TitleScreenFadeOutToDemo",
    });
}
