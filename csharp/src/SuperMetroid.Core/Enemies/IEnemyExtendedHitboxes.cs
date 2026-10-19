namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy whose extended-frame collision geometry is engine-owned rather than read from installed frames.</summary>
internal interface IEnemyExtendedHitboxes : IEnemyEntity
{
    /// <summary>
    /// The callback of the first of the occupant's hitboxes the probe overlaps: true with the callback, false when none
    /// overlaps, or null when the species' geometry does not cover the current frame.
    /// </summary>
    /// <param name="probe">The colliding edges and walker.</param>
    /// <param name="callback">The selected touch or shot callback pointer.</param>
    bool? FindHitboxCallback(EnemyHitboxProbe probe, out EnemyInteractionCallback? callback);
}
