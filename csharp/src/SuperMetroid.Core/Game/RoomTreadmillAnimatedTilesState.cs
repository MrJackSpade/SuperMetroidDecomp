using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Treadmill objects selected by LoadFxHeader's area animation bitset, independently
/// of the additional entrance object spawned by door ASM.
/// </summary>
public sealed class RoomTreadmillAnimatedTilesState
{
    private readonly List<WreckedShipTreadmillAnimatedTilesState> objects = [];

    /// <summary>Clears the previous population and follows the selected FX record's native bit order.</summary>
    public void LoadRoom(ISnesAddressSpace bus, ushort fxPointer, ushort doorPointer, AreaId area)
    {
        objects.Clear();
        if (fxPointer == 0) return;
        ushort record = RoomFxRomData.SelectRecord(bus, fxPointer, doorPointer);
        if (record == 0) return;
        byte bits = RoomFxRomData.ReadRecordByte(bus, record, RoomFxRomData.Record.AnimatedTileBitsetOffset);
        ushort list = RomDataReader.ReadWordFixedBank(bus,
            RoomFxRomData.Tables.AreaAnimatedTileObjectListPointers + AreaIds.ToIndex(area) * 2);
        for (int bit = 0; bit < 8; bit++)
        {
            if ((bits & (1 << bit)) == 0) continue;
            ushort definition = RomDataReader.ReadWordFixedBank(bus,
                RoomFxRomData.Banks.RoomDefinitions | unchecked((ushort)(list + bit * 2)));
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
