namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How many frames the frame counter advances while a door's new music loads. The original program loaded music by
/// stalling its main loop; frames kept being counted and the controller kept being read during the stall, although no
/// gameplay update ran. Those counts are gameplay state: their parity selects, for example, the order in which a bomb's
/// explosion scans the blocks around it. The game itself loads instantly (<see cref="InstantDoorMusicLoad"/>); a replay
/// supplies the counts its recording shows.
/// </summary>
/// <remarks>
/// Consulted once per door transition, when the transition starts the destination's music set. Evidence: the music
/// upload wait (<c>$80:8028</c>) in the SMV recordings' input-consumption traces.
/// </remarks>
[CompatibilityService("frames counted while a door's new music loads")]
public interface IDoorMusicLoadFrames
{
    /// <summary>The frames counted while music set <paramref name="musicSet"/> loads.</summary>
    /// <param name="musicSet">The music set the destination room selects.</param>
    /// <returns>A nonnegative count added to the frame counter, without running gameplay updates.</returns>
    int FramesCountedDuringMusicLoad(int musicSet);
}
