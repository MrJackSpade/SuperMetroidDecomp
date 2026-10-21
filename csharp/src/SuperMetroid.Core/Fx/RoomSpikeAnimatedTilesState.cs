using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Fx;

/// <summary>Horizontal spikes selected by the room FX animation bitset, including Crocomire's left wall.</summary>
[RoomLifetime]
public sealed class RoomSpikeAnimatedTilesState
{
    /// <summary>Current room's horizontal-spike tile transfer state, reset whenever a new FX record is selected.</summary>
    private readonly RoomFxAnimatedTilesState animation = new();

    /// <summary>Spawns one room's objects from the door-selected FX record in native bit order.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    /// <param name="area">The room's area, which selects each bit's object.</param>
    public RoomSpikeAnimatedTilesState(RoomFxDefinition? fx, CartridgeDoorHeader? door, AreaId area)
    {
        if (fx?.Select(door) is not { } record) return;
        byte bits = record.AnimatedTileBitset;
        for (int bit = 0; bit < 8; bit++)
            if ((bits & (1 << bit)) != 0 &&
                AreaAnimatedTileObjectDefinitions.Read(area, bit) == AnimatedTileObject.HorizontalSpikes)
            {
                animation.LoadDefinition(AnimatedTileObject.HorizontalSpikes);
                return;
            }
    }

    /// <summary>Schedules the authored character transfer for the next accepted NMI.</summary>
    public void Step(VramWriteQueue writes) =>
        animation.Step(null, writes);
}
