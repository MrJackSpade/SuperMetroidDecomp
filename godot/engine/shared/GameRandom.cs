namespace Engine.Shared;

/// <summary>
/// The gameplay random generator. A session has one, shared by everything in gameplay that needs chance, and the order
/// of draws is part of how the game plays out. Effects that are only for show never draw from it.
/// </summary>
public sealed class GameRandom
{
    /// <summary>The sequence the draws come from.</summary>
    private readonly IRandomSequence _sequence;

    /// <summary>A generator drawing from <paramref name="sequence"/>.</summary>
    public GameRandom(IRandomSequence sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        _sequence = sequence;
    }

    /// <summary>The next number of the sequence, as it comes.</summary>
    public uint Next() => _sequence.Next();

    /// <summary>The number last drawn, without drawing again.</summary>
    public RandomDraw Last => new(_sequence.Current);

    /// <summary>Starts the sequence again from <paramref name="seed"/>, as a game whose outcomes follow a fixed course does at set points.</summary>
    public void Restart(uint seed) => _sequence.Restart(seed);

    /// <summary>
    /// Draws the next number of the sequence for something that settles several things from one draw, each from its
    /// own part of the number.
    /// </summary>
    public RandomDraw Draw() => new(_sequence.Next());

    /// <summary>
    /// A chance offset about a place, for something thrown a little way from where it is. One number is drawn to
    /// settle which way it goes, by its two highest digits of sixteen: leftward and upward. Then a distance is drawn
    /// across and another down, each a number below the spread less how far into the spread the middle lies, drawn
    /// again for as long as it falls short of the middle. The spreads are powers of two.
    /// </summary>
    public PixelOffset Scatter(int width, int height, int middleX, int middleY)
    {
        RandomDraw signs = Draw();
        int across = ScatterDistance(width, middleX), down = ScatterDistance(height, middleY);
        return PixelOffset.FromWhole(signs.Part(15, 1) == 1 ? -across : across, signs.Part(14, 1) == 1 ? -down : down);
    }

    /// <summary>A chance distance within a spread, measured from its middle; a draw short of the middle is drawn again.</summary>
    private int ScatterDistance(int span, int middle)
    {
        if (span <= 0 || (span & (span - 1)) != 0 || middle < 0 || middle >= span)
            throw new ArgumentOutOfRangeException(nameof(span), span, "A scatter's spread is a power of two with its middle inside it.");
        while (true)
        {
            int distance = (int)(Draw().Number % (uint)span) - middle;
            if (distance >= 0)
                return distance;
        }
    }

    /// <summary>A number from 0 up to, not including, <paramref name="count"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The count is not positive.</exception>
    public int NextBelow(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        return (int)(_sequence.Next() % (uint)count);
    }
}
