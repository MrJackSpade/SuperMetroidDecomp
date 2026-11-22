using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The published artwork name of each fake-death draw (<see cref="MotherBrainFakeDeathDraw"/>, #1994, #2548): the name the installed
/// room-object visual file keys its picture by, written out so renaming a member never changes it.
/// </summary>
internal static partial class MotherBrainFakeDeathDrawKeys
{
    /// <summary>Each MotherBrainFakeDeathDraw member's installed key.</summary>
    internal static InstalledKeys<MotherBrainFakeDeathDraw> Installed { get; } = new(new Dictionary<MotherBrainFakeDeathDraw, string>
    {
        [MotherBrainFakeDeathDraw.FillWall] = "fill-wall",
        [MotherBrainFakeDeathDraw.EscapeDoor] = "escape-door",
        [MotherBrainFakeDeathDraw.ClearCeilingBlock] = "clear-ceiling-block",
        [MotherBrainFakeDeathDraw.ClearCeilingTube] = "clear-ceiling-tube",
        [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] = "clear-bottom-middle-side-tube",
        [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] = "clear-bottom-middle-tubes",
        [MotherBrainFakeDeathDraw.ClearBottomLeftTube] = "clear-bottom-left-tube",
        [MotherBrainFakeDeathDraw.ClearBottomRightTube] = "clear-bottom-right-tube",
    });
}
