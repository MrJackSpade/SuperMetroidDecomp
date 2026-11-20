using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid's feeding as its victim asks about it (#1835, #2030): the Baby Metroid that feeds on a victim is the enemy the
/// room places directly before it (<see cref="BabyMetroidVictim"/>), and it has already fed when it started dormant,
/// hidden and out of reach. A victim placed first, or placed after an enemy that is not a Baby Metroid, has no Baby Metroid
/// feeding on it, so nothing has fed on it.
/// </summary>
/// <param name="enemies">The room's enemies, in placement order.</param>
[RoomLifetime]
internal sealed class BabyMetroidFeeding(EnemyPlaces enemies) : IBabyMetroidFeeding
{
    /// <inheritdoc/>
    public bool HasAlreadyFedOn(RoomEnemyPlace victim) =>
        BabyMetroidVictim.FeederOf(enemies.InUpdateOrder, Ensure.NotNull(victim).Ordinal)?.SpeciesState<BabyMetroidEnemyState>()
            is { Action: BabyMetroidAction.Dormant };
}
