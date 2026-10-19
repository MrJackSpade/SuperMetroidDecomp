using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Sand-family portion of LoadFxHeader's bank-$87 object population. Selection comes
/// from the area table and selected FX record, never from a particular room number.
/// Other animated-tile families retain their existing independent owners.
/// </summary>
public sealed class RoomSandAnimatedTilesState
{
    /// <summary>Only sand-family objects selected by the current room's area and FX record.</summary>
    internal readonly List<RoomFxAnimatedTilesState> objects = [];

    /// <summary>Spawns one room's objects from the door-selected FX record in native bit order.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    /// <param name="area">The room's area, which selects each bit's object.</param>
    public RoomSandAnimatedTilesState(RoomFxDefinition? fx, CartridgeDoorHeader? door, AreaId area)
    {
        if (fx?.Select(door) is not { } record) return;
        byte bits = record.AnimatedTileBitset;
        for (int bit = 0; bit < 8; bit++)
        {
            if ((bits & (1 << bit)) == 0) continue;
            AnimatedTileObject definition = AreaAnimatedTileObjectDefinitions.Read(area, bit);
            if (definition is not (AnimatedTileObject.MaridiaSandCeiling or AnimatedTileObject.MaridiaSandFalling))
                continue;
            var animation = new RoomFxAnimatedTilesState();
            animation.LoadDefinition(definition);
            objects.Add(animation);
        }
    }

    /// <summary>Advances the shared timed-list interpreter and queues the ROM-authored character frames for NMI.</summary>
    public void Step(SnesVram vram, VramWriteQueue writes)
    {
        foreach (var animation in objects) animation.Step(vram, writes);
    }
}
