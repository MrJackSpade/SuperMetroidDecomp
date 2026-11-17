namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Reads the Power Bomb blast's radius speed, which only its running stages keep (#2350).</summary>
internal static class PowerBombBlastTimelineAccess
{
    /// <summary>Reads the blast's stage state.</summary>
    extension(PowerBombBlastTimeline self)
    {
        /// <summary>The running stage's per-update radius change.</summary>
        internal PowerBombRadius RadiusSpeed => PrivateState.Field<PowerBombBlastTimeline, PowerBombRadius>(self, "_radiusSpeed");
    }
}
