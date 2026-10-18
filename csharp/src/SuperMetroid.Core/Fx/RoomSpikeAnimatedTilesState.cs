using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>Horizontal spikes selected by the room FX animation bitset, including Crocomire's left wall.</summary>
public sealed class RoomSpikeAnimatedTilesState
{
    /// <summary>Current room's horizontal-spike tile transfer state, reset whenever a new FX record is selected.</summary>
    private readonly RoomFxAnimatedTilesState animation = new();

    /// <summary>Replaces the previous room's animation using the selected door-specific FX record.</summary>
    public void LoadRoom(ushort fxPointer, CartridgeDoorHeader? door, AreaId area)
    {
        animation.Reset();
        if (fxPointer == 0) return;
        ushort record = RoomFxRecordDefinitions.Select(fxPointer, door);
        if (record == 0) return;
        byte bits = RoomFxRecordDefinitions.Get(record).AnimatedTileBitset;
        for (int bit = 0; bit < 8; bit++)
            if ((bits & (1 << bit)) != 0 &&
                AreaAnimatedTileObjectDefinitions.Read(area, bit) == AnimatedTileObject.HorizontalSpikes)
            {
                animation.LoadDefinition(AnimatedTileObject.HorizontalSpikes);
                return;
            }
    }

    /// <summary>Schedules the authored character transfer for the next accepted NMI.</summary>
    public void Step(SnesVram vram, VramWriteQueue writes) =>
        animation.Step(vram, writes);
}
