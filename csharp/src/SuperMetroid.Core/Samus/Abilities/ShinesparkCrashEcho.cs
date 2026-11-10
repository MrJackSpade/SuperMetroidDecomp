using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// One of the two echoes a shinespark crash releases as it finishes: a shot that flies out from Samus along its
/// departure angle, eight pixels further each update, and leaves once it is off the screen. The crash shows the echo
/// where its shot last placed it for as long as the shot carries it, and on Samus on the update it departs, before its
/// shot first moves.
/// </summary>
public sealed class ShinesparkCrashEcho
{
    /// <summary>Where the echo was last placed: Samus's position as it departed, then where its shot moved it.</summary>
    private Position16<RoomPixels> _position;

    /// <summary>The shot that carries the echo, or none before the echo first departs and after Samus's shots are cleared.</summary>
    internal SamusProjectile? Shot { get; private set; }

    /// <summary>Where the echo is shown, or none once no shot carries it.</summary>
    public Position16<RoomPixels>? ShownPosition =>
        Shot is { FrameBehavior: SamusProjectileFrameBehavior.ShinesparkEcho } ? _position : null;

    /// <summary>Whether <paramref name="shot"/> is the shot that carries this echo.</summary>
    /// <param name="shot">A shot running the echo's update.</param>
    internal bool IsCarriedBy(SamusProjectile shot) => ReferenceEquals(Shot, shot);

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
    internal void Forget()
    {
        Shot = null;
        _position = Position16<RoomPixels>.Zero;
    }
}
