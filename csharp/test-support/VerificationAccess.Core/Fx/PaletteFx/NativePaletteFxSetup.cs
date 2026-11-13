namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The cartridge's palette-FX setup routines, verification's native view (#1554): each names the
/// <see cref="ColourEffectStart"/> it stands for through <see cref="FxNativeWords.StartOf"/>.
/// </summary>
internal enum NativePaletteFxSetup
{
    /// <summary>The no-op setup: <see cref="ColourEffectStart.Plain"/>.</summary>
    Null = 1,
    /// <summary>The intro's setup, which installs a pre-instruction that does nothing: <see cref="ColourEffectStart.Plain"/>.</summary>
    Intro,
    /// <summary>Norfair's setup: <see cref="ColourEffectStart.ChoosesHeatAnimationForSuit"/>.</summary>
    Norfair,
    /// <summary>Brinstar's setup: <see cref="ColourEffectStart.SkippedOnceAreaMiniBossIsDead"/>.</summary>
    Brinstar,
}
