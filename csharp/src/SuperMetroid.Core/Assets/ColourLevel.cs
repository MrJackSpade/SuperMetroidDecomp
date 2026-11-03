namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour shown at a level between black and itself (#1512): at level <see cref="Level"/> of <see cref="Levels"/>,
/// each channel is scaled by that fraction and rounded down, as the opening's cross-fader shows each colour's fade.
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
    public override RgbColor Apply(RgbColor shown) => shown.Map((_, channel) => channel * Level / Levels);
}
