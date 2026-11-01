using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The Lower Norfair Chozo hand's wait.</summary>
internal static class ChozoStatueFrameBehaviors
{
    /// <summary>Waits for the hand's trigger block to clear; the hand's animation installs it.</summary>
    internal static PlmFrameBehavior WaitForLowerNorfairHand { get; } = new(
        "Lower Norfair Chozo hand wait",
        static (slot, context) => ChozoStatuePlms.RunChozoStatueFrameBehavior(context.Level, slot));
}
