namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's eye fade (#2247): his eyes show sixteen shades, one an update from the darkest to his own eye colours, then
/// hold his own eye colours before his body fade begins. The colours are the import's.
/// </summary>
internal static class RidleyEyeFadeDefinitions
{
    /// <summary>The eye shades the fade shows, fifteen (darkest) down to zero (his own eye colours).</summary>
    public const int ShadeCount = 16;

    /// <summary>The updates the fade holds his own eye colours after its last shade.</summary>
    public const int HoldUpdates = 48;
}
