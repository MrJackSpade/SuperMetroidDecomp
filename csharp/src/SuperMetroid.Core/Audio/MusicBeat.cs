namespace SuperMetroid.Core.Audio;

/// <summary>
/// The music's beat as the clip player keeps it (#1305, #1478, #1725): the part of the current beat already played, kept
/// as an exact fraction of a beat, and how long the beat lasts while no song plays, which starts as the clip set's
/// <see cref="ClipAudioLibrary.InitialBeat"/> and is then each song's last. A song change waits for the beat to complete,
/// so songs change in time with the music. Presentation only: no gameplay update reads it.
/// </summary>
internal sealed class MusicBeat
{
    /// <summary>The played part of the beat over <see cref="whole"/>: zero or more, and less than it.</summary>
    private long played;

    /// <summary>The denominator of the played part, at least one; the fraction is kept in lowest terms.</summary>
    private long whole = 1;

    /// <summary>A beat with nothing of it played yet, lasting <paramref name="held"/> while no song plays.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="held"/> is null.</exception>
    internal MusicBeat(BeatLength held) => Held = Ensure.NotNull(held);

    /// <summary>How long the beat lasts while no song plays.</summary>
    internal BeatLength Held { get; private set; }

    /// <summary>Holds <paramref name="length"/> while no song plays.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="length"/> is null.</exception>
    internal void Hold(BeatLength length) => Held = Ensure.NotNull(length);

    /// <summary>
    /// Plays <paramref name="frames"/> clip frames of the beat while it lasts <paramref name="length"/> and reports whether
    /// that completed it. The overshoot starts the next beat; further whole beats those frames span are not counted.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frames"/> is negative.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="length"/> is null.</exception>
    /// <exception cref="OverflowException">The beat lengths played so far share no denominator the fraction can hold.</exception>
    internal bool Advance(long frames, BeatLength length)
    {
        Ensure.AtLeastZero(frames);
        Ensure.NotNull(length);
        // played / whole + frames * Beats / Frames, over the common denominator whole * Frames.
        long denominator = checked(whole * length.Frames);
        long reached = checked((played * length.Frames) + (frames * length.Beats * whole));
        bool completed = reached >= denominator;
        reached %= denominator;
        long divisor = GreatestCommonDivisor(reached, denominator);
        played = reached / divisor;
        whole = denominator / divisor;
        return completed;
    }

    /// <summary>The greatest common divisor of <paramref name="value"/> (zero or more) and <paramref name="other"/> (at least one).</summary>
    private static long GreatestCommonDivisor(long value, long other)
    {
        while (value != 0)
        {
            long remainder = other % value;
            other = value;
            value = remainder;
        }
        return other;
    }
}
