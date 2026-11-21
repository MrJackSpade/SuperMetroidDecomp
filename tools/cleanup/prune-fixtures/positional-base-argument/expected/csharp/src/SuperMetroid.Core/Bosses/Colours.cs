namespace SuperMetroid.Core.Bosses;

/// <summary>The shade a boss's parts show, as the colours his body shows hold it.</summary>
public sealed class Colours
{
    /// <summary>What his body shows.</summary>
    private BodyShown shown = new RoomColours();

    /// <summary>The shade his parts show, or null before any band.</summary>
    public Tint? PartsShade => shown.PartsShade;

    /// <summary>How strong a pale glow is.</summary>
    public static Tint GlowStrength => new Glow(Tint.Pale).Strength;

    /// <summary>Shows a band of <paramref name="tint"/>.</summary>
    /// <param name="tint">The band's shade.</param>
    public void Show(Tint tint) => shown = new BandShade(tint);

    /// <summary>What his body shows, each case with the shade his parts show.</summary>
    /// <param name="PartsShade">The shade his parts show, or null before any band.</param>
    private abstract record BodyShown(Tint? PartsShade);

    /// <summary>His room's own colours, before any band.</summary>
    private sealed record RoomColours() : BodyShown((Tint?)null);

    /// <summary>A band's shade, on his body and his parts.</summary>
    private sealed record BandShade : BodyShown
    {
        /// <summary>Creates the record from what its base takes.</summary>
        /// <param name="shade">The shade, which his body's colours take.</param>
        public BandShade(Tint shade)
            : base(shade)
        {
        }
    }

    /// <summary>A glow, whose strength its level gives.</summary>
    /// <param name="Level">The level, which gives the glow's strength.</param>
    private sealed record Glow(Tint Level) : BodyShown((Tint?)null)
    {
        /// <summary>How strong the glow is.</summary>
        public Tint Strength { get; } = Level;
    }
}
