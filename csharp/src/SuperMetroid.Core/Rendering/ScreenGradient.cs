using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A colour blended over a band of screen lines (#1468), described by its stops. Each stop's colour and blend hold from
/// its line down to the next stop's, and the last stop's down to <see cref="Bottom"/>. The gradient steps from stop to
/// stop rather than mixing neighbouring stops, as the title's glow changes its colour in whole steps where each band
/// begins. Lines above <see cref="Top"/> and from <see cref="Bottom"/> down are not blended.
/// </summary>
public sealed record ScreenGradient
{
    /// <summary>The stops from the top of the band down, each beginning on a lower line than the one before.</summary>
    public ImmutableArray<GradientStop> Stops { get; }

    /// <summary>The band's first line: the first stop's.</summary>
    public int Top => Stops[0].Line;

    /// <summary>The line just below the band.</summary>
    public int Bottom { get; }

    /// <summary>Describes a gradient by its stops and the line its band ends above.</summary>
    /// <param name="stops">At least one stop, top to bottom, each beginning on a lower line than the one before, the first on the screen.</param>
    /// <param name="bottom">The line just below the band: below the last stop's line and no lower than the screen's bottom.</param>
    /// <exception cref="ArgumentException"><paramref name="stops"/> is empty, starts above the screen or is not in top-to-bottom order.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bottom"/> is not below the last stop or lies below the screen.</exception>
    public ScreenGradient(ImmutableArray<GradientStop> stops, int bottom)
    {
        if (stops.IsDefaultOrEmpty)
            throw new ArgumentException("A gradient has at least one stop.", nameof(stops));
        if (stops[0].Line < 0)
            throw new ArgumentException("A gradient's first stop begins on the screen.", nameof(stops));
        for (int stop = 1; stop < stops.Length; stop++)
        {
            if (stops[stop].Line <= stops[stop - 1].Line)
                throw new ArgumentException("A gradient's stops begin on successively lower lines.", nameof(stops));
        }
        if (bottom <= stops[^1].Line || bottom > DisplayScreen.Height)
            throw new ArgumentOutOfRangeException(nameof(bottom), bottom, "A gradient's band ends below its last stop and on the screen.");
        Stops = stops;
        Bottom = bottom;
    }

    /// <summary>Whether <paramref name="other"/> describes the same band with the same stops.</summary>
    /// <param name="other">The gradient compared, or null.</param>
    /// <returns>True when both bands end on the same line and their stops agree in order.</returns>
    public bool Equals(ScreenGradient? other) =>
        other is not null && Bottom == other.Bottom && Stops.AsSpan().SequenceEqual(other.Stops.AsSpan());

    /// <summary>A hash of the band's end and every stop, consistent with <see cref="Equals(ScreenGradient)"/>.</summary>
    /// <returns>The hash.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Bottom);
        foreach (GradientStop stop in Stops)
            hash.Add(stop);
        return hash.ToHashCode();
    }
}
