using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks all 23 band boundaries against the ROM table and verifies camera selection, 16-frame scroll accumulation, wrapping, and terminal fallback.</summary>
    /// <param name="rom">Retail address space containing the scrolling-sky section records.</param>
    private static void VerifySkySectionTopPositions(SuperMetroidAddressSpace rom)
    {
        int originalCount = ReadVerificationWord(rom, 0x88adef) / 8;
        AssertEqual(23, originalCount, "Native sky loop bound");
        AssertEqual(originalCount, RoomFxRomData.ScrollingSky.SectionCount, "Compiled sky section domain");
        for (int index = 0; index < originalCount; index++)
            AssertEqual(ReadVerificationWord(rom, 0x88aec1 + index * 8),
                RoomFxRomData.ScrollingSky.GetSection(index).TopPosition, "Sky native band boundary");
        foreach (int invalid in new[] { int.MinValue, -1, 23, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => RoomFxRomData.ScrollingSky.GetSection(invalid),
                "Sky preserves former indexed row bounds");

        var sky = new ScrollingSkyState();
        var expectedSlots = new uint[23];
        for (int frame = 0; frame < 16; frame++)
        {
            sky.ProcessFrame(0x0300, false, new VramWriteQueue());
            for (int index = 0; index < originalCount; index++)
            {
                int source = 0x88aec1 + index * 8;
                int slot = (ReadVerificationWord(rom, source + 6) - 0x9f80) / 4;
                uint velocity = (uint)ReadVerificationWord(rom, source + 2) |
                    ((uint)ReadVerificationWord(rom, source + 4) << 16);
                expectedSlots[slot] = unchecked(expectedSlots[slot] + velocity);
            }
            expectedSlots[22] = 0;
        }
        ushort end = ReadVerificationWord(rom, 0x88af79);
        for (int worldY = 0; worldY <= ushort.MaxValue; worldY++)
        {
            ushort expected = 0;
            for (int index = 0; index < originalCount; index++)
            {
                int source = 0x88aec1 + index * 8;
                ushort top = ReadVerificationWord(rom, source);
                ushort next = index + 1 < originalCount ? ReadVerificationWord(rom, source + 8) : end;
                if (worldY < top || worldY >= next) continue;
                int slot = (ReadVerificationWord(rom, source + 6) - 0x9f80) / 4;
                expected = (ushort)(expectedSlots[slot] >> 16);
                break;
            }
            ushort actual = sky.BuildGameplayHorizontalScrolls(unchecked((ushort)(worldY - 32)), 1)[0];
            AssertEqual(expected, actual, "Sky caller band selection, ushort camera wrap and terminal fallback");
        }
    }

    /// <summary>Checks each compiled sky band's fractional scroll increment against its native record, including the duplicate slot.</summary>
    /// <param name="rom">Retail address space containing the scrolling-sky section records.</param>
    private static void VerifySkySectionSubspeeds(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 23; index++)
            AssertEqual(ReadVerificationWord(rom, 0x88aec3 + index * 8),
                RoomFxRomData.ScrollingSky.GetSection(index).Subspeed, "Sky fractional speed including band17");
    }

    /// <summary>Checks each compiled sky band's integer scroll increment against its native record.</summary>
    /// <param name="rom">Retail address space containing the scrolling-sky section records.</param>
    private static void VerifySkySectionSpeeds(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 23; index++)
            AssertEqual(ReadVerificationWord(rom, 0x88aec5 + index * 8),
                RoomFxRomData.ScrollingSky.GetSection(index).Speed, "Sky integer speed");
    }

    /// <summary>Checks the HDMA accumulator slot assigned to each sky band against the native table, including repeated slot assignments.</summary>
    /// <param name="rom">Retail address space containing the scrolling-sky section records.</param>
    private static void VerifySkySectionDataSlots(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 23; index++)
            AssertEqual((ReadVerificationWord(rom, 0x88aec7 + index * 8) - 0x9f80) / 4,
                RoomFxRomData.ScrollingSky.GetSection(index).DataSlot, "Sky HDMA data slot including duplicate8");
    }
}
