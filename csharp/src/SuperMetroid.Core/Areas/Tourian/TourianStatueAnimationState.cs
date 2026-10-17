namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The Tourian statue room's shared animation state: which boss statues are still animating, whether one statue is
/// already playing its release, and whether all four have released. The cartridge sets and clears these bits
/// independently (OR and AND-NOT), so they compose.
/// </summary>
[Flags]
public enum TourianStatueAnimationState
{
    /// <summary>No statue is animating and no release has happened; the entrance statue's decorations are then finished.</summary>
    None = 0,

    /// <summary>Phantoon's statue is running its idle or release animation.</summary>
    PhantoonAnimating = 0x0001,

    /// <summary>Ridley's statue is running its idle or release animation.</summary>
    RidleyAnimating = 0x0002,

    /// <summary>Kraid's statue is running its idle or release animation.</summary>
    KraidAnimating = 0x0004,

    /// <summary>Draygon's statue is running its idle or release animation.</summary>
    DraygonAnimating = 0x0008,

    /// <summary>All four grey-statue events exist; latched by the descent before it waits for the last release to end.</summary>
    AllReleased = 0x0010,

    /// <summary>One statue is playing its release; the others keep idling until it finishes, so releases play one at a time.</summary>
    ReleaseInProgress = 0x8000,
}
