using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>One cue of the downward shot gate's animations: what it does, with the values it acts on.</summary>
internal abstract record DownwardGateProjectileCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private DownwardGateProjectileCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this downward gate projectile cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(DownwardGateProjectileCueReactions reactions);


    /// <summary>Sets the gate's vertical velocity.</summary>
    /// <param name="YVelocity">The vertical velocity a <see cref="SetYVelocity"/> cue installs.</param>
    internal sealed record SetYVelocity(SignedEightEight YVelocity) : DownwardGateProjectileCue
    {
        /// <inheritdoc/>
        internal override void React(DownwardGateProjectileCueReactions reactions) => Ensure.NotNull(reactions).SetYVelocity(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(DownwardGateProjectileCueKind Kind) : DownwardGateProjectileCue
    {
        /// <inheritdoc/>
        internal override void React(DownwardGateProjectileCueReactions reactions) => Ensure.NotNull(reactions).Signal(this);
    }
}

/// <summary>What the owner of a <see cref="DownwardGateProjectileCue"/> does on each of its cases (#2553).</summary>
/// <param name="SetYVelocity">Reacts to a <see cref="DownwardGateProjectileCue.SetYVelocity"/>.</param>
/// <param name="Signal">Reacts to a <see cref="DownwardGateProjectileCue.Signal"/>.</param>
internal sealed record DownwardGateProjectileCueReactions(Action<DownwardGateProjectileCue.SetYVelocity> SetYVelocity, Action<DownwardGateProjectileCue.Signal> Signal);
