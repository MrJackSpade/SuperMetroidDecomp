using Engine.Content;
using Engine.Shared;

namespace Engine.Actors.Movement;

/// <summary>
/// A curve of speeds: a list of marks, each a speed, that something moves along to speed up and slow down smoothly.
/// A place on the curve is counted in 256ths of a mark, so that a thing can creep along it by less than a mark an
/// update; its speed is that of the whole mark it has reached. Hops, lunges and swings that several enemies share
/// read their speeds from one of these.
/// </summary>
/// <remarks><code>{ "speeds": [0, 0.004, 0.012, 0.024] }</code></remarks>
public sealed class SpeedCurve
{
    /// <summary>How many parts a mark is divided into.</summary>
    public const int PartsPerMark = 256;

    private readonly IReadOnlyList<Pixels> _speeds;

    /// <summary>A curve with the given speed at each mark.</summary>
    public SpeedCurve(IReadOnlyList<Pixels> speeds)
    {
        if (speeds.Count == 0)
            throw new ArgumentException("A speed curve has at least one mark.", nameof(speeds));
        _speeds = speeds;
    }

    /// <summary>How many marks the curve has.</summary>
    public int Marks => _speeds.Count;

    /// <summary>Reads a curve from its data file.</summary>
    public static SpeedCurve Read(DataNode data) => new(data.List("speeds").Select(speed => speed.AsPixels()).ToList());

    /// <summary>The speed at a place on the curve, given in 256ths of a mark.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place lies before the curve's start or past its last mark.</exception>
    public Pixels SpeedAt(int place)
    {
        int mark = place >> 8;
        if (place < 0 || mark >= _speeds.Count)
            throw new ArgumentOutOfRangeException(nameof(place), place, $"The speed curve has marks 0 through {_speeds.Count - 1}.");
        return _speeds[mark];
    }

    /// <summary>
    /// The place a climb of <paramref name="height"/> starts from, when it moves back along the curve by
    /// <paramref name="step"/> each update until it reaches the start: the first place from which the speeds passed
    /// through, each cut off to a 256th of a pixel, add up to the height.
    /// </summary>
    public int PlaceForClimb(Pixels height, int step)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(step);
        int place = 0;
        Pixels climbed = Pixels.Zero;
        while (place + step < _speeds.Count * PartsPerMark)
        {
            place += step;
            Pixels speed = SpeedAt(place);
            climbed += speed.WithFraction(speed.Fraction & ~0xFF);
            if (climbed >= height)
                return place;
        }

        throw new InvalidOperationException($"A climb of {height} at {step} parts of a mark an update never reaches its height on this curve.");
    }
}
