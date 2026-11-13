namespace SuperMetroid.Core.Fx;

/// <summary>
/// One band of the scrolling sky (#1687): the room rows from <paramref name="TopRow"/> down to the next band's top, or
/// to the sky's end, slide sideways together with <paramref name="Drift"/>.
/// </summary>
/// <param name="TopRow">The first room row the band covers.</param>
/// <param name="Drift">The drift the band's rows move with.</param>
public sealed record SkyBand(int TopRow, SkyDrift Drift);
