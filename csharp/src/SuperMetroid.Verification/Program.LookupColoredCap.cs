using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks that all 48 colored-cap frames retain their pointer order, visual identities, and native draw geometry.</summary>
    private static void VerifyColoredCapGeometry(SuperMetroidAddressSpace rom) => VerifyColoredCapField(rom, 0);

    /// <summary>Compares the collision-related high bits of every colored-cap row with its native PLM data.</summary>
    private static void VerifyColoredCapCollision(SuperMetroidAddressSpace rom) => VerifyColoredCapField(rom, 1);

    /// <summary>Compares the visual tile bits of every colored-cap row with its native PLM data.</summary>
    private static void VerifyColoredCapVisuals(SuperMetroidAddressSpace rom) => VerifyColoredCapField(rom, 2);

    /// <summary>Validates either frame identity and geometry or one selected set of row-word bits against the cartridge data.</summary>
    /// <param name="rom">The address space used to read the cartridge's native PLM words.</param>
    /// <param name="field">Selects full geometry and identity checks (0), collision bits (1), or visual tile bits (2).</param>
    private static void VerifyColoredCapField(SuperMetroidAddressSpace rom, int field)
    {
        (ushort First, string Name)[] families = [
            (0xa767,"yellow-left"),(0xa797,"yellow-right"),(0xa7c7,"yellow-up"),(0xa7f7,"yellow-down"),
            (0xa827,"green-left"),(0xa857,"green-right"),(0xa887,"green-up"),(0xa8b7,"green-down"),
            (0xa8e7,"red-left"),(0xa917,"red-right"),(0xa947,"red-up"),(0xa977,"red-down")];
        ushort[] pointers = families.SelectMany(family => Enumerable.Range(0,4).Select(frame => (ushort)(family.First+12*frame))).ToArray();
        string[] ids = families.SelectMany(family => Enumerable.Range(0,4).Select(frame => $"{family.Name}-frame-{frame}")).ToArray();
        var exported = ColoredDoorPlmDrawDefinitions.All.ToArray();
        AssertEqual(48, exported.Length, "Colored cap export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, ColoredDoorPlmDrawDefinitions.TryDescribe(pointer, out var shape), "Colored cap descriptor domain");
                AssertEqual(owned, ColoredDoorPlmDrawDefinitions.TryGet(pointer, out var dto), "Colored cap DTO domain");
                if (!owned)
                {
                    AssertEqual(default(ColoredDoorPlmDrawDefinitions.Draw), shape, "Colored cap missing descriptor");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Colored cap missing DTO");
                    AssertThrows<InvalidDataException>(() => ColoredDoorPlmDrawDefinitions.VisualId(pointer), "Colored cap missing ID");
                }
            }
            foreach (string id in new[] { "YELLOW-LEFT-FRAME-0", "green-left-frame-4", "", "unknown" })
            {
                AssertTrue(!ColoredDoorPlmDrawDefinitions.TryGetByVisualId(id, out var missing), "Colored cap ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Colored cap missing ID output");
            }
        }
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            ColoredDoorPlmDrawDefinitions.TryDescribe(pointer, out var shape);
            ColoredDoorPlmDrawDefinitions.TryGet(pointer, out var dto);
            if (field == 0)
            {
                AssertEqual(pointer, exported[index].Pointer, "Colored cap original order");
                AssertEqual(ids[index], ColoredDoorPlmDrawDefinitions.VisualId(pointer), "Colored cap published ID");
                AssertTrue(ColoredDoorPlmDrawDefinitions.TryGetByVisualId(ids[index], out var byId), "Colored cap reverse ID");
                AssertEqual(pointer, byId.Pointer, "Colored cap ID identity");
                AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | pointer), shape.DirectionAndCount, "Colored cap native geometry");
                AssertEqual((ushort)0, ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + 10)), "Colored cap native final offset");
                foreach (var frame in new[] {dto,exported[index],byId})
                {
                    AssertEqual(1, frame.Runs.Length, "Colored cap one run");
                    AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | pointer), frame.Runs.Span[0].DirectionAndCount, "Colored cap DTO geometry");
                    AssertEqual(4, frame.Runs.Span[0].LevelWords.Length, "Colored cap DTO width");
                    AssertEqual((sbyte)0, frame.Runs.Span[0].NextX, "Colored cap DTO final X");
                    AssertEqual((sbyte)0, frame.Runs.Span[0].NextY, "Colored cap DTO final Y");
                }
                foreach (int invalid in new[] {int.MinValue,-1,4,int.MaxValue})
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid), "Colored cap calculated row bounds");
            }
            else for (int row = 0; row < 4; row++)
            {
                int mask = field == 1 ? 0xf000 : 0xfff;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + 2 + row * 2)) & mask;
                AssertEqual(expected, shape.WordAt(row) & mask, "Colored cap original calculated field");
                AssertEqual(expected, dto.Runs.Span[0].LevelWords.Span[row] & mask, "Colored cap original DTO field");
                AssertEqual(expected, exported[index].Runs.Span[0].LevelWords.Span[row] & mask, "Colored cap original export field");
            }
        }
    }
}
