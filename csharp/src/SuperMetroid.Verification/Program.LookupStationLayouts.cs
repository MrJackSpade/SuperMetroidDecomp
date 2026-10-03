using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyStationLayoutGeometry(SuperMetroidAddressSpace rom) => VerifyStationLayoutDrawField(rom, 0);
    private static void VerifyStationLayoutCollision(SuperMetroidAddressSpace rom) => VerifyStationLayoutDrawField(rom, 1);
    private static void VerifyStationLayoutVisuals(SuperMetroidAddressSpace rom) => VerifyStationLayoutDrawField(rom, 2);

    private static void VerifyStationLayoutDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0x9f25,0x9f6d,0x9f91,0x9f31,0x9f79,0x9f9d,0x9f3d,0x9f85,0x9fa9,0x9a3f,0x9a9f,0x9a6f,0x9f49,0x9f55,0x9f5b,0x9f67,0x9fb5,0x9fbb,0x9fc1,0x9fc7];
        string[] ids = ["map-frame-0","energy-frame-0","missile-frame-0","map-frame-1","energy-frame-1","missile-frame-1","map-frame-2","energy-frame-2","missile-frame-2","save-idle","save-active-a","save-active-b","map-right-retracted","map-right-extended","map-left-retracted","map-left-extended","resource-right-retracted","resource-right-extended","resource-left-retracted","resource-left-extended"];
        var exported = RoomPlmStationDrawDefinitions.All.ToArray();
        AssertEqual(20, exported.Length, "Station layout export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, RoomPlmStationDrawDefinitions.TryDescribe(pointer, out var shape), "Station layout shape domain");
                AssertEqual(owned, RoomPlmStationDrawDefinitions.TryGet(pointer, out var dto), "Station layout DTO domain");
                if (!owned)
                {
                    AssertEqual(default(RoomPlmStationDrawDefinitions.Draw), shape, "Station layout missing shape");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Station layout missing DTO");
                    AssertThrows<InvalidDataException>(() => RoomPlmStationDrawDefinitions.VisualId(pointer), "Station layout missing ID");
                }
            }
            foreach (string id in new[] { "MAP-FRAME-0", "broken", "", "unknown" })
            {
                AssertTrue(!RoomPlmStationDrawDefinitions.TryGetByVisualId(id, out var missing), "Station layout ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Station layout rejected ID output");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            RoomPlmStationDrawDefinitions.TryDescribe(pointer, out var shape);
            RoomPlmStationDrawDefinitions.TryGet(pointer, out var dto);
            RoomPlmStationDrawDefinitions.TryGetByVisualId(ids[index], out var byId);
            if (field == 0)
            {
                AssertEqual(pointer, exported[index].Pointer, "Station layout original export order");
                AssertEqual(ids[index], RoomPlmStationDrawDefinitions.VisualId(pointer), "Station layout published ID");
                AssertEqual(pointer, byId.Pointer, "Station layout reverse ID");
                foreach (int invalid in new[] {int.MinValue,-1,shape.RunCount,int.MaxValue})
                {
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordCount(invalid), "Station layout run bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.NextX(invalid), "Station layout X bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.NextY(invalid), "Station layout offset bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid, 0), "Station layout word run bounds");
                }
            }
            var level = CreateRoom(32, 16, new ushort[512], new byte[512], blockDefinitions: new byte[0x400 * 8]);
            var plms = new RoomPlmSystem();
            var bus = new TestAddressSpace();
            if (field == 0)
            {
                bus.WriteBytes(0x8f9400, [0x0b,0xb7,8,8,0,0,0,0]);
                WriteWord(bus, 0x84b70d, 0xafb6);
                AssertEqual(1, plms.LoadRoomPopulation(bus, level, level.CreateBackgroundStreamer(),
                    new SnesVram(), SuperMetroid.AssetExtraction.RoomPlmPopulationImporter.Read(bus, 0x9400),
                    new Bank80SystemState(), AreaId.Crateria, () => null, () => false), "Elevator platform fixture spawn");
                plms.SetSoleInstructionPointerForVerification(0xf100, [1, pointer]);
                plms.Step(bus, level, level.CreateBackgroundStreamer(), 0, 0, 0);
            }
            int drawX = 8, drawY = 8;
            int cursor = 0x840000 | pointer;
            int run = 0;
            while (true)
            {
                ushort header = ReadSamusEaterPlmWord(rom, cursor);
                int count = header & 0x7fff;
                ushort offset = ReadSamusEaterPlmWord(rom, cursor + 2 + count * 2);
                if (field == 0)
                {
                    AssertEqual((ushort)count, header, "Station layout native direction");
                    AssertEqual(count, shape.WordCount(run), "Station layout native calculated count");
                    AssertEqual((ushort)((byte)shape.NextX(run) | (byte)shape.NextY(run) << 8), offset, "Station layout native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],byId})
                    {
                        var part = frame.Runs.Span[run];
                        AssertEqual(header, part.DirectionAndCount, "Station layout native DTO geometry");
                        AssertEqual(count, part.LevelWords.Length, "Station layout native DTO count");
                        AssertEqual(offset, (ushort)((byte)part.NextX | (byte)part.NextY << 8), "Station layout native DTO offset");
                    }
                    for (int cell = 0; cell < count; cell++)
                        AssertEqual(ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2),
                            level.GetCollisionBlock(drawX + cell, drawY).LevelWord,
                            "Station native physical word at original absolute run position");
                    drawX = 8 + unchecked((sbyte)offset);
                    drawY = 8 + unchecked((sbyte)(offset >> 8));
                    int selectedRun = run;
                    foreach (int invalid in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(selectedRun, invalid), "Station layout cell bounds");
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    int mask = field == 1 ? 0xf000 : 0xfff;
                    int expected = ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2) & mask;
                    AssertEqual(expected, shape.WordAt(run, cell) & mask, "Station layout original calculated field");
                    foreach (var frame in new[] {dto,exported[index],byId})
                        AssertEqual(expected, frame.Runs.Span[run].LevelWords.Span[cell] & mask, "Station layout original DTO field");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 7, "Station layout native run termination");
            }
            AssertEqual(run, shape.RunCount, "Station layout native run count");
            AssertEqual(run, dto.Runs.Length, "Station layout native DTO run count");
        }
        AssertEqual(64, cells, "Station layout independent physical cell count");
    }
}
