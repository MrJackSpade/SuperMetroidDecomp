namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One of the slime drops the hatched Baby Metroid sheds: how it drifts sideways and how it flies up and falls.</summary>
/// <param name="Drift">How far the drop moves sideways each update.</param>
/// <param name="Fall">The drop's vertical speed for each update it has flown.</param>
internal readonly record struct IntroEggSlimeLaunch(SignedSixteenSixteen Drift, IntroEggFallCurve Fall);
