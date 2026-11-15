namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The ceiling, platform and spike changes Kraid's fight makes to his room: the only spawns Kraid's room spawner performs.
/// </summary>
public enum KraidRoomMutation
{
    /// <summary>Crumbles the ceiling's left-edge block as Kraid rises, leaving its background (#1895).</summary>
    CrumbleCeilingAtLeftEdge,
    /// <summary>Crumbles a platform block in an odd column as Kraid sinks, leaving that column's background.</summary>
    CrumblePlatformInOddColumn,
    /// <summary>Crumbles a ceiling block in an odd column as Kraid rises, leaving that column's background.</summary>
    CrumbleCeilingInOddColumn,
    /// <summary>Crumbles a platform block in an even column as Kraid sinks, leaving that column's background.</summary>
    CrumblePlatformInEvenColumn,
    /// <summary>Crumbles a ceiling block in an even column as Kraid rises, leaving that column's background.</summary>
    CrumbleCeilingInEvenColumn,
    /// <summary>Clears the ceiling of a room whose Kraid is already defeated.</summary>
    ClearCeiling,
    /// <summary>Clears the spikes of a room whose Kraid is already defeated.</summary>
    ClearSpikes,
    /// <summary>Crumbles the spikes on Kraid's death, two columns at a time.</summary>
    CrumbleSpikes,
}
