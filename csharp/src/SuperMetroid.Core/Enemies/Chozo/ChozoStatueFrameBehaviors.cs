using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The Lower Norfair Chozo hand's wait.</summary>
internal static class ChozoStatueFrameBehaviors
{
    /// <summary><c>$84:D15C</c>: waits for the hand's trigger block to clear; the hand's list installs it through <c>$84:86C1</c>.</summary>
    internal static PlmFrameBehavior WaitForLowerNorfairHand { get; } = new(
        "Lower Norfair Chozo hand wait",
        static (slot, context) => ChozoStatuePlms.RunChozoStatueFrameBehavior(context.Level, slot));
}
