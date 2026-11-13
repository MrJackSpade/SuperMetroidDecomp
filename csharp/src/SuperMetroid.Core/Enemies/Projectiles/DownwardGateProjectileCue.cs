using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>One cue of the downward shot gate's animations: what it does, with the values it acts on.</summary>
internal abstract record DownwardGateProjectileCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private DownwardGateProjectileCue()
    {
    }

    /// <summary>Installs a movement behaviour.</summary>
    /// <param name="FrameBehavior">The movement behaviour the cue installs.</param>
    internal sealed record SetFrameBehavior(EnemyProjectileFrameBehavior FrameBehavior) : DownwardGateProjectileCue;

    /// <summary>Sets the gate's signed 8.8 Y velocity.</summary>
    /// <param name="YVelocity">The vertical velocity a <see cref="SetYVelocity"/> cue installs.</param>
    internal sealed record SetYVelocity(SignedEightEight YVelocity) : DownwardGateProjectileCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(DownwardGateProjectileCueKind Kind) : DownwardGateProjectileCue;
}
