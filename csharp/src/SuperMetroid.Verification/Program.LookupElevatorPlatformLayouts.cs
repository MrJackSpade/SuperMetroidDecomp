using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyElevatorPlatformLayoutGeometry(SuperMetroidAddressSpace rom) => VerifyElevatorPlatformLayoutDrawField(rom, 0);
    private static void VerifyElevatorPlatformLayoutCollision(SuperMetroidAddressSpace rom) => VerifyElevatorPlatformLayoutDrawField(rom, 1);
    private static void VerifyElevatorPlatformLayoutVisuals(SuperMetroidAddressSpace rom) => VerifyElevatorPlatformLayoutDrawField(rom, 2);

    private static void VerifyElevatorPlatformLayoutDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0xaa97,0xaaaf,0xaac7];
        string[] ids = ["first-frame","second-frame","third-frame"];
        var exported = ElevatorPlatformPlmDefinitions.DrawLists.ToArray();
        AssertEqual(3, exported.Length, "ElevatorPlatform layout export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, ElevatorPlatformPlmDefinitions.TryDescribe(pointer, out var shape), "ElevatorPlatform layout shape domain");
                AssertEqual(owned, ElevatorPlatformPlmDefinitions.TryGetDraw(pointer, out var dto), "ElevatorPlatform layout DTO domain");
                if (!owned)
                {
                    AssertEqual(default(ElevatorPlatformPlmDefinitions.Draw), shape, "ElevatorPlatform layout missing shape");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "ElevatorPlatform layout missing DTO");
                    AssertThrows<InvalidDataException>(() => ElevatorPlatformPlmDefinitions.VisualId(pointer), "ElevatorPlatform layout missing ID");
                }
            }
            foreach (string id in new[] { "FIRST-FRAME", "broken", "", "unknown" })
            {
                AssertTrue(!ElevatorPlatformPlmDefinitions.TryGetByVisualId(id, out var missing), "ElevatorPlatform layout ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "ElevatorPlatform layout rejected ID output");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            ElevatorPlatformPlmDefinitions.TryDescribe(pointer, out var shape);
            ElevatorPlatformPlmDefinitions.TryGetDraw(pointer, out var dto);
            ElevatorPlatformPlmDefinitions.TryGetByVisualId(ids[index], out var byId);
            if (field == 0)
            {
                AssertEqual(pointer, exported[index].Pointer, "ElevatorPlatform layout original export order");
                AssertEqual(ids[index], ElevatorPlatformPlmDefinitions.VisualId(pointer), "ElevatorPlatform layout published ID");
                AssertEqual(pointer, byId.Pointer, "ElevatorPlatform layout reverse ID");
                foreach (int invalid in new[] {int.MinValue,-1,shape.RunCount,int.MaxValue})
                {
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordCount(invalid), "ElevatorPlatform layout run bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.NextX(invalid), "ElevatorPlatform layout X bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.NextY(invalid), "ElevatorPlatform layout offset bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid, 0), "ElevatorPlatform layout word run bounds");
                }
            }
            var level = CreateRoom(32, 16, new ushort[512], new byte[512], blockDefinitions: new byte[0x400 * 8]);
            var plms = new RoomPlmSystem();
            var bus = new TestAddressSpace();
            if (field == 0)
            {
                bus.WriteBytes(0x8f9400, [0x0b,0xb7,3,4,0,0,0,0]);
                WriteWord(bus, 0x84b70d, 0xafb6);
                AssertEqual(1, plms.LoadRoomPopulation(bus, level, level.CreateBackgroundStreamer(),
                    new SnesVram(), SuperMetroid.AssetExtraction.RoomPlmPopulationImporter.Read(bus, 0x9400),
                    new Bank80SystemState(), AreaId.Crateria, () => null, () => false), "Elevator platform fixture spawn");
                plms.SetSoleInstructionPointerForVerification(0xf100, [1, pointer]);
                plms.Step(bus, level, level.CreateBackgroundStreamer(), 0, 0, 0);
            }
            int drawX = 3, drawY = 4;
            int cursor = 0x840000 | pointer;
            int run = 0;
            while (true)
            {
                ushort header = ReadSamusEaterPlmWord(rom, cursor);
                int count = header & 0x7fff;
                ushort offset = ReadSamusEaterPlmWord(rom, cursor + 2 + count * 2);
                if (field == 0)
                {
                    AssertEqual((ushort)count, header, "ElevatorPlatform layout native direction");
                    AssertEqual(count, shape.WordCount(run), "ElevatorPlatform layout native calculated count");
                    AssertEqual((ushort)((byte)shape.NextX(run) | (byte)shape.NextY(run) << 8), offset, "ElevatorPlatform layout native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],byId})
                    {
                        var part = frame.Runs.Span[run];
                        AssertEqual(header, part.DirectionAndCount, "ElevatorPlatform layout native DTO geometry");
                        AssertEqual(count, part.LevelWords.Length, "ElevatorPlatform layout native DTO count");
                        AssertEqual(offset, (ushort)((byte)part.NextX | (byte)part.NextY << 8), "ElevatorPlatform layout native DTO offset");
                    }
                    for (int cell = 0; cell < count; cell++)
                        AssertEqual(ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2),
                            level.GetCollisionBlock(drawX + cell, drawY).LevelWord,
                            "ElevatorPlatform native physical word at original absolute run position");
                    drawX = 3 + unchecked((sbyte)offset);
                    drawY = 4 + unchecked((sbyte)(offset >> 8));
                    int selectedRun = run;
                    foreach (int invalid in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(selectedRun, invalid), "ElevatorPlatform layout cell bounds");
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    int mask = field == 1 ? 0xf000 : 0xfff;
                    int expected = ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2) & mask;
                    AssertEqual(expected, shape.WordAt(run, cell) & mask, "ElevatorPlatform layout original calculated field");
                    foreach (var frame in new[] {dto,exported[index],byId})
                        AssertEqual(expected, frame.Runs.Span[run].LevelWords.Span[cell] & mask, "ElevatorPlatform layout original DTO field");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 6, "ElevatorPlatform layout native run termination");
            }
            AssertEqual(run, shape.RunCount, "ElevatorPlatform layout native run count");
            AssertEqual(run, dto.Runs.Length, "ElevatorPlatform layout native DTO run count");
        }
        AssertEqual(18, cells, "ElevatorPlatform layout independent physical cell count");
    }
}
