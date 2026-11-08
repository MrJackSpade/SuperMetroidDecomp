namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour shown at a level between black and itself (#1512, #1544): at level <see cref="Level"/> of <see cref="Levels"/>,
/// each of the colour's fade levels (<see cref="FadeLevelColour"/>) is scaled by that fraction and rounded down, as the
/// opening's cross-fader shows each colour's fade, and the colour shown is that of the levels it reaches.
/// </summary>
public sealed record ColourLevel : ColourEffect
{
    /// <summary>Keeps the level and its scale.</summary>
    /// <param name="level">How bright the colour shows, from zero (black) to <paramref name="levels"/> (itself).</param>
    /// <param name="levels">The steps between black and the colour, at least one.</param>
    public ColourLevel(int level, int levels)
    {
        Levels = Ensure.BetweenInclusive(1, int.MaxValue, levels);
        Level = Ensure.BetweenInclusive(0, levels, level);
    }

    /// <summary>How bright the colour shows.</summary>
    public int Level { get; }

    /// <summary>The steps between black and the colour.</summary>
    public int Levels { get; }

    /// <inheritdoc />
    public override RgbColor Apply(RgbColor shown)
    {
        FadeLevelColour levels = shown.FadeLevels;
        return new FadeLevelColour(Scaled(levels.Red), Scaled(levels.Green), Scaled(levels.Blue)).ToColour();
    }

    /// <summary>One fade level scaled to <see cref="Level"/> of <see cref="Levels"/>, rounded down.</summary>
    private int Scaled(int level) => level * Level / Levels;
}
