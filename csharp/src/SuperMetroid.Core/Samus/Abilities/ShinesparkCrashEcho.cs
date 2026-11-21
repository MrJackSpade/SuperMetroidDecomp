using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// One of the two echoes a shinespark crash releases as it finishes: a shot that flies out from Samus along its
/// departure angle, further each update by <see cref="ShinesparkDefinitions.EchoRadiusStep"/>, and leaves once it is off the screen. The crash shows the echo
/// where its shot last placed it for as long as the shot carries it, and on Samus on the update it departs, before its
/// shot first moves.
/// </summary>
/// <param name="release">Which of the crash's two echoes this is.</param>
public sealed class ShinesparkCrashEcho(ShinesparkCrashEchoRelease release)
{
    /// <summary>Where the echo was last placed: Samus's position as it departed, then where its shot moved it.</summary>
    private Position16<RoomPixels> _position;

    /// <summary>
    /// The shot that carries the echo with the flight it departed on, or none before the echo first departs and after
    /// Samus's shots are cleared.
    /// </summary>
    private Carrier? _carrier;

    /// <summary>
    /// Where the echo is shown, or none once no shot carries it: once its shot's place no longer holds the flight it
    /// departed on, another shot has taken the place.
    /// </summary>
    public Position16<RoomPixels>? ShownPosition =>
        _carrier is { } carrier && carrier.Shot.Occupant is { } held && ReferenceEquals(held.KindState, carrier.Flight) ? _position : null;

<<<<<<< ours
    /// <summary>Whether <paramref name="shot"/> is the shot that carries this echo.</summary>
    /// <param name="shot">A shot running the echo's update.</param>
    internal bool IsCarriedBy(SamusProjectile shot) => _carrier is { } carrier && ReferenceEquals(carrier.Shot, shot);
||||||| base
    /// <summary>Whether <paramref name="shot"/> is the shot that carries this echo.</summary>
    /// <param name="shot">A shot running the echo's update.</param>
    internal bool IsCarriedBy(SamusProjectile shot) => ReferenceEquals(Shot, shot);
=======
    /// <summary>
    /// Which of the crash's two echoes this is: the first's flight leaves its height for the crash's orbit, and clears it
    /// once it leaves the screen (<see cref="SamusShinesparkState.FirstReleasedEcho"/>).
    /// </summary>
    internal ShinesparkCrashEchoRelease Release { get; } = release;
>>>>>>> theirs

    /// <summary>The echo departs on <paramref name="shot"/>, flying <paramref name="flight"/>, from where the crash finished.</summary>
    /// <param name="shot">The shot that now carries the echo.</param>
    /// <param name="flight">The flight the shot's place holds for the echo.</param>
    /// <param name="crashPosition">Samus's position as the crash finishes.</param>
    internal void Depart(SamusProjectile shot, ShinesparkEchoShotState flight, Position16<RoomPixels> crashPosition)
    {
        _carrier = new Carrier(Ensure.NotNull(shot), Ensure.NotNull(flight));
        _position = crashPosition;
    }

    /// <summary>The echo's shot has moved it to <paramref name="position"/>.</summary>
    /// <param name="position">Where the shot placed the echo this update.</param>
    internal void MoveTo(Position16<RoomPixels> position) => _position = position;

    /// <summary>Samus's shots have been cleared, and with them the echo.</summary>
    internal void Forget() => _carrier = null;

    /// <summary>The shot carrying the echo and the flight its place holds for it.</summary>
    /// <param name="Shot">The shot.</param>
    /// <param name="Flight">The flight.</param>
    private sealed record Carrier(SamusProjectile Shot, ShinesparkEchoShotState Flight);
}
