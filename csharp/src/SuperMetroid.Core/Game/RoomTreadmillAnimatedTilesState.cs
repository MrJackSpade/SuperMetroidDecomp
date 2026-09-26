using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Treadmill objects selected by LoadFxHeader's area animation bitset, independently
/// of the additional entrance object spawned by door ASM.
/// </summary>
public sealed class RoomTreadmillAnimatedTilesState
{
    private readonly List<WreckedShipTreadmillAnimatedTilesState> objects = [];

    /// <summary>Number of treadmill objects selected by the current room/door FX bitset.</summary>
    public int Count => objects.Count;

    /// <summary>Clears the previous population and follows the selected FX record's native bit order.</summary>
    public void LoadRoom(ISnesAddressSpace bus, ushort fxPointer, ushort doorPointer, AreaId area,
        bool useCompiledRecords = false)
    {
        objects.Clear();
        if (fxPointer == 0) return;
        var fxRecords = new RoomFxRecordReader(bus, useCompiledRecords);
        ushort record = fxRecords.Select(fxPointer, doorPointer);
        if (record == 0) return;
        byte bits = fxRecords.ReadByte(record, RoomFxRomData.Record.AnimatedTileBitsetOffset);
        for (int bit = 0; bit < 8; bit++)
        {
            if ((bits & (1 << bit)) == 0) continue;
            ushort definition = AreaAnimatedTileObjectDefinitions.Read(area, bit);
            WreckedShipTreadmillDirection? direction = definition switch
            {
                AnimatedTileObjectPointers.WreckedShipTreadmillRightwards => WreckedShipTreadmillDirection.Rightwards,
                AnimatedTileObjectPointers.WreckedShipTreadmillLeftwards => WreckedShipTreadmillDirection.Leftwards,
                _ => null,
            };
            // Other animation families retain their own owners. Selection here is by
            // native object identity, not room number or an assumed area-specific bit.
            if (direction is null) continue;
            var animation = new WreckedShipTreadmillAnimatedTilesState();
            animation.Start(bus, direction.Value);
            objects.Add(animation);
        }
    }

    /// <summary>Runs each selected native list; its boss wait gates graphics independently of floor collision.</summary>
    public void Step(ISnesAddressSpace bus, bool areaBossDefeated, VramWriteQueue writes)
    {
        foreach (var animation in objects) animation.Step(bus, areaBossDefeated, writes);
    }
}
