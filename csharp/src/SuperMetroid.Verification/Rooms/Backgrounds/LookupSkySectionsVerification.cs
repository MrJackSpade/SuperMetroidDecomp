using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>Checks scrolling-sky section boundaries, speeds, and HDMA data slots.</summary>
internal static class LookupSkySectionsVerification
{
    /// <summary>Compares all 23 native bands and every camera Y's rendered band selection.</summary>
    internal static void VerifySkySectionTopPositions(SuperMetroidAddressSpace rom)
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
                uint velocity = SnesSixteenSixteen.Compose(ReadVerificationWord(rom, source + 4), ReadVerificationWord(rom, source + 2));
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
                expected = SnesFixedPosition.WholeOf(unchecked((int)expectedSlots[slot]));
                break;
            }
            ushort actual = sky.BuildGameplayHorizontalScrolls(unchecked((ushort)(worldY - 32)), 1)[0];
            AssertEqual(expected, actual, "Sky caller band selection, ushort camera wrap and terminal fallback");
        }
    }

    /// <summary>Compares all 23 fractional section speeds, including band 17.</summary>
    internal static void VerifySkySectionSubspeeds(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 23; index++)
            AssertEqual(ReadVerificationWord(rom, 0x88aec3 + index * 8),
                RoomFxRomData.ScrollingSky.GetSection(index).Subspeed, "Sky fractional speed including band17");
    }

    /// <summary>Compares each native section's whole-pixel speed word.</summary>
    internal static void VerifySkySectionSpeeds(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 23; index++)
            AssertEqual(ReadVerificationWord(rom, 0x88aec5 + index * 8),
                RoomFxRomData.ScrollingSky.GetSection(index).Speed, "Sky integer speed");
    }

    /// <summary>Checks the HDMA slot mapping, including the duplicate slot at band 8.</summary>
    internal static void VerifySkySectionDataSlots(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 23; index++)
            AssertEqual((ReadVerificationWord(rom, 0x88aec7 + index * 8) - 0x9f80) / 4,
                RoomFxRomData.ScrollingSky.GetSection(index).DataSlot, "Sky HDMA data slot including duplicate8");
    }
}
