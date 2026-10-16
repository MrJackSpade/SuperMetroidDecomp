using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship attic's callback.</summary>
internal static class WreckedShipAtticPreInstructions
{
    /// <summary>
    /// <c>$84:BAFA</c> toggles the accumulator width to 8-bit and immediately back to 16-bit, then returns. Those
    /// processor-status writes leave no gameplay state.
    /// </summary>
    internal static PlmPreInstruction NoOpCallback { get; } = PlmPreInstruction.NoOp("Wrecked Ship attic callback", WreckedShipAtticPlmRomData.NoOpCallback);
}
