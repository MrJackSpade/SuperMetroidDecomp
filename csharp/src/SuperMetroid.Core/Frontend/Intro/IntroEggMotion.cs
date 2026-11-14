namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Moves the hatching egg's shell pieces (<see cref="IntroEggMotionDefinitions"/>): where each starts, how fast it flies
/// sideways, and its vertical speed along <see cref="IntroEggMotionDefinitions.ShellFall"/> and past it.
/// </summary>
internal static class IntroEggMotion
{
    /// <summary>
    /// A shell piece's vertical speed on the update after it has flown <paramref name="updatesFlown"/> updates: along the
    /// shell's curve, then the installed steps past it (<paramref name="pastCurve"/>).
    /// </summary>
    /// <param name="updatesFlown">The updates the piece has flown.</param>
    /// <param name="pastCurve">The installed steps a piece falls by past the curve.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="updatesFlown"/> is negative or past every piece's landing.</exception>
    public static SignedSixteenSixteen FragmentFallSpeed(int updatesFlown, ShellFragmentFallPastCurve pastCurve) =>
        updatesFlown >= IntroEggMotionDefinitions.ShellFall.Updates
            ? Ensure.NotNull(pastCurve).Step(updatesFlown - IntroEggMotionDefinitions.ShellFall.Updates)
            : IntroEggMotionDefinitions.ShellFall.Speed(updatesFlown);

    /// <summary>Where <paramref name="fragment"/> appears as the egg hatches.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="fragment"/> is not a piece of the shell.</exception>
    public static Position16<ScreenPixels> FragmentInitialPosition(IntroEggShellFragment fragment) =>
        IntroEggMotionDefinitions.FragmentInitialPositions.TryGetValue(fragment, out Position16<ScreenPixels> position)
            ? position : throw new ArgumentOutOfRangeException(nameof(fragment));

    /// <summary>How far <paramref name="fragment"/> flies sideways each update.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="fragment"/> is not a piece of the shell.</exception>
    public static SignedSixteenSixteen FragmentX(IntroEggShellFragment fragment) =>
        IntroEggMotionDefinitions.FragmentXVelocities.TryGetValue(fragment, out SignedSixteenSixteen velocity)
            ? velocity : throw new ArgumentOutOfRangeException(nameof(fragment));
}
