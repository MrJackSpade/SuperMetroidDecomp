using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The Lower Norfair Chozo hand's wait.</summary>
internal static class ChozoStatuePreInstructions
{
    /// <summary><c>$84:D15C</c>: waits for the hand's trigger block to clear; the hand's list installs it through <c>$84:86C1</c>.</summary>
    internal static PlmPreInstruction WaitForLowerNorfairHand { get; } = new(
        "Lower Norfair Chozo hand wait", ChozoStatuePlmRomData.WaitForLowerNorfairHand,
        static (slot, context) => ChozoStatuePlms.RunChozoStatuePreInstruction(context.Level, slot));
}
