namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The Tourian statue room's shared animation state (#1549): which boss statues are still animating, whether one statue
/// is playing its release, whether all four have released and whether the entrance statue's sinking has finished. The
/// statue animations, the descent and the entrance statue's decorations set and clear each part independently; how far
/// the statue has sunk is the descent's own (<see cref="ITourianStatueRoom.StatueOffset"/>, #2033). The statue room holds it
/// (<see cref="ITourianStatueRoom.Animation"/>, #2486).
/// </summary>
public sealed class TourianStatueAnimation
{
    /// <summary>
    /// Whether the entrance statue has finished sinking. The statue's base decoration sets it once the sinking animation
    /// has ended.
    /// </summary>
    public bool Finished { get; internal set; }

    /// <summary>The statues running their idle or release animation.</summary>
    public TourianGreyStatues Animating { get; internal set; }

    /// <summary>
    /// Whether one statue is playing its release; the others keep idling until it finishes, so releases play one at a
    /// time.
    /// </summary>
    public bool ReleaseInProgress { get; internal set; }

    /// <summary>Whether all four grey-statue events exist, latched by the descent before it waits for the last release.</summary>
    public bool AllReleased { get; internal set; }

    /// <summary>
    /// Whether no statue is animating, none is releasing and the descent has latched nothing: the room-load state, in
    /// which the entrance statue's decorations are finished.
    /// </summary>
    public bool AtRest => Animating == TourianGreyStatues.None && !ReleaseInProgress && !AllReleased;
}
