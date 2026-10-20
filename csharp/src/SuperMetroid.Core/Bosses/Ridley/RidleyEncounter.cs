using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley encounter state shared by the Ceres and Lower Norfair fights, and the native
/// <c>ceres_status</c> escape handoff word that Ceres Ridley publishes.
/// </summary>
internal sealed class RidleyEncounter
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
    /// Ridley's bank-$A6 state extension while either encounter owns slot zero. The native actor
    /// extends far beyond the common $40-byte enemy record, so exposing a deliberately named
    /// object is both more accurate and considerably easier to inspect than aliasing dozens
    /// of unrelated generic slot words.
    /// </summary>
    public RidleyEnemyState? Ridley { get; internal set; }

    /// <summary>
    /// Compatibility view used by the existing Ceres debugger. It deliberately becomes
    /// null for Lower Norfair Ridley so callers cannot accidentally apply Baby/Mode-7
    /// encounter assumptions to the real boss fight.
    /// </summary>
    public RidleyEnemyState? CeresRidley =>
        _slotTable.Slots[0].EnemyDefinitionPointer == EnemyDefinitionId.RidleyCeres ? Ridley : null;

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

    /// <summary>Requires a slot-zero Ridley definition and its shared bank-$A6 state extension.</summary>
    internal RidleyEnemyState RequireRidley(RoomEnemySlot slot)
    {
        if (!RidleyBehavior.IsRidleyDefinition(slot.EnemyDefinitionPointer) ||
            slot.SlotIndex != 0 ||
            Ridley is null)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed shared Ridley code without an " +
                "$E13F/$E17F slot-zero state extension.");
        }
        return Ridley;
    }

    /// <summary>Admits only Lower Norfair Ridley before returning the shared state extension.</summary>
    internal RidleyEnemyState RequireNorfairRidley(RoomEnemySlot slot)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Ridley)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed Lower Norfair Ridley code as " +
                $"definition ${(int)slot.EnemyDefinitionPointer:X4}.");
        }
        return RequireRidley(slot);
    }

    /// <summary>Admits only Ceres Ridley's slot-zero actor and initialized state extension.</summary>
    internal RidleyEnemyState RequireCeresRidley(RoomEnemySlot slot)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.RidleyCeres ||
            slot.SlotIndex != 0 ||
            Ridley is null)
        {
            throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} executed a Ceres Ridley routine without " +
                "the $E13F slot-zero state extension.");
        }

        return Ridley;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (Ridley?.LastDeathSoundEffect is { } ridleyDeath)
            audio.QueueEnemySound(ridleyDeath, maximumQueued: 3);
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(Ridley?.MusicRequest, MusicCommandDelay.EightFrames);
    }
}
