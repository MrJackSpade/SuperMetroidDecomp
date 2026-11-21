namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>The flight of a Fireflea that circles where it started.</summary>
public sealed partial class CirclingFirefleaState : FirefleaFlightState
{
    /// <inheritdoc/>
    internal override void React<TContext>(FirefleaFlightStateReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).CirclingFirefleaState(this, context);

    /// <summary>Starts the circling flight its spawn settings chose, <paramref name="definition"/>.</summary>
    internal CirclingFirefleaState(FirefleaFlight.Circling definition) => Definition = Ensure.NotNull(definition);

    /// <summary>
    /// The circling flight its spawn settings chose (#2476): how far, and which way, the Fireflea turns around its circle
    /// each update. The circle's radius, 8 through 64 pixels, is its spawn settings' radius.
    /// </summary>
    internal FirefleaFlight.Circling Definition { get; }

    /// <summary>
    /// The circle angle, wrapping around the turn. Main AI uses its whole step as the circle's trigonometry angle.
    /// </summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>Initial whole-pixel room position used as the fixed circle center.</summary>
    public Position16<RoomPixels> Center { get; internal set; }
}
