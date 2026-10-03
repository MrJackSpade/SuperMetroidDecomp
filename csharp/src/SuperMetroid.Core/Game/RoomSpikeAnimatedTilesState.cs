using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Horizontal spikes selected by the room FX animation bitset, including Crocomire's left wall.</summary>
public sealed class RoomSpikeAnimatedTilesState
{
    private readonly RoomFxAnimatedTilesState animation = new();

    /// <summary>Replaces the previous room's animation using the selected door-specific FX record.</summary>
    public void LoadRoom(ISnesAddressSpace bus, ushort fxPointer, ushort doorPointer, AreaId area)
    {
        animation.Reset();
        if (fxPointer == 0) return;
        ushort record = RoomFxRecordDefinitions.Select(fxPointer, doorPointer);
        if (record == 0) return;
        byte bits = RoomFxRecordDefinitions.Get(record).AnimatedTileBitset;
        for (int bit = 0; bit < 8; bit++)
            if ((bits & (1 << bit)) != 0 &&
                AreaAnimatedTileObjectDefinitions.Read(area, bit) == AnimatedTileObjectPointers.HorizontalSpikes)
            {
                animation.LoadDefinition(bus, AnimatedTileObjectPointers.HorizontalSpikes);
                return;
            }
    }

    /// <summary>Schedules the authored character transfer for the next accepted NMI.</summary>
    public void Step(ISnesAddressSpace bus, SnesVram vram, VramWriteQueue writes) =>
        animation.Step(bus, vram, writes);
}
