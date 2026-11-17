namespace SuperMetroid.Core.Frontend;

/// <summary>Which step of an escape timer's expiry (<see cref="Progression.SuperMetroidGameState.TimeUp"/>) runs.</summary>
internal enum TimeUpStep
{
    /// <summary>Gameplay continues while every colour whitens.</summary>
    WhiteningOut,
    /// <summary>
    /// The whitened screen fades to black without gameplay, then Ceres is destroyed with Samus aboard or, on Zebes, the
    /// death black-out follows.
    /// </summary>
    FadingToBlack,
}
