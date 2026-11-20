namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The projectile action a gate's script publishes to the enemy-projectile pool (#2483): a spawn of a gate projectile
/// (<see cref="DownwardGateProjectileSpawn"/>) or a wake of the one already spawned for the gate
/// (<see cref="DownwardGateProjectileWake"/>). Applying the request belongs to the runtime's projectile owner.
/// </summary>
public abstract record DownwardGateProjectileRequest
{
    /// <summary>Only the spawn and the wake derive from this.</summary>
    /// <param name="roomObjectBlock">The room block of the gate's top block, where its room object sits.</param>
    private protected DownwardGateProjectileRequest(Position16<RoomBlocks> roomObjectBlock) => RoomObjectBlock = roomObjectBlock;

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this downward gate projectile request, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(DownwardGateProjectileRequestReactions reactions);


    /// <summary>The room block of the gate's top block, where its room object sits.</summary>
    public Position16<RoomBlocks> RoomObjectBlock { get; }
}

/// <summary>What the owner of a <see cref="DownwardGateProjectileRequest"/> does on each of its cases (#2553).</summary>
/// <param name="DownwardGateProjectileSpawn">Reacts to a <see cref="DownwardGateProjectileSpawn"/>.</param>
/// <param name="DownwardGateProjectileWake">Reacts to a <see cref="DownwardGateProjectileWake"/>.</param>
public sealed record DownwardGateProjectileRequestReactions(Action<DownwardGateProjectileSpawn> DownwardGateProjectileSpawn, Action<DownwardGateProjectileWake> DownwardGateProjectileWake);
