namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The Tourian statue room's shared animation state (#1549): which boss statues are still animating, whether one statue
/// is playing its release, and whether all four have released. The statue animations and the descent set and clear
/// each part independently.
/// </summary>
public sealed class TourianStatueAnimation
{
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
