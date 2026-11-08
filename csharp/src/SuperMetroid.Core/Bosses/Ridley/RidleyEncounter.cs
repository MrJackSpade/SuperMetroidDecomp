using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley encounter state shared by the Ceres and Lower Norfair fights, and the
/// <c>ceres_status</c> escape handoff value that Ceres Ridley publishes.
/// </summary>
[RoomLifetime]
internal sealed class RidleyEncounter : ICeresEscapeStatus
{
    /// <summary>Binds both Ridley encounters to the room's enemies, among which <see cref="Parts"/> names his.</summary>
    /// <param name="slotTable">The room's enemies.</param>
    /// <param name="session">The session that holds Ceres's escape progress across rooms.</param>
    internal RidleyEncounter(EnemySlotTable slotTable, EnemySession session)
    {
        Parts = new RidleyParts(slotTable);
        _session = Ensure.NotNull(session);
    }

    /// <summary>The session that holds Ceres's escape progress across rooms.</summary>
    private readonly EnemySession _session;

    /// <summary>The enemies of Ridley's rooms his encounters reach by name: his body and Ceres's door.</summary>
    internal RidleyParts Parts { get; }

    /// <summary>
    /// Ridley's state beyond the common enemy record while either encounter owns his body,
    /// kept in named fields rather than in dozens of generic enemy variables.
    /// </summary>
    public RidleyEnemyState? Ridley { get; internal set; }

    /// <summary>
    /// Ridley's state while his body is Ceres Ridley, and null during the Lower
    /// Norfair fight, which has neither the Baby nor the rotating Ceres floor.
    /// </summary>
    public RidleyEnemyState? CeresRidley =>
        Parts.Body.DefinitionId == EnemyDefinitionId.RidleyCeres ? Ridley : null;

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

    /// <summary>Requires Ridley's body as a Ridley definition and its shared state extension.</summary>
    internal RidleyEnemyState RequireRidley(RoomEnemySlot slot)
    {
        if (!RidleyInteractions.IsRidleyDefinition(slot.DefinitionId) ||
            !Parts.IsBody(slot) ||
            Ridley is null)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed shared Ridley code without being Ridley's body " +
                "with an extended Ridley state.");
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

    /// <summary>Admits only Ceres Ridley's body and initialized state extension.</summary>
    internal RidleyEnemyState RequireCeresRidley(RoomEnemySlot slot)
    {
        if (slot.DefinitionId != EnemyDefinitionId.RidleyCeres ||
            !Parts.IsBody(slot) ||
            Ridley is null)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed a Ceres Ridley routine without being " +
                "Ceres Ridley's body with the extended Ceres Ridley state.");
        }

        return Ridley;
    }
}
