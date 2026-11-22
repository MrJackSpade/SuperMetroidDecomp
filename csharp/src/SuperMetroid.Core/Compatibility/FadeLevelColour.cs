using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A colour counted in the game's colour steps (#1544, #2563): red, green and blue, each from zero to <see cref="FullLevel"/>,
/// the thirty-two steps spread evenly over a plain colour's channel with both ends included. Every colour the game ships lies
/// on these steps; a colour off them is rejected wherever its steps are read (<see cref="Of"/>).
/// </summary>
/// <remarks>
/// <para>
/// Gameplay behaviour reproduced: the colours that glow and fade a step a channel an update, Kraid's eyes
/// (<see cref="KraidEyeGlow"/>), the Wrecked Ship ghosts (<see cref="WreckedShipGhostFade"/>), the Morph Ball eye's beam, the
/// power bomb's afterglow and the suit pickup's beam, take as many updates as the steps between where they start and
/// where they end, so a colour's steps time Kraid's roar, a ghost's appearing and the eye beam's removal.
/// </para>
/// <para>
/// A fade is not counted here: the simulation names how far a fade has come (<see cref="FadeLevel"/>) and the backend blends
/// it (<see cref="IColourBlend"/>). The backend and the import read a colour's steps through this type, so the steps a
/// colour shows are stated once.
/// </para>
/// <para>
/// Evidence: the original's glow and fade routines step each colour channel one of its thirty-two steps an update and test
/// the steps they reach; the Kraid replays confirm the roar's timing.
/// </para>
/// </remarks>
[CompatibilityService("colours that glow and fade a step a channel an update, of the thirty-two steps a channel holds, so a colour's steps time Kraid's roar, a ghost's appearing and the Morph Ball eye beam's removal")]
public readonly partial record struct FadeLevelColour
{
    /// <summary>A channel's highest step.</summary>
    public const int FullLevel = 31;

    /// <summary>The plain channel values each step spans.</summary>
    private const int ChannelValuesPerLevel = (RgbColor.FullChannel + 1) / (FullLevel + 1);

    /// <summary>
    /// The steps whose spread adds one more plain channel value: the top step reaches full, so a step's plain value gains
    /// one for each of this many steps below it.
    /// </summary>
    private const int LevelsPerSpread = (FullLevel + 1) / ChannelValuesPerLevel;

    /// <summary>
    /// The step whose plain value each channel value 0-<see cref="RgbColor.FullChannel"/> is, or none for a value that is
    /// no step's (#1627), so reading a colour's steps costs a look-up a channel.
    /// </summary>
    private static readonly Dictionary<int, int> LevelByChannel = LevelsByChannel();

    /// <summary>Makes a colour from its three steps, each 0-<see cref="FullLevel"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A step is outside 0-<see cref="FullLevel"/>.</exception>
    public FadeLevelColour(int red, int green, int blue)
    {
        Red = Level(red, nameof(red));
        Green = Level(green, nameof(green));
        Blue = Level(blue, nameof(blue));
    }

    /// <summary>The red step, 0-<see cref="FullLevel"/>.</summary>
    public int Red { get; }

    /// <summary>The green step, 0-<see cref="FullLevel"/>.</summary>
    public int Green { get; }

    /// <summary>The blue step, 0-<see cref="FullLevel"/>.</summary>
    public int Blue { get; }

    /// <summary>One channel's step, 0-<see cref="FullLevel"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="channel"/> is not a defined channel.</exception>
    public int this[ColorChannel channel] => channel switch
    {
        ColorChannel.Red => Red,
        ColorChannel.Green => Green,
        ColorChannel.Blue => Blue,
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
    };

    /// <summary>The steps of <paramref name="colour"/>, which must lie on them, as every colour the game shows does.</summary>
    /// <exception cref="ArgumentException">A channel of <paramref name="colour"/> is not one of the thirty-two steps.</exception>
    public static FadeLevelColour Of(RgbColor colour) =>
        new(LevelOf(colour.Red, colour), LevelOf(colour.Green, colour), LevelOf(colour.Blue, colour));

    /// <summary>The plain colour these steps show: the steps spread evenly over each channel from none to full.</summary>
    public RgbColor ToColour() => new(Shown(Red), Shown(Green), Shown(Blue));

    /// <summary>
    /// The grey <paramref name="level"/> steps above black in every channel: black at zero, white at
    /// <see cref="FullLevel"/>.
    /// </summary>
    /// <param name="level">The grey's step, from zero through <see cref="FullLevel"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is below black or above white.</exception>
    public static FadeLevelColour Grey(int level) => new(level, level, level);

    /// <summary>These steps with one channel's replaced.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="channel"/> is not a defined channel.</exception>
    public FadeLevelColour With(ColorChannel channel, int value) => channel switch
    {
        ColorChannel.Red => WithRed(value),
        ColorChannel.Green => WithGreen(value),
        ColorChannel.Blue => WithBlue(value),
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Undefined color channel."),
    };

    /// <summary>These steps with red replaced.</summary>
    public FadeLevelColour WithRed(int red) => new(red, Green, Blue);

    /// <summary>These steps with green replaced.</summary>
    public FadeLevelColour WithGreen(int green) => new(Red, green, Blue);

    /// <summary>These steps with blue replaced.</summary>
    public FadeLevelColour WithBlue(int blue) => new(Red, Green, blue);

    /// <summary>Applies <paramref name="map"/> to each channel's step; the results must be steps.</summary>
    public FadeLevelColour Map(Func<ColorChannel, int, int> map)
    {
        Ensure.NotNull(map);
        return new(map(ColorChannel.Red, Red), map(ColorChannel.Green, Green), map(ColorChannel.Blue, Blue));
    }

    /// <inheritdoc />
    public override string ToString() => $"Levels(r{Red}, g{Green}, b{Blue})";

    /// <summary>The step whose plain value is <paramref name="channel"/>, one of <paramref name="colour"/>'s channels.</summary>
    /// <exception cref="ArgumentException"><paramref name="channel"/> is not the plain value of a step.</exception>
    private static int LevelOf(int channel, RgbColor colour)
    {
        return LevelByChannel.TryGetValue(channel, out int level)
            ? level
            : throw new ArgumentException($"{colour} is not a colour of thirty-two levels a channel.", nameof(colour));
    }

    /// <summary>The plain channel value of a step already known to lie in 0-<see cref="FullLevel"/>.</summary>
    private static int Shown(int level) => level * ChannelValuesPerLevel + level / LevelsPerSpread;

    /// <summary>Builds <see cref="LevelByChannel"/>: each step at its plain value, every other channel value no step's.</summary>
    private static Dictionary<int, int> LevelsByChannel()
    {
        var levels = new Dictionary<int, int>();
        for (int level = 0; level <= FullLevel; level++)
            levels[Shown(level)] = level;
        return levels;
    }

    /// <summary>Rejects a step outside 0-<see cref="FullLevel"/>.</summary>
    private static int Level(int value, string name) =>
        value is >= 0 and <= FullLevel
            ? value
            : throw new ArgumentOutOfRangeException(name, value, $"A colour level is 0-{FullLevel}.");
}
