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

    /// <summary>The shot that carries the echo, or none before the echo first departs and after Samus's shots are cleared.</summary>
    private SamusProjectile? Shot { get; set; }

    /// <summary>Where the echo is shown, or none once no shot carries it.</summary>
    public Position16<RoomPixels>? ShownPosition =>
        Shot is { Behavior: ShinesparkEchoFlight } ? _position : null;

    /// <summary>
    /// Which of the crash's two echoes this is: the first's flight leaves its height for the crash's orbit, and clears it
    /// once it leaves the screen (<see cref="SamusShinesparkState.FirstReleasedEcho"/>).
    /// </summary>
    internal ShinesparkCrashEchoRelease Release { get; } = release;

    /// <summary>The echo departs on <paramref name="shot"/> from where the crash finished.</summary>
    /// <param name="shot">The shot that now carries the echo.</param>
    /// <param name="crashPosition">Samus's position as the crash finishes.</param>
    internal void Depart(SamusProjectile shot, Position16<RoomPixels> crashPosition)
    {
        Shot = Ensure.NotNull(shot);
        _position = crashPosition;
    }

    /// <summary>The echo's shot has moved it to <paramref name="position"/>.</summary>
    /// <param name="position">Where the shot placed the echo this update.</param>
    internal void MoveTo(Position16<RoomPixels> position) => _position = position;

    /// <summary>Samus's shots have been cleared, and with them the echo.</summary>
    internal void Forget() => Shot = null;
}
