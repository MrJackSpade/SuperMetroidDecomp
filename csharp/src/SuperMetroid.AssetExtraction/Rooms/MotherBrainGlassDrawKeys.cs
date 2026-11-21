using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each Mother Brain glass frame (<see cref="MotherBrainGlassDraw"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static partial class MotherBrainGlassDrawKeys
{
    /// <summary>Each MotherBrainGlassDraw member's installed key.</summary>
    internal static InstalledKeys<MotherBrainGlassDraw> Installed { get; } = new(new Dictionary<MotherBrainGlassDraw, string>
    {
        [MotherBrainGlassDraw.Intact] = "initial",
        [MotherBrainGlassDraw.PaneCracked] = "pane-cracked",
        [MotherBrainGlassDraw.PaneCrackedThrough] = "pane-cracked-through",
        [MotherBrainGlassDraw.PaneTransition] = "pane-transition",
        [MotherBrainGlassDraw.PaneShifted] = "pane-shifted",
        [MotherBrainGlassDraw.PaneShiftedCrumbling] = "pane-shifted-crumbling",
        [MotherBrainGlassDraw.PaneShiftedEmptying] = "pane-shifted-emptying",
        [MotherBrainGlassDraw.RimBuckling] = "rim-buckling",
        [MotherBrainGlassDraw.RimBursting] = "rim-bursting",
        [MotherBrainGlassDraw.RimGone] = "rim-gone",
        [MotherBrainGlassDraw.Cleared] = "cleared",
    });
}
