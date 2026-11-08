namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour counted in the levels the game's authored colours, ramps and step-wise fades move through (#1544): red, green
/// and blue, each from zero to <see cref="FullLevel"/>, the thirty-two levels spread evenly over a plain colour's channel
/// with both ends included. Every colour the game ships was authored on this scale, so each colour the import supplies
/// is exactly one of these levels. The sites that compute a colour from authored levels (a palette animation's ramp, a
/// glow, a level-by-level fade) count in it, as do the few whose timing depends on when a level-by-level change ends; a
/// plain colour operation (<see cref="RgbColor.Blend"/>, <see cref="RgbColor.Scaled"/>) needs no levels.
/// </summary>
public readonly record struct FadeLevelColour
{
    /// <summary>A channel's highest level.</summary>
    public const int FullLevel = 31;

    /// <summary>The plain channel values each level spans.</summary>
    private const int ChannelValuesPerLevel = (RgbColor.FullChannel + 1) / (FullLevel + 1);

    /// <summary>
    /// The levels whose spread adds one more plain channel value: the top level reaches full, so a level's plain value
    /// gains one for each of this many levels below it.
    /// </summary>
    private const int LevelsPerSpread = (FullLevel + 1) / ChannelValuesPerLevel;

    /// <summary>Makes a colour from its three levels, each 0-<see cref="FullLevel"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A level is outside 0-<see cref="FullLevel"/>.</exception>
    public FadeLevelColour(int red, int green, int blue)
    {
        Red = Level(red, nameof(red));
        Green = Level(green, nameof(green));
        Blue = Level(blue, nameof(blue));
    }

    /// <summary>The red level, 0-<see cref="FullLevel"/>.</summary>
    public int Red { get; }

    /// <summary>The green level, 0-<see cref="FullLevel"/>.</summary>
    public int Green { get; }

    /// <summary>The blue level, 0-<see cref="FullLevel"/>.</summary>
    public int Blue { get; }

    /// <summary>One channel's level, 0-<see cref="FullLevel"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="channel"/> is not a defined channel.</exception>
    public int this[ColorChannel channel] => channel switch
    {
        ColorChannel.Red => Red,
        ColorChannel.Green => Green,
        ColorChannel.Blue => Blue,
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
    };

    /// <summary>The levels of <paramref name="colour"/>, which must lie on the scale.</summary>
    /// <exception cref="ArgumentException">A channel of <paramref name="colour"/> is not one of the thirty-two levels.</exception>
    public static FadeLevelColour Of(RgbColor colour) =>
        new(LevelOf(colour.Red, colour), LevelOf(colour.Green, colour), LevelOf(colour.Blue, colour));

    /// <summary>
    /// The levels nearest <paramref name="colour"/>'s channels, a channel halfway between two levels taking the lower: the
    /// colour's own levels when it lies on the scale, as every authored colour does, and the closest the scale comes to a
    /// colour a plain blend or scale made.
    /// </summary>
    public static FadeLevelColour Nearest(RgbColor colour) =>
        new(NearestLevel(colour.Red), NearestLevel(colour.Green), NearestLevel(colour.Blue));

    /// <summary>The plain channel value <paramref name="level"/> shows: the levels spread evenly from none to full.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> is outside 0-<see cref="FullLevel"/>.</exception>
    public static int ChannelOf(int level)
    {
        _ = Level(level, nameof(level));
        return level * ChannelValuesPerLevel + level / LevelsPerSpread;
    }

    /// <summary>The plain colour these levels show.</summary>
    public RgbColor ToColour() => new(ChannelOf(Red), ChannelOf(Green), ChannelOf(Blue));

    /// <summary>These levels with one channel's replaced.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="channel"/> is not a defined channel.</exception>
    public FadeLevelColour With(ColorChannel channel, int value) => channel switch
    {
        ColorChannel.Red => WithRed(value),
        ColorChannel.Green => WithGreen(value),
        ColorChannel.Blue => WithBlue(value),
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
    };

    /// <summary>These levels with red replaced.</summary>
    public FadeLevelColour WithRed(int red) => new(red, Green, Blue);

    /// <summary>These levels with green replaced.</summary>
    public FadeLevelColour WithGreen(int green) => new(Red, green, Blue);

    /// <summary>These levels with blue replaced.</summary>
    public FadeLevelColour WithBlue(int blue) => new(Red, Green, blue);

    /// <summary>Applies <paramref name="map"/> to each channel's level; the results must be levels.</summary>
    public FadeLevelColour Map(Func<ColorChannel, int, int> map)
    {
        Ensure.NotNull(map);
        return new(map(ColorChannel.Red, Red), map(ColorChannel.Green, Green), map(ColorChannel.Blue, Blue));
    }

    /// <summary>Combines matching channels' levels of these and <paramref name="other"/>; the results must be levels.</summary>
    public FadeLevelColour Zip(FadeLevelColour other, Func<ColorChannel, int, int, int> combine)
    {
        Ensure.NotNull(combine);
        return new(combine(ColorChannel.Red, Red, other.Red), combine(ColorChannel.Green, Green, other.Green),
            combine(ColorChannel.Blue, Blue, other.Blue));
    }

    /// <inheritdoc />
    public override string ToString() => $"Levels(r{Red}, g{Green}, b{Blue})";

    /// <summary>The level whose plain value is <paramref name="channel"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="channel"/> is not the plain value of a level.</exception>
    private static int LevelOf(int channel, RgbColor colour)
    {
        int level = channel / ChannelValuesPerLevel;
        return ChannelOf(level) == channel
            ? level
            : throw new ArgumentException($"{colour} is not a colour of thirty-two levels a channel.", nameof(colour));
    }

    /// <summary>The level whose plain value lies nearest <paramref name="channel"/>, the lower of two equally near.</summary>
    private static int NearestLevel(int channel)
    {
        int below = Math.Min(FullLevel, channel / ChannelValuesPerLevel);
        if (ChannelOf(below) > channel)
            below--;
        if (below == FullLevel)
            return below;
        return channel - ChannelOf(below) <= ChannelOf(below + 1) - channel ? below : below + 1;
    }

    /// <summary>Rejects a level outside 0-<see cref="FullLevel"/>.</summary>
    private static int Level(int value, string name) =>
        value is >= 0 and <= FullLevel
            ? value
            : throw new ArgumentOutOfRangeException(name, value, $"A colour level is 0-{FullLevel}.");
}
