namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The suit's shade a cancelled speed boost or a new pose shows (#2562), which shows at the update's colour step, before the
/// charge and drained colourings of the same update, so they show over it.
/// </summary>
/// <remarks>
/// Evidence: the game writes the suit's palette at once where it cancels the boost's momentum and where a pose initializes,
/// before its charge and special colouring handling of the same update replace it. Samus's colours are the colour step's,
/// not the movement's, the pose handling's or the firing's, so the shade waits here for the colour step, which shows it
/// first.
/// Lifetime: Samus's.
/// </remarks>
[CompatibilityService("the suit's shade a cancelled boost or a new pose shows waits for the update's colour step")]
public sealed partial class SuitShadeAtColourStep
{
    /// <summary>Whether the suit's shade waits for the colour step.</summary>
    internal bool IsDue { get; set; }

    /// <summary>A cancelled boost or a new pose shows the suit's shade at the colour step.</summary>
    internal void Show() => IsDue = true;

    /// <summary>The colour step takes the shade: whether one was due, which it then shows.</summary>
    internal bool Take()
    {
        bool due = IsDue;
        IsDue = false;
        return due;
    }
}
