namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour counted on the game's fade scale (#1544): red, green and blue, each from zero to <see cref="FullLevel"/>, the
/// thirty-two levels spread evenly over a plain colour's channel with both ends included. Every colour the game ships was
/// authored on this scale, and every fade, flash, glow, ramp and pre-coloured faded shade the game shows steps through it:
/// each computes its steps on these levels, exactly as the game's level arithmetic does, and shows the plain colour of the
/// level it reaches (<see cref="ToColour"/>). Every colour the game shows therefore lies on the scale, and a colour off it
/// is rejected wherever its levels are read (<see cref="Of"/>).
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

    /// <summary>
    /// The level whose plain value each channel value 0-<see cref="RgbColor.FullChannel"/> is, or none for a value that is
    /// no level's (#1627), so reading a colour's levels costs a look-up a channel.
    /// </summary>
    private static readonly int?[] LevelByChannel = LevelsByChannel();

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

    /// <summary>The levels of <paramref name="colour"/>, which must lie on the scale, as every colour the game shows does.</summary>
    /// <exception cref="ArgumentException">A channel of <paramref name="colour"/> is not one of the thirty-two levels.</exception>
    public static FadeLevelColour Of(RgbColor colour) =>
        new(LevelOf(colour.Red, colour), LevelOf(colour.Green, colour), LevelOf(colour.Blue, colour));

    /// <summary>The plain colour these levels show: the levels spread evenly over each channel from none to full.</summary>
    public RgbColor ToColour() => new(Shown(Red), Shown(Green), Shown(Blue));

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

    /// <inheritdoc />
    public override string ToString() => $"Levels(r{Red}, g{Green}, b{Blue})";

    /// <summary>The level whose plain value is <paramref name="channel"/>, one of <paramref name="colour"/>'s channels.</summary>
    /// <exception cref="ArgumentException"><paramref name="channel"/> is not the plain value of a level.</exception>
    private static int LevelOf(int channel, RgbColor colour)
    {
        return LevelByChannel[channel] is int level
            ? level
            : throw new ArgumentException($"{colour} is not a colour of thirty-two levels a channel.", nameof(colour));
    }

    /// <summary>The plain channel value of a level already known to lie in 0-<see cref="FullLevel"/>.</summary>
    private static int Shown(int level) => level * ChannelValuesPerLevel + level / LevelsPerSpread;

    /// <summary>Builds <see cref="LevelByChannel"/>: each level at its plain value, every other channel value no level's.</summary>
    private static int?[] LevelsByChannel()
    {
        var levels = new int?[RgbColor.FullChannel + 1];
        for (int level = 0; level <= FullLevel; level++)
            levels[Shown(level)] = level;
        return levels;
    }

    /// <summary>Rejects a level outside 0-<see cref="FullLevel"/>.</summary>
    private static int Level(int value, string name) =>
        value is >= 0 and <= FullLevel
            ? value
            : throw new ArgumentOutOfRangeException(name, value, $"A colour level is 0-{FullLevel}.");
}
