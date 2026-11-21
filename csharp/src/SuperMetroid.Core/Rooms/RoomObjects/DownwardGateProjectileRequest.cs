namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The projectile action a gate's script publishes to the enemy-projectile pool (#2483): a spawn of a gate projectile
/// (<see cref="DownwardGateProjectileSpawn"/>) or a wake of the one already spawned for the gate
/// (<see cref="DownwardGateProjectileWake"/>). Applying the request belongs to the runtime's projectile owner.
/// </summary>
public abstract partial record DownwardGateProjectileRequest
{
    /// <summary>Only the spawn and the wake derive from this.</summary>
    /// <param name="roomObjectBlock">The room block of the gate's top block, where its room object sits.</param>
    private protected DownwardGateProjectileRequest(Position16<RoomBlocks> roomObjectBlock) => RoomObjectBlock = roomObjectBlock;

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this downward gate projectile request, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(DownwardGateProjectileRequestReactions<TContext> reactions, TContext context);


    /// <summary>The room block of the gate's top block, where its room object sits.</summary>
    public Position16<RoomBlocks> RoomObjectBlock { get; }
}

/// <summary>What the owner of a <see cref="DownwardGateProjectileRequest"/> does on each of its cases (#2553).</summary>
/// <param name="DownwardGateProjectileSpawn">Reacts to a <see cref="DownwardGateProjectileSpawn"/>.</param>
/// <param name="DownwardGateProjectileWake">Reacts to a <see cref="DownwardGateProjectileWake"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed record DownwardGateProjectileRequestReactions<TContext>(Action<DownwardGateProjectileSpawn, TContext> DownwardGateProjectileSpawn, Action<DownwardGateProjectileWake, TContext> DownwardGateProjectileWake);
