namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A plane rippled sideways by a sine wave (#1468): every screen line shows the plane at <see cref="BaseScroll"/>, and
/// each line from <see cref="FirstLine"/> up to <see cref="EndLine"/> is pushed sideways by the wave at that line. The
/// backend's shader evaluates the wave line by line; nothing keeps a table of lines.
/// </summary>
/// <remarks>
/// The wave's angle at screen line <c>y</c> is <see cref="Phase"/> advanced by <c>y</c> of <see cref="Period"/>'s whole
/// turn, in the byte angle's steps rounded down. Its displacement is the game's signed sine sample at that angle
/// (<see cref="Trigonometry.SignedSine"/>) scaled by <see cref="Amplitude"/>, rounded toward zero, so the ripple has the
/// shape the game's other sine motion has.
/// </remarks>
/// <param name="BaseScroll">The horizontal scroll every line shows before its displacement, 0 through 65535.</param>
/// <param name="Amplitude">The wave's peak displacement in 1/256ths of a pixel, 0 through 65535.</param>
/// <param name="Phase">The wave's angle at screen line zero.</param>
/// <param name="Period">The screen lines of one whole cycle of the wave, at least one.</param>
/// <param name="FirstLine">The first screen line the wave displaces, 0 through the screen's height.</param>
/// <param name="EndLine">The screen line after the last the wave displaces, <paramref name="FirstLine"/> through the screen's height.</param>
public sealed record ScrollWave(int BaseScroll, int Amplitude, ByteAngle Phase, int Period, int FirstLine, int EndLine) : LineScroll
{
    /// <summary>The horizontal scroll every line shows before its displacement, 0 through 65535.</summary>
    public override int BaseScroll
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    } = Ensure.BetweenInclusive(0, ushort.MaxValue, BaseScroll);

    /// <summary>The wave's peak displacement in 1/256ths of a pixel, 0 through 65535.</summary>
    public int Amplitude
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    } = Ensure.BetweenInclusive(0, ushort.MaxValue, Amplitude);

    /// <summary>The screen lines of one whole cycle of the wave, 1 through 65535.</summary>
    public int Period
    {
        get;
        init => field = Ensure.BetweenInclusive(1, ushort.MaxValue, value);
    } = Ensure.BetweenInclusive(1, ushort.MaxValue, Period);

    /// <summary>The first screen line the wave displaces.</summary>
    public int FirstLine
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ScreenGeometry.ScreenHeightPixels, value);
    } = Ensure.BetweenInclusive(0, ScreenGeometry.ScreenHeightPixels, FirstLine);

    /// <summary>The screen line after the last the wave displaces; lines from it down show <see cref="BaseScroll"/>.</summary>
    public int EndLine
    {
        get;
        init => field = Ensure.BetweenInclusive(FirstLine, ScreenGeometry.ScreenHeightPixels, value);
    } = Ensure.BetweenInclusive(FirstLine, ScreenGeometry.ScreenHeightPixels, EndLine);
}
