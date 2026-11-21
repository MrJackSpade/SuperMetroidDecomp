namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One piece of the hatching egg's shell (#1500): where it starts, how fast it flies sideways and what it shows.</summary>
/// <param name="Start">Where the piece appears as the egg hatches.</param>
/// <param name="LaunchSpeed">How far the piece flies sideways each update.</param>
/// <param name="Animation">The animation the piece shows while it flies.</param>
internal sealed record IntroEggShellPiece(Offset16<ScreenPixels> Start, SignedSixteenSixteen LaunchSpeed, IntroEggEffectAnimation Animation);
