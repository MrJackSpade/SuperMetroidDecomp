using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Fx;

/// <summary>Quicksand reaction PLMs and the allocation-full check their spawns depend on.</summary>
internal sealed class QuicksandObjects
{
    /// <summary>Shared forty-slot PLM pool; occupied slots can prevent a quicksand reaction from spawning.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds quicksand reactions to the room's shared PLM allocation table.</summary>
    internal QuicksandObjects(RoomObjectSlotTable slotTable)
    {
        _slotTable = slotTable;
    }
    /// <summary>True when all forty PLM slots are occupied.</summary>
    public bool IsAllocationFull => _slotTable.IsFull;

    /// <summary>
    /// Allocates one transient sand-reaction PLM and leaves its setup effects to the
    /// collision owner, as spawning a PLM at the current block does.
    /// </summary>
    /// <remarks>
    /// Every sand reaction leaves the room on its first room-object update. X-Ray and G-Mode stop those updates, so
    /// repeated contact fills the room; subsequent contacts run no setup and sand falls back to air. The reactions' places
    /// are therefore observable gameplay state even though they never draw.
    /// </remarks>
    /// <returns>False only when the PLM pool is full.</returns>
    public bool TrySpawnQuicksandReaction(
        int blockIndex,
        RoomObjectHeaderId header)
    {
        QuicksandReactionDefinition definition =
            QuicksandReactions.ResolveReaction(header);

        if (_slotTable.SpawnAtCurrentBlock() is not { } slot)
            return false;

        slot.Header = header;
        slot.BlockIndex = blockIndex;
        slot.Become(new TerrainChangeObject(definition.Change));
        return true;
    }
}
