using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>One cue of the downward shot gate's animations: what it does, with the values it acts on.</summary>
internal abstract partial record DownwardGateProjectileCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private DownwardGateProjectileCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this downward gate projectile cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(DownwardGateProjectileCueReactions<TContext> reactions, TContext context);


    /// <summary>Sets the gate's vertical velocity.</summary>
    /// <param name="YVelocity">The vertical velocity a <see cref="SetYVelocity"/> cue installs.</param>
    internal sealed partial record SetYVelocity(SignedEightEight YVelocity) : DownwardGateProjectileCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DownwardGateProjectileCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SetYVelocity(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(DownwardGateProjectileCueKind Kind) : DownwardGateProjectileCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DownwardGateProjectileCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}
