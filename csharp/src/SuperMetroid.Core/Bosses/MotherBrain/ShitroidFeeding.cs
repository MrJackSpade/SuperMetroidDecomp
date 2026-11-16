using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Shitroid's feeding as its victim asks about it (#1835, #2030): the Shitroid that feeds on a victim is the enemy the
/// room places directly before it (<see cref="BabyMetroidVictim"/>), and it has already fed when it started dormant,
/// hidden and out of reach. A victim placed first, or placed after an enemy that is not a Shitroid, has no Shitroid
/// feeding on it, so nothing has fed on it.
/// </summary>
/// <param name="enemies">The room's enemies, in placement order.</param>
[RoomLifetime]
internal sealed class ShitroidFeeding(EnemySlotTable enemies) : IShitroidFeeding
{
    /// <inheritdoc/>
    public bool HasAlreadyFedOn(RoomEnemySlot victim) =>
        BabyMetroidVictim.FeederOf(enemies.Slots, Ensure.NotNull(victim).SlotIndex)?.SpeciesState<ShitroidEnemyState>()
            is { Action: ShitroidAction.Dormant };
}
