namespace SuperMetroid.Core.Assets;

/// <summary>
/// A colour partway through a fade between the colour an ink shows and one endpoint colour (#2289, #2563): the fade names
/// its endpoint, its direction and how far it has come (<see cref="FadeLevel"/>), and the backend blends the two colours
/// (<see cref="IColourBlend"/>). A door's fades, the death fade and the time-up whiteout show the whole display so.
/// </summary>
public sealed record ColourFade : ColourEffect
{
    /// <summary>Keeps the fade's endpoint colour, direction and progress.</summary>
    /// <param name="endpoint">The endpoint colour: the one faded toward, or the one faded in from.</param>
    /// <param name="direction">Whether the colour shown fades toward the endpoint colour or in from it.</param>
    /// <param name="progress">How far the fade has come.</param>
    public ColourFade(RgbColor endpoint, GradualFadeDirection direction, FadeLevel progress)
    {
        if (!Enum.IsDefined(direction))
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined gradual fade direction.");
        Endpoint = endpoint;
        Direction = direction;
        Progress = progress;
    }

    /// <summary>The endpoint colour: the one faded toward, or the one faded in from.</summary>
    public RgbColor Endpoint { get; }

    /// <summary>Whether the colour shown fades toward <see cref="Endpoint"/> or in from it.</summary>
    public GradualFadeDirection Direction { get; }

    /// <summary>How far the fade has come.</summary>
    public FadeLevel Progress { get; }

    /// <inheritdoc />
    public override RgbColor Apply(RgbColor shown, IColourBlend blend) =>
        Ensure.NotNull(blend).Partway(Direction == GradualFadeDirection.Toward ? shown : Endpoint,
            Direction == GradualFadeDirection.Toward ? Endpoint : shown, Progress);
}
