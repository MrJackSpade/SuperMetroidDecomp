using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>
/// What an eye door's shot keeps: the door whose opening stops it, the game state that records that door's opening, and
/// the direction it accelerates along.
/// </summary>
/// <param name="door">The eye's door; the shot bursts once that door has been opened.</param>
/// <param name="doors">The game state that records the opened doors, the shot's spawn's.</param>
internal sealed class EyeDoorShotState(LockedDoor door, SharedGameState doors) : EnemyProjectileState
{
    /// <summary>The eye's door; the shot bursts once that door has been opened.</summary>
    internal LockedDoor Door { get; } = Ensure.NotNull(door);

    /// <summary>The game state that records the opened doors, which the shot reads each update for its door.</summary>
    internal SharedGameState Doors { get; } = Ensure.NotNull(doors);

    /// <summary>The direction the shot accelerates along: the zero angle until its animation aims it at Samus.</summary>
    internal TrigonometryAngle Heading { get; set; }
}
