using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBombTorizoHandDrawGeometry(SuperMetroidAddressSpace rom) => VerifyBombTorizoHandDrawField(rom, 0);
    private static void VerifyBombTorizoHandDrawCollision(SuperMetroidAddressSpace rom) => VerifyBombTorizoHandDrawField(rom, 1);
    private static void VerifyBombTorizoHandDrawVisuals(SuperMetroidAddressSpace rom) => VerifyBombTorizoHandDrawField(rom, 2);

    private static void VerifyBombTorizoHandDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0x9877,0x989d];
        string[] ids = ["intact","cleared"];
        var exported = BombTorizoHandPlmDrawDefinitions.All.ToArray();
        AssertEqual(2, exported.Length, "Torizo hand export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, BombTorizoHandPlmDrawDefinitions.TryDescribe(pointer, out var shape), "Torizo hand descriptor domain");
                AssertEqual(owned, BombTorizoHandPlmDrawDefinitions.TryGet(pointer, out var dto), "Torizo hand DTO domain");
                if (!owned)
                {
                    AssertEqual(default(BombTorizoHandPlmDrawDefinitions.Draw), shape, "Torizo hand missing descriptor");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Torizo hand missing DTO");
                    AssertThrows<InvalidDataException>(() => BombTorizoHandPlmDrawDefinitions.VisualId(pointer), "Torizo hand missing ID");
                }
            }
            foreach (string bad in new[] {"Intact","cleared ","","unknown"})
            {
                AssertTrue(!BombTorizoHandPlmDrawDefinitions.TryGetByVisualId(bad, out var missing), "Torizo hand ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Torizo hand missing ID output");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            BombTorizoHandPlmDrawDefinitions.TryDescribe(pointer, out var shape);
            BombTorizoHandPlmDrawDefinitions.TryGet(pointer, out var dto);
            AssertEqual(pointer, exported[index].Pointer, "Torizo hand original export order");
            AssertEqual(ids[index], BombTorizoHandPlmDrawDefinitions.VisualId(pointer), "Torizo hand published identity");
            AssertTrue(BombTorizoHandPlmDrawDefinitions.TryGetByVisualId(ids[index], out var byId), "Torizo hand reverse identity");
            AssertEqual(pointer, byId.Pointer, "Torizo hand reverse pointer");
            int cursor = pointer, run = 0;
            sbyte x = 0, y = 0;
            while (true)
            {
                ushort count = ReadSamusEaterPlmWord(rom, 0x840000 | cursor);
                cursor += 2;
                AssertTrue(count is > 0 and < 0x8000, "Torizo hand original horizontal run");
                if (field == 0)
                {
                    AssertEqual((int)count, shape.WordCount(run), "Torizo hand native run width");
                    AssertEqual(x, shape.OriginX(run), "Torizo hand original origin-relative X");
                    AssertEqual(y, shape.OriginY(run), "Torizo hand original origin-relative Y");
                    foreach (var frame in new[] {dto,exported[index],byId})
                    {
                        AssertEqual(count, frame.Runs.Span[run].DirectionAndCount, "Torizo hand DTO count");
                        AssertEqual((int)count, frame.Runs.Span[run].LevelWords.Length, "Torizo hand DTO width");
                    }
                    foreach (int bad in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(run,bad), "Torizo hand cell bounds");
                }
                for (int word = 0; word < count; word++, cursor += 2)
                {
                    if (field != 0)
                    {
                        int mask = field == 1 ? 0xf000 : 0xfff;
                        int expected = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & mask;
                        AssertEqual(expected, shape.WordAt(run,word) & mask, "Torizo hand original calculated field");
                        AssertEqual(expected, dto.Runs.Span[run].LevelWords.Span[word] & mask, "Torizo hand original DTO field");
                        AssertEqual(expected, exported[index].Runs.Span[run].LevelWords.Span[word] & mask, "Torizo hand original export field");
                    }
                    cells++;
                }
                x = unchecked((sbyte)rom.ReadByte(0x840000 | cursor++));
                y = unchecked((sbyte)rom.ReadByte(0x840000 | cursor++));
                if (field == 0)
                {
                    AssertEqual(x, shape.NextX(run), "Torizo hand native next X");
                    AssertEqual(y, shape.NextY(run), "Torizo hand native next Y");
                    foreach (var frame in new[] {dto,exported[index],byId})
                    {
                        AssertEqual(x, frame.Runs.Span[run].NextX, "Torizo hand DTO next X");
                        AssertEqual(y, frame.Runs.Span[run].NextY, "Torizo hand DTO next Y");
                    }
                }
                run++;
                if (x == 0 && y == 0) break;
            }
            AssertEqual(index == 0 ? 0x9897 : 0x98d1, cursor, "Torizo hand native record end");
            AssertEqual(run, shape.RunCount, "Torizo hand native run count");
            AssertEqual(run, dto.Runs.Length, "Torizo hand DTO run count");
            AssertEqual(run, exported[index].Runs.Length, "Torizo hand export run count");
            if (field == 0)
            foreach (int bad in new[] {int.MinValue,-1,run,int.MaxValue})
            {
                AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(bad,0), "Torizo hand run bounds");
                AssertThrows<IndexOutOfRangeException>(() => shape.OriginX(bad), "Torizo hand X bounds");
                AssertThrows<IndexOutOfRangeException>(() => shape.OriginY(bad), "Torizo hand Y bounds");
                AssertThrows<IndexOutOfRangeException>(() => shape.NextX(bad), "Torizo hand next X bounds");
                AssertThrows<IndexOutOfRangeException>(() => shape.NextY(bad), "Torizo hand next Y bounds");
            }
        }
        AssertEqual(24, cells, "Torizo hand complete native cell domain");
    }
}
