using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The trigger, projectile and accepted of one downward gate plms case.</summary>
/// <param name="Trigger">The trigger.</param>
/// <param name="Projectile">The projectile.</param>
/// <param name="Accepted">The accepted.</param>
internal readonly record struct DownwardGatePlmsCase(DownwardGateTriggerBehavior Trigger, ushort Projectile, bool Accepted);
