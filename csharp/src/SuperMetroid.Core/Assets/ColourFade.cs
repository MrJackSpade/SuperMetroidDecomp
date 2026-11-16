namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour partway through a fade between the colour an ink shows and one endpoint colour (#2289): each channel moves from
/// the colour the fade starts at toward the one it ends on by the fade's progress, truncated to a whole level, as a typed
/// fade between two shadings moves (<see cref="PictureShadeFade"/>). A door's fades, the death fade and the time-up whiteout
/// show the whole display so.
/// </summary>
public sealed record ColourFade : ColourEffect
{
    /// <summary>Keeps the fade's endpoint colour, direction and progress.</summary>
    /// <param name="endpoint">The endpoint colour: the one faded toward, or the one faded in from.</param>
    /// <param name="direction">Whether the colour shown fades toward the endpoint colour or in from it.</param>
    /// <param name="progress">How far the fade has come, zero through <paramref name="length"/>.</param>
    /// <param name="length">The fade's length, one or more.</param>
    public ColourFade(RgbColor endpoint, GradualFadeDirection direction, int progress, int length)
    {
        if (!Enum.IsDefined(direction))
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined gradual fade direction.");
        Endpoint = endpoint;
        Direction = direction;
        Length = Ensure.BetweenInclusive(1, int.MaxValue, length);
        Progress = Ensure.BetweenInclusive(0, Length, progress);
    }

    /// <summary>The endpoint colour: the one faded toward, or the one faded in from.</summary>
    public RgbColor Endpoint { get; }

    /// <summary>Whether the colour shown fades toward <see cref="Endpoint"/> or in from it.</summary>
    public GradualFadeDirection Direction { get; }

    /// <summary>How far the fade has come, zero through <see cref="Length"/>.</summary>
    public int Progress { get; }

    /// <summary>The fade's length.</summary>
    public int Length { get; }

    /// <inheritdoc />
    public override RgbColor Apply(RgbColor shown)
    {
        RgbColor from = Direction == GradualFadeDirection.Toward ? shown : Endpoint;
        RgbColor to = Direction == GradualFadeDirection.Toward ? Endpoint : shown;
        return new RgbColor(Channel(from.Red, to.Red), Channel(from.Green, to.Green), Channel(from.Blue, to.Blue));
    }

    /// <summary>One channel partway from <paramref name="from"/> to <paramref name="to"/>.</summary>
    private int Channel(int from, int to) => from + ((to - from) * Progress / Length);
}
