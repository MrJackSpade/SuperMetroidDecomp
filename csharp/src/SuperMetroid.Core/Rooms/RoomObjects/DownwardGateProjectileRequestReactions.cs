namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What the owner of a <see cref="DownwardGateProjectileRequest"/> does on each of its cases (#2553).</summary>
/// <param name="DownwardGateProjectileSpawn">Reacts to a <see cref="DownwardGateProjectileSpawn"/>.</param>
/// <param name="DownwardGateProjectileWake">Reacts to a <see cref="DownwardGateProjectileWake"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed record DownwardGateProjectileRequestReactions<TContext>(Action<DownwardGateProjectileSpawn, TContext> DownwardGateProjectileSpawn, Action<DownwardGateProjectileWake, TContext> DownwardGateProjectileWake);
