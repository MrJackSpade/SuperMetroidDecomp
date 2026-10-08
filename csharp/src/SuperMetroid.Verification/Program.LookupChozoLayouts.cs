using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyChozoLayoutGeometry(SuperMetroidAddressSpace rom) => VerifyChozoLayoutDrawField(rom, 0);
    private static void VerifyChozoLayoutCollision(SuperMetroidAddressSpace rom) => VerifyChozoLayoutDrawField(rom, 1);
    private static void VerifyChozoLayoutVisuals(SuperMetroidAddressSpace rom) => VerifyChozoLayoutDrawField(rom, 2);

    private static void VerifyChozoLayoutDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0xa2b5,0x9cc5,0x9d0f];
        string[] ids = ["lower-norfair-cleared-hand","wrecked-ship-clear-slope-access","wrecked-ship-block-slope-access"];
        var exported = ChozoStatuePlmDrawDefinitions.All.ToArray();
        AssertEqual(3, exported.Length, "Chozo layout export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, ChozoStatuePlmDrawDefinitions.TryDescribe(pointer, out var shape), "Chozo layout shape domain");
                AssertEqual(owned, ChozoStatuePlmDrawDefinitions.TryGet(pointer, out var dto), "Chozo layout DTO domain");
                if (!owned)
                {
                    AssertEqual(default(ChozoStatuePlmDrawDefinitions.Draw), shape, "Chozo layout missing shape");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Chozo layout missing DTO");
                    AssertThrows<InvalidDataException>(() => ChozoStatuePlmDrawDefinitions.VisualId(pointer), "Chozo layout missing ID");
                }
            }
            foreach (string id in new[] { "LOWER-NORFAIR-CLEARED-HAND", "broken", "", "unknown" })
            {
                AssertTrue(!ChozoStatuePlmDrawDefinitions.TryGetByVisualId(id, out var missing), "Chozo layout ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Chozo layout rejected ID output");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            ChozoStatuePlmDrawDefinitions.TryDescribe(pointer, out var shape);
            ChozoStatuePlmDrawDefinitions.TryGet(pointer, out var dto);
            ChozoStatuePlmDrawDefinitions.TryGetByVisualId(ids[index], out var byId);
            if (field == 0)
            {
                AssertEqual(pointer, exported[index].Pointer, "Chozo layout original export order");
                AssertEqual(ids[index], ChozoStatuePlmDrawDefinitions.VisualId(pointer), "Chozo layout published ID");
                AssertEqual(pointer, byId.Pointer, "Chozo layout reverse ID");
                foreach (int invalid in new[] {int.MinValue,-1,shape.RunCount,int.MaxValue})
                {
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordCount(invalid), "Chozo layout run bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.NextX(invalid), "Chozo layout X bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.NextY(invalid), "Chozo layout offset bounds");
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid, 0), "Chozo layout word run bounds");
                }
            }
            var level = CreateRoom(32, 16, new ushort[512], new byte[512], blockDefinitions: new byte[0x400 * 8]);
            var plms = new RoomPlmSystem();
            var guarded = new ChozoProgramAndDrawReadGuard(rom);
            if (field == 0)
            {
                AssertTrue(plms.TrySpawnChozoStatuePlm(level,
                    new ChozoStatuePlmRequest(0xd6f8, 3, 4, IsHardcoded: false)), "Chozo layout fixture spawn");
                plms.DrawSolePlmFrameForVerification(guarded, level, level.CreateBackgroundStreamer(), pointer);
                AssertEqual(0, guarded.ForbiddenReadAttempts, "Chozo calculated draw avoids ROM reads");
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
                    AssertEqual((ushort)count, header, "Chozo layout native direction");
                    AssertEqual(count, shape.WordCount(run), "Chozo layout native calculated count");
                    AssertEqual((ushort)((byte)shape.NextX(run) | (byte)shape.NextY(run) << 8), offset, "Chozo layout native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],byId})
                    {
                        var part = frame.Runs.Span[run];
                        AssertEqual(header, part.DirectionAndCount, "Chozo layout native DTO geometry");
                        AssertEqual(count, part.LevelWords.Length, "Chozo layout native DTO count");
                        AssertEqual(offset, (ushort)((byte)part.NextX | (byte)part.NextY << 8), "Chozo layout native DTO offset");
                    }
                    for (int cell = 0; cell < count; cell++)
                        AssertEqual(ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2),
                            level.GetCollisionBlock(drawX + cell, drawY).LevelWord,
                            "Chozo native physical word at original absolute run position");
                    drawX = 3 + unchecked((sbyte)offset);
                    drawY = 4 + unchecked((sbyte)(offset >> 8));
                    int selectedRun = run;
                    foreach (int invalid in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(selectedRun, invalid), "Chozo layout cell bounds");
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    int mask = field == 1 ? 0xf000 : 0xfff;
                    int expected = ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2) & mask;
                    AssertEqual(expected, shape.WordAt(run, cell) & mask, "Chozo layout original calculated field");
                    foreach (var frame in new[] {dto,exported[index],byId})
                        AssertEqual(expected, frame.Runs.Span[run].LevelWords.Span[cell] & mask, "Chozo layout original DTO field");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 6, "Chozo layout native run termination");
            }
            AssertEqual(run, shape.RunCount, "Chozo layout native run count");
            AssertEqual(run, dto.Runs.Length, "Chozo layout native DTO run count");
        }
        AssertEqual(55, cells, "Chozo layout independent physical cell count");
    }
}
