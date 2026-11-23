using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>What a Kago bug keeps while it hops about: the shell it came from and its two countdowns.</summary>
/// <param name="source">The Kago shell that released the bug.</param>
internal sealed partial class KagoBugState(RoomEnemyPlace source) : EnemyProjectileState
{
    /// <summary>The Kago shell that released the bug: the bug may be shot once it is far enough from it, and hops home to it.</summary>
    internal RoomEnemyPlace Source { get; } = source;

    /// <summary>The wait for the bug's sound, which plays on the update it ends; it stays over once the sound has played.</summary>
    internal UpdateCountdown SoundDelay { get; set; } = UpdateCountdown.Ended();

    /// <summary>The bug's wait on the ground, which ends on the update it hops.</summary>
    internal UpdateCountdown GroundWait { get; set; } = UpdateCountdown.Ended();
}
