using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>What a Kago bug keeps while it hops about: the shell it came from and its two countdowns.</summary>
/// <param name="source">The Kago shell that released the bug.</param>
internal sealed class KagoBugState(RoomEnemySlot source) : EnemyProjectileState
{
    /// <summary>The Kago shell that released the bug: the bug may be shot once it is far enough from it, and hops home to it.</summary>
    internal RoomEnemySlot Source { get; } = source;

    /// <summary>Updates until the bug's sound plays; it stays at zero once it has played.</summary>
    internal int SoundDelay
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>Updates the bug waits on the ground before its next hop; it hops on the update after this reaches zero.</summary>
    internal int IdleFrames
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }
}
