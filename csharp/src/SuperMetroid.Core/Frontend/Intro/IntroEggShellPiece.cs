namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One piece of the hatching egg's shell (#1500): where it starts and how fast it flies sideways. What it shows is
/// the piece's own animation, keyed by its fragment.</summary>
/// <param name="Start">Where the piece appears as the egg hatches.</param>
/// <param name="LaunchSpeed">How far the piece flies sideways each update.</param>
internal sealed record IntroEggShellPiece(Offset16<ScreenPixels> Start, SignedSixteenSixteen LaunchSpeed);
