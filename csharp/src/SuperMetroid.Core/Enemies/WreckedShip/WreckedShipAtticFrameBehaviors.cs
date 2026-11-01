using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship attic's callback.</summary>
internal static class WreckedShipAtticFrameBehaviors
{
    /// <summary>
    /// The attic's frame behaviour switches its arithmetic width and immediately back, then returns. Those
    /// switches leave no gameplay state.
    /// </summary>
    internal static PlmFrameBehavior NoOpCallback { get; } = PlmFrameBehavior.NoOp("Wrecked Ship attic callback");
}
