using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley encounter state shared by the Ceres and Lower Norfair fights, and the
/// <c>ceres_status</c> escape handoff value that Ceres Ridley publishes.
/// </summary>
internal sealed class RidleyEncounter : ICeresEscapeStatus
{
    /// <summary>Physical slot table used to distinguish the Ceres actor from the Norfair boss.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Binds both Ridley encounters to their shared enemy slot table.</summary>
    /// <param name="slotTable">The enemy slots, whose slot zero distinguishes Ceres Ridley.</param>
    /// <param name="session">The session that holds Ceres's escape progress across rooms.</param>
    internal RidleyEncounter(EnemySlotTable slotTable, EnemySession session)
    {
        _slotTable = slotTable;
        _session = Ensure.NotNull(session);
    }

    /// <summary>The session that holds Ceres's escape progress across rooms.</summary>
    private readonly EnemySession _session;

    /// <summary>Ridley's body, which both encounters keep in slot zero.</summary>
    internal RoomEnemySlot Body => _slotTable.Slots[0];

    /// <summary>
    /// Ridley's state beyond the common enemy record while either encounter owns slot zero,
    /// kept in named fields rather than in dozens of generic enemy variables.
    /// </summary>
    public RidleyEnemyState? Ridley { get; internal set; }

    /// <summary>
    /// Ridley's state while the Ceres getaway owns slot zero, and null during the Lower
    /// Norfair fight, which has neither the Baby nor the rotating Ceres floor.
    /// </summary>
    public RidleyEnemyState? CeresRidley =>
        _slotTable.Slots[0].DefinitionId == EnemyDefinitionId.RidleyCeres ? Ridley : null;

    /// <summary>
    /// How far Ceres's escape has progressed, consumed by the Ceres doors and the elevator shaft. A fresh station load
    /// begins before Ridley escapes; Ceres Ridley publishes his escape and the self-destruct, and the elevator door's
    /// destruction publishes the last stage.
    /// </summary>
    public CeresEscapeStatus CeresStatus
    {
        get => _session.CeresStatus;
        set => _session.CeresStatus = value;
    }

    /// <summary>Requires a slot-zero Ridley definition and its shared state extension.</summary>
    internal RidleyEnemyState RequireRidley(RoomEnemySlot slot)
    {
        if (!RidleyInteractions.IsRidleyDefinition(slot.DefinitionId) ||
            slot.SlotIndex != 0 ||
            Ridley is null)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed shared Ridley code without an " +
                "extended Ridley state in the first enemy slot.");
        }
        return Ridley;
    }

    /// <summary>Admits only Lower Norfair Ridley before returning the shared state extension.</summary>
    internal RidleyEnemyState RequireNorfairRidley(RoomEnemySlot slot)
    {
        if (slot.DefinitionId != EnemyDefinitionId.Ridley)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed Lower Norfair Ridley code as " +
                $"definition {slot.DefinitionId}.");
        }
        return RequireRidley(slot);
    }

    /// <summary>Admits only Ceres Ridley's slot-zero actor and initialized state extension.</summary>
    internal RidleyEnemyState RequireCeresRidley(RoomEnemySlot slot)
    {
        if (slot.DefinitionId != EnemyDefinitionId.RidleyCeres ||
            slot.SlotIndex != 0 ||
            Ridley is null)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed a Ceres Ridley routine without " +
                "the extended Ceres Ridley state in the first enemy slot.");
        }

        return Ridley;
    }
}
