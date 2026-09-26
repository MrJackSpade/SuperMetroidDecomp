using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Sand-family portion of LoadFxHeader's bank-$87 object population. Selection comes
/// from the area table and selected FX record, never from a particular room number.
/// Other animated-tile families retain their existing independent owners.
/// </summary>
public sealed class RoomSandAnimatedTilesState
{
    private readonly List<RoomFxAnimatedTilesState> objects = [];

    /// <summary>Number of sand objects selected by the current room/door FX bitset.</summary>
    public int Count => objects.Count;

    /// <summary>Clears the previous room and spawns selected ceiling/falling-sand definitions in native order.</summary>
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
            if (definition is not (AnimatedTileObjectPointers.MaridiaSandCeiling or AnimatedTileObjectPointers.MaridiaSandFalling))
                continue;
            var animation = new RoomFxAnimatedTilesState();
            animation.LoadDefinition(bus, definition);
            objects.Add(animation);
        }
    }

    /// <summary>Advances the shared timed-list interpreter and queues the ROM-authored character frames for NMI.</summary>
    public void Step(ISnesAddressSpace bus, SnesVram vram, VramWriteQueue writes)
    {
        foreach (var animation in objects) animation.Step(bus, vram, writes);
    }
}
