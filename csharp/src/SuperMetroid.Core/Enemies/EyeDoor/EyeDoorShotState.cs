using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>What an eye door's shot keeps: the door whose opening stops it, and the direction it accelerates along.</summary>
/// <param name="doorBit">The opened-door flag of the eye's door; the shot bursts once that door has been opened.</param>
internal sealed class EyeDoorShotState(int doorBit) : EnemyProjectileState
{
    /// <summary>The opened-door flag of the eye's door; the shot bursts once that door has been opened.</summary>
    internal int DoorBit { get; } = doorBit;

    /// <summary>The direction the shot accelerates along: the zero angle until its program aims it at Samus.</summary>
    internal ByteAngle Heading { get; set; }
}
