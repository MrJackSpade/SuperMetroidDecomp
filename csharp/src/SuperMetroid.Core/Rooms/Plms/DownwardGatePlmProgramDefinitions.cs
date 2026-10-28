namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The downward gate's PLM programs (#1296), in place of its bank-$84 instruction lists. The resident gate alternates
/// between an open and a closed sleep; a shot (or, when closed, Samus below it) wakes it past the sleep into the
/// lowering or raising images while its bank-$86 projectile carries the collision. Each trigger draws its shot block
/// once and deletes.
/// </summary>
internal static class DownwardGatePlmProgramDefinitions
{
    /// <summary>How long the open gate shows before it starts lowering.</summary>
    internal const int CloseDelay = 16;
    /// <summary>How long each lowering or raising image shows.</summary>
    internal const int MovementFrameDuration = 16;
    /// <summary>How long the last lowering or raising image shows.</summary>
    internal const int LastMovementFrameDuration = 24;
    /// <summary>The movement sound's native queue cap.</summary>
    internal static readonly SoundQueueLimit MovementSoundMaximumQueued = SoundQueueLimit.Max6;
}
