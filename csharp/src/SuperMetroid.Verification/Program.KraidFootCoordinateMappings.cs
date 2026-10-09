using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies the first Kraid foot's calculated X coordinate for each native frame.</summary>
    /// <param name="rom">Retail address space used to read the original coordinate table.</param>
    private static void VerifyKraidFootFirstX(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidFootCoordinate), () => VerifyKraidFootCoordinate(rom, 0, false, KraidFootCollisionDefinitions.FirstX));

    /// <summary>Verifies the first Kraid foot's calculated Y coordinate for each native frame.</summary>
    /// <param name="rom">Retail address space used to read the original coordinate table.</param>
    private static void VerifyKraidFootFirstY(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidFootCoordinate), () => VerifyKraidFootCoordinate(rom, 0, true, KraidFootCollisionDefinitions.FirstY));

    /// <summary>Verifies the second Kraid foot's calculated X coordinate for each native frame.</summary>
    /// <param name="rom">Retail address space used to read the original coordinate table.</param>
    private static void VerifyKraidFootSecondX(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidFootCoordinate), () => VerifyKraidFootCoordinate(rom, 1, false, KraidFootCollisionDefinitions.SecondX));

    /// <summary>Verifies the second Kraid foot's calculated Y coordinate for each native frame.</summary>
    /// <param name="rom">Retail address space used to read the original coordinate table.</param>
    private static void VerifyKraidFootSecondY(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidFootCoordinate), () => VerifyKraidFootCoordinate(rom, 1, true, KraidFootCollisionDefinitions.SecondY));

    /// <summary>Compares one foot component's calculated coordinate and production view against the retail frame table.</summary>
    /// <param name="rom">Retail address space containing the frame records.</param>
    /// <param name="component">Zero-based foot component index, first or second.</param>
    /// <param name="vertical"><see langword="true"/> to compare Y; otherwise compares X.</param>
    /// <param name="calculate">Coordinate selector that resolves the requested frame ordinal.</param>
    private static void VerifyKraidFootCoordinate(SuperMetroidAddressSpace rom, int component, bool vertical, Func<int, short> calculate)
    {
        ushort Word(int pointer) => (ushort)(rom.ReadByte(0xa70000 | pointer) | rom.ReadByte(0xa70000 | (pointer + 1)) << 8);
        for (int ordinal = 0; ordinal <= 35; ordinal++)
        {
            ushort pointer = ordinal == 35 ? (ushort)0xa565 : (ushort)(0x8ce3 + 18 * ordinal);
            short expected = unchecked((short)Word(pointer + 2 + component * 8 + (vertical ? 2 : 0)));
            AssertEqual(expected, calculate(ordinal == 35 ? 0 : ordinal), "Original foot coordinate");
            AssertTrue(KraidFootCollisionDefinitions.TryGetComponents(pointer, out var components), "Native foot components exist");
            AssertEqual(2, components.Length, "Foot has two components");
            var actual = components[component];
            AssertEqual(expected, vertical ? actual.Y : actual.X, "Foot coordinate through production view");
        }
        AssertThrows<IndexOutOfRangeException>(() => calculate(-1), "Negative foot coordinate frame");
        AssertThrows<IndexOutOfRangeException>(() => calculate(35), "Past foot coordinate frame");
    }

    /// <summary>Checks Kraid's shared hitbox record and validates membership and component selectors across the full pointer domain.</summary>
    /// <param name="rom">Retail address space supplying the native hitbox and component records.</param>
    private static void VerifyKraidFootSharedHitbox(SuperMetroidAddressSpace rom)
    {
        ushort Word(int pointer) => (ushort)(rom.ReadByte(0xa70000 | pointer) | rom.ReadByte(0xa70000 | (pointer + 1)) << 8);
        var boxes = KraidFootCollisionDefinitions.HitboxesAt(0x9453);
        AssertEqual((int)Word(0x9453), boxes.Length, "Native shared rectangle count");
        var expected = new KraidFootCollisionHitbox(unchecked((short)Word(0x9455)), unchecked((short)Word(0x9457)),
            unchecked((short)Word(0x9459)), unchecked((short)Word(0x945b)), Word(0x945d), Word(0x945f));
        AssertEqual(expected, boxes[0], "Original shared hitbox fields");
        int count = 0;
        foreach (var box in boxes) { AssertEqual(expected, box, "Shared rectangle enumeration"); count++; }
        AssertEqual(1, count, "One enumerated rectangle");
        AssertEqual(expected, boxes.ToArray()[0], "Shared rectangle materialization");
        AssertThrows<IndexOutOfRangeException>(() => _ = boxes[-1], "Negative shared rectangle index");
        AssertThrows<IndexOutOfRangeException>(() => _ = boxes[1], "Past shared rectangle index");
        AssertThrows<InvalidDataException>(() => KraidFootCollisionDefinitions.HitboxesAt(0x9454), "Unaligned hitbox rejects");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            bool expectedMember = raw == 0xa565 || (raw >= 0x8ce3 && raw <= 0x8f47 && (raw - 0x8ce3) % 18 == 0);
            AssertEqual(expectedMember, KraidFootCollisionDefinitions.TryGetComponents((ushort)raw, out var components),
                "Complete native foot frame membership");
            if (!expectedMember) { AssertEqual(0, components.Length, "Rejected frame has no components"); continue; }
            AssertEqual((int)Word(raw), components.Length, "Native component count");
            count = 0;
            foreach (var component in components)
            {
                AssertEqual(components[count], component, "Calculated component enumeration");
                AssertEqual(Word(raw + 8 + 8 * count), component.HitboxPointer, "Original component hitbox selector");
                count++;
            }
            AssertEqual(2, count, "Two enumerated foot components");
            AssertThrows<IndexOutOfRangeException>(() => _ = components[-1], "Negative component index");
            AssertThrows<IndexOutOfRangeException>(() => _ = components[2], "Past component index");
        }
    }
}
