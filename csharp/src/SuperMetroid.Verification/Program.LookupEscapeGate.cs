using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks the escape-gate program's complete byte and overlapping-word ownership against its source data.</summary>
    /// <param name="rom">Cartridge address space supplying the original escape-gate fields.</param>
    private static void VerifyEscapeGateProgramControls(SuperMetroidAddressSpace rom) => VerifyEscapeGateProgramField(rom, false);

    /// <summary>Checks the escape-gate program's complete draw-field byte and overlapping-word ownership.</summary>
    /// <param name="rom">Cartridge address space supplying the original escape-gate fields.</param>
    private static void VerifyEscapeGateProgramDraws(SuperMetroidAddressSpace rom) => VerifyEscapeGateProgramField(rom, true);

    /// <summary>Compares control or drawing field ownership over the full 16-bit address space and validates composed readers.</summary>
    /// <param name="rom">Cartridge address space containing the native program bytes.</param>
    /// <param name="draw">Selects the drawing operands when true, otherwise the control operands.</param>
    private static void VerifyEscapeGateProgramField(SuperMetroidAddressSpace rom, bool draw)
    {
        ushort[] controls = [0xbb34,0xbb38,0xbb3a,0xbb3e,0xbb42,0xbb44,0xbb48,0xbb4c,0xbb50];
        ushort[] draws = [0xbb36,0xbb3c,0xbb40,0xbb46,0xbb4a,0xbb4e];
        ushort[] fields = draw ? draws : controls;
        bool OwnsByte(int address) => fields.Contains((ushort)(address & ~1));
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool byteOwned = raw is >= 0xbb34 and <= 0xbb51;
            bool wordOwned = raw is >= 0xbb34 and < 0xbb51;
            AssertEqual(byteOwned, MotherBrainEscapeGatePlmProgramDefinitions.TryReadMechanicsByte(address, out byte b), "Escape gate complete byte domain");
            AssertEqual(wordOwned, MotherBrainEscapeGatePlmProgramDefinitions.TryReadMechanicsWord(address, out ushort w), "Escape gate complete overlapping word domain");
            if (!byteOwned) AssertEqual((byte)0, b, "Escape gate missing byte output");
            else if (OwnsByte(raw))
            {
                byte expected = rom.ReadByte(0x840000 | raw);
                AssertEqual(expected, b, "Escape gate original field byte");
                AssertTrue(RoomPlmProgramDefinitions.TryReadByte(address, out byte shared), "Escape gate composed byte reader");
                AssertEqual(expected, shared, "Escape gate composed original byte");
            }
            if (!wordOwned) AssertEqual((ushort)0, w, "Escape gate missing word output");
            else
            {
                int mask = (OwnsByte(raw) ? 0xff : 0) | (OwnsByte(raw + 1) ? 0xff00 : 0);
                if (mask == 0) continue;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | raw) & mask;
                AssertEqual(expected, w & mask, "Escape gate original field in overlapping word");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Escape gate composed word reader");
                AssertEqual(expected, shared & mask, "Escape gate composed original overlapping field");
            }
        }
    }

    /// <summary>Verifies the exported escape-gate frame identities, order, and shared geometry.</summary>
    /// <param name="rom">Cartridge address space used to check original frame geometry.</param>
    private static void VerifyEscapeGateDrawGeometry(SuperMetroidAddressSpace rom) => VerifyEscapeGateDrawField(rom, 0);

    /// <summary>Verifies the four collision words for each escape-gate frame against native data.</summary>
    /// <param name="rom">Cartridge address space containing native collision values.</param>
    private static void VerifyEscapeGateDrawCollision(SuperMetroidAddressSpace rom) => VerifyEscapeGateDrawField(rom, 1);

    /// <summary>Verifies the four visual words for each escape-gate frame against native data.</summary>
    /// <param name="rom">Cartridge address space containing native visual values.</param>
    private static void VerifyEscapeGateDrawVisuals(SuperMetroidAddressSpace rom) => VerifyEscapeGateDrawField(rom, 2);

    /// <summary>Checks the selected escape-gate draw contract, including pointer coverage and the chosen geometry, collision, or visual words.</summary>
    /// <param name="rom">Cartridge address space used as the native reference.</param>
    /// <param name="field">Selects geometry and identity checks (0), collision bits (1), or visual bits (2).</param>
    private static void VerifyEscapeGateDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0x9473,0x947f,0x948b];
        string[] ids = ["open","half-closed","closed"];
        var exported = MotherBrainEscapeGatePlmDrawDefinitions.All.ToArray();
        AssertEqual(3, exported.Length, "Escape gate export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, MotherBrainEscapeGatePlmDrawDefinitions.TryDescribe(pointer, out var shape), "Escape gate descriptor domain");
                AssertEqual(owned, MotherBrainEscapeGatePlmDrawDefinitions.TryGet(pointer, out var dto), "Escape gate DTO domain");
                if (!owned)
                {
                    AssertEqual(default(MotherBrainEscapeGatePlmDrawDefinitions.Draw), shape, "Escape gate missing descriptor");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Escape gate missing DTO");
                    AssertThrows<InvalidDataException>(() => MotherBrainEscapeGatePlmDrawDefinitions.VisualId(pointer), "Escape gate missing ID");
                }
            }
            foreach (string id in new[] { "OPEN", "half", "", "unknown" })
            {
                AssertTrue(!MotherBrainEscapeGatePlmDrawDefinitions.TryGetByVisualId(id, out var missing), "Escape gate ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Escape gate missing ID output");
            }
        }
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            MotherBrainEscapeGatePlmDrawDefinitions.TryDescribe(pointer, out var shape);
            MotherBrainEscapeGatePlmDrawDefinitions.TryGet(pointer, out var dto);
            if (field == 0)
            {
                AssertEqual(pointer, exported[index].Pointer, "Escape gate original order");
                AssertEqual(ids[index], MotherBrainEscapeGatePlmDrawDefinitions.VisualId(pointer), "Escape gate published ID");
                AssertTrue(MotherBrainEscapeGatePlmDrawDefinitions.TryGetByVisualId(ids[index], out var byId), "Escape gate reverse ID");
                AssertEqual(pointer, byId.Pointer, "Escape gate ID identity");
                AssertEqual((ushort)0x8004, ReadSamusEaterPlmWord(rom, 0x840000 | pointer), "Escape gate native vertical geometry");
                AssertEqual((ushort)0, ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + 10)), "Escape gate native final offset");
                foreach (var frame in new[] {dto,exported[index],byId})
                {
                    AssertEqual(1, frame.Runs.Length, "Escape gate one run");
                    AssertEqual((ushort)0x8004, frame.Runs.Span[0].DirectionAndCount, "Escape gate DTO geometry");
                    AssertEqual(4, frame.Runs.Span[0].LevelWords.Length, "Escape gate DTO width");
                    AssertEqual((sbyte)0, frame.Runs.Span[0].NextX, "Escape gate DTO final X");
                    AssertEqual((sbyte)0, frame.Runs.Span[0].NextY, "Escape gate DTO final Y");
                }
                foreach (int invalid in new[] {int.MinValue,-1,4,int.MaxValue})
                    AssertThrows<IndexOutOfRangeException>(() => shape.WordAt(invalid), "Escape gate calculated row bounds");
            }
            else for (int row = 0; row < 4; row++)
            {
                int mask = field == 1 ? 0xf000 : 0xfff;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + 2 + row * 2)) & mask;
                AssertEqual(expected, shape.WordAt(row) & mask, "Escape gate original calculated field");
                AssertEqual(expected, dto.Runs.Span[0].LevelWords.Span[row] & mask, "Escape gate original DTO field");
                AssertEqual(expected, exported[index].Runs.Span[0].LevelWords.Span[row] & mask, "Escape gate original export field");
            }
        }
    }
}
