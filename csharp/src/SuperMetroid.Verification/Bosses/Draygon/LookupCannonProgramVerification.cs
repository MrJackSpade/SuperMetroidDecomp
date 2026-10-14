using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Partitions Draygon cannon PLM programs by control, draw, target, callback, and hit-count bytes.</summary>
internal static class LookupCannonProgramVerification
{
    /// <summary>Checks native cannon instruction bytes outside the identified visual, target, callback, and hit-count fields.</summary>
    internal static void VerifyCannonControls(SuperMetroidAddressSpace rom) => VerifyCannonField(rom, 0);
    /// <summary>Checks the eleven right-hand draw positions and relocated left-hand counterparts.</summary>
    internal static void VerifyCannonDraws(SuperMetroidAddressSpace rom) => VerifyCannonField(rom, 1);
    /// <summary>Checks five native branch-target positions per cannon facing.</summary>
    internal static void VerifyCannonTargets(SuperMetroidAddressSpace rom) => VerifyCannonField(rom, 2);
    /// <summary>Checks each facing's native callback operand bytes.</summary>
    internal static void VerifyCannonCallback(SuperMetroidAddressSpace rom) => VerifyCannonField(rom, 3);
    /// <summary>Checks the native hit-count operand in both cannon lists.</summary>
    internal static void VerifyCannonHitCount(SuperMetroidAddressSpace rom) => VerifyCannonField(rom, 4);

    /// <summary>Compares one independently located field across all overlapping byte and word reads in both bank-$84 cannon lists.</summary>
    private static void VerifyCannonField(SuperMetroidAddressSpace rom, int field)
    {
        int[] draws = [0xdce8,0xdcf7,0xdcfb,0xdcff,0xdd03,0xdd07,0xdd0b,0xdd15,0xdd19,0xdd1d,0xdd21];
        int[] targets = [0xdce0,0xdcee,0xdcf3,0xdd0f,0xdd25];
        int FieldAt(int raw)
        {
            int a = raw >= 0xddb9 ? raw - 0xddb9 + 0xdcde : raw;
            if (draws.Any(start => a == start || a == start + 1)) return 1;
            if (targets.Any(start => a == start || a == start + 1)) return 2;
            return a is 0xdce4 or 0xdce5 ? 3 : a == 0xdcf2 ? 4 : 0;
        }
        int count = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool ownsByte = raw is >= 0xdcde and <= 0xdd26 or >= 0xddb9 and <= 0xde01;
            bool ownsWord = ownsByte && raw is not (0xdd26 or 0xde01);
            AssertEqual(ownsByte, DraygonCannonPlmProgramDefinitions.TryReadMechanicsByte(address, out byte b), "Cannon byte domain");
            AssertEqual(ownsWord, DraygonCannonPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort w), "Cannon word domain");
            if (!ownsByte) AssertEqual((byte)0, b, "Cannon rejected byte output");
            else if (FieldAt(raw) == field)
            {
                count++;
                byte expected = CannonFieldRomByte(rom, 0x840000 | raw);
                AssertEqual(expected, b, "Cannon native field byte");
                AssertTrue(RoomPlmProgramDefinitions.TryReadByte(address, out byte shared), "Cannon composed byte ownership");
                AssertEqual(expected, shared, "Cannon composed native byte");
            }
            if (!ownsWord) AssertEqual((ushort)0, w, "Cannon rejected word output");
            else
            {
                int mask = (FieldAt(raw) == field ? 0xff : 0) | (FieldAt(raw + 1) == field ? 0xff00 : 0);
                if (mask == 0) continue;
                int expected = ReadSamusEaterPlmWord(rom, 0x840000 | raw) & mask;
                AssertEqual(expected, w & mask, "Cannon native overlapping field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Cannon composed word ownership");
                AssertEqual(expected, shared & mask, "Cannon composed overlapping field");
            }
        }
        AssertEqual(field switch {0 => 76,1 => 44,2 => 20,3 => 4,_ => 2}, count, "Cannon independent field count");
    }

    /// <summary>Reads a retail cannon program byte solely for the field oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for cannon field")]
    private static byte CannonFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
