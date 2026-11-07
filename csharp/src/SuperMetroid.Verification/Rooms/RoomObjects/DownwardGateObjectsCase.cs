using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The trigger, projectile and accepted of one downward gate roomObjects case.</summary>
/// <param name="Trigger">The trigger.</param>
/// <param name="Projectile">The projectile.</param>
/// <param name="Accepted">The accepted.</param>
internal readonly record struct DownwardGateObjectsCase(DownwardGateTriggerBehavior Trigger, ushort Projectile, bool Accepted);
