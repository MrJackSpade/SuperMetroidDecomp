using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The room's Shitroid's feeding: it has already fed when it starts dormant, hidden and out of reach.</summary>
/// <param name="shitroid">The room's Shitroid.</param>
[RoomLifetime]
internal sealed class ShitroidFeeding(ShitroidBehavior shitroid) : IShitroidFeeding
{
    /// <inheritdoc/>
    public bool HasAlreadyFed => shitroid.Shitroid?.Action == ShitroidAction.Dormant;
}
