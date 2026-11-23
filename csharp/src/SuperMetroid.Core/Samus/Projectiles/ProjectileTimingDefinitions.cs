namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// How often Samus's shots leave trails and how long a shot keeps her from touching her own projectiles (#2111).
/// </summary>
internal static partial class ProjectileTimingDefinitions
{
    /// <summary>
    /// The updates before a trailing shot's first trail, and between its trails after that unless its flight sets its
    /// own: every fired beam, missile and special-attack part that trails starts with this wait.
    /// </summary>
    internal const int TrailInterval = 4;

    /// <summary>The updates between an uncharged Wave or Ice and Wave shot's trails after its first.</summary>
    internal const int UnchargedWaveTrailInterval = 3;

    /// <summary>The updates between a Super Missile's trails after its first: twice as often as a missile's.</summary>
    internal const int SuperMissileTrailInterval = 2;

    /// <summary>The updates a fired beam or Hyper Beam shot keeps Samus from interacting with her own projectiles, bomb jumps included.</summary>
    internal const int BeamInteractionImmunity = 10;

    /// <summary>The updates a fired missile or Super Missile keeps Samus from interacting with her own projectiles, bomb jumps included.</summary>
    internal const int MissileInteractionImmunity = 20;
}
