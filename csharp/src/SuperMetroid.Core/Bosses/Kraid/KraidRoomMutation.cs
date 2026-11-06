namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The ceiling, platform and spike changes Kraid's fight makes to his room: the only spawns Kraid's room spawner performs.
/// </summary>
public enum KraidRoomMutation
{
    /// <summary>Crumbles a ceiling block as Kraid rises, leaving background one.</summary>
    CrumbleCeilingIntoBackground1,
    /// <summary>Crumbles a platform block as Kraid sinks, leaving background two.</summary>
    CrumblePlatformVariant1,
    /// <summary>Crumbles a ceiling block as Kraid rises, leaving background two.</summary>
    CrumbleCeilingIntoBackground2,
    /// <summary>Crumbles a platform block as Kraid sinks, leaving background three.</summary>
    CrumblePlatformVariant2,
    /// <summary>Crumbles a ceiling block as Kraid rises, leaving background three.</summary>
    CrumbleCeilingIntoBackground3,
    /// <summary>Clears the ceiling of a room whose Kraid is already defeated.</summary>
    ClearCeiling,
    /// <summary>Clears the spikes of a room whose Kraid is already defeated.</summary>
    ClearSpikes,
    /// <summary>Crumbles the spikes on Kraid's death, two columns at a time.</summary>
    CrumbleSpikes,
}
