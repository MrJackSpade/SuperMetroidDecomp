namespace SuperMetroid.Core.Frontend;

/// <summary>
/// How bright a frontend screen is: a level from 0 (black) through 15 (fully lit), or blanked. A blanked screen shows
/// nothing; it is what a completed fade-out leaves, and fading in from it starts again at level one. A screen at level
/// zero that is not blanked looks the same but has not finished fading out.
/// </summary>
public readonly record struct ScreenBrightness
{
    /// <summary>The fully lit level.</summary>
    public const int FullLevel = 15;

    /// <summary>Stores a validated level and blanking; callers use <see cref="Lit"/>, <see cref="Blanked"/> or <see cref="FullyLit"/>.</summary>
    private ScreenBrightness(int level, bool isBlanked)
    {
        Level = level;
        IsBlanked = isBlanked;
    }

    /// <summary>A blanked screen, showing nothing.</summary>
    public static ScreenBrightness Blanked => new(0, true);

    /// <summary>A fully lit screen.</summary>
    public static ScreenBrightness FullyLit => new(FullLevel, false);

    /// <summary>A screen shown at <paramref name="level"/>, 0 (black) through <see cref="FullLevel"/>.</summary>
    public static ScreenBrightness Lit(int level) => new(Ensure.BetweenInclusive(0, FullLevel, level), false);

    /// <summary>The level the screen shows at, 0 (black) through <see cref="FullLevel"/>; a blanked screen's is zero.</summary>
    [IntegerQuantity("screen brightness level, 0 black through 15 fully lit")]
    public int Level { get; }

    /// <summary>Whether the screen is blanked, which is how a fade-out reports that it has finished.</summary>
    public bool IsBlanked { get; }

    /// <summary>The same level, no longer blanked: a blanked screen becomes level zero.</summary>
    public ScreenBrightness Unblanked() => Lit(Level);
}
