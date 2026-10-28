using static SuperMetroid.Verification.Rooms.Plms.PlmRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Cross-checks Mother Brain fake-death wall and door draw records against bank $84.</summary>
internal static class LookupMotherBrainBoundaryVerification
{
    /// <summary>Checks pointer ownership, two-run geometry, and bounds for the wall and door boundary records.</summary>
    internal static void VerifyMotherBrainBoundaryGeometry(SuperMetroidAddressSpace rom)
    {
        var exports = MotherBrainFakeDeathPlmDrawDefinitions.All.Take(2).ToArray();
        ushort[] pointers = [0x94a3,0x94b1];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            int index = Array.IndexOf(pointers, (ushort)raw);
            AssertEqual(index >= 0, MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBoundary(NativePlmDraws.At<MotherBrainFakeDeathDraw>((ushort)raw), out var shape),
                "Mother Brain wall/door ownership");
            if (index < 0)
            {
                AssertEqual(default(MotherBrainFakeDeathPlmDrawDefinitions.BoundaryDraw), shape, "Mother Brain missing wall/door descriptor");
                continue;
            }
            AssertEqual((ushort)raw, PlmDrawAddressCodec.Of(exports[index].Draw), "Mother Brain boundary export order");
            MotherBrainFakeDeathPlmDrawDefinitions.TryGet(NativePlmDraws.At<MotherBrainFakeDeathDraw>((ushort)raw), out var dto);
            int cursor = raw;
            for (int run = 0; run < 2; run++)
            {
                ushort direction = ReadSamusEaterPlmWord(rom, 0x840000 | cursor);
                int count = direction & 0x7fff;
                AssertEqual((ushort)(0x8000 | shape.Count(run)), direction, "Mother Brain vertical boundary run");
                cursor += 2 + count * 2;
                sbyte x = unchecked((sbyte)MotherBrainBoundaryGeometryRomByte(rom, 0x840000 | cursor++));
                sbyte y = unchecked((sbyte)MotherBrainBoundaryGeometryRomByte(rom, 0x840000 | cursor++));
                AssertEqual(x, shape.NextX(run), "Mother Brain native boundary X");
                AssertEqual(y, shape.NextY(run), "Mother Brain native boundary Y");
                foreach (var draw in new[] {dto,exports[index]})
                {
                    AssertEqual(2, draw.Runs.Length, "Mother Brain two boundary runs");
                    AssertRunHeader(direction, draw.Runs.Span[run], "Mother Brain DTO direction/count");
                    AssertEqual(count, draw.Runs.Span[run].LevelWords.Length, "Mother Brain DTO run width");
                    AssertEqual(x, draw.Runs.Span[run].Next.X, "Mother Brain DTO X");
                    AssertEqual(y, draw.Runs.Span[run].Next.Y, "Mother Brain DTO Y");
                }
                foreach (int bad in new[] {int.MinValue,-1,count,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.WordAt(run,bad), "Mother Brain boundary cell bounds");
            }
            AssertEqual(index == 0 ? 0x94b1 : 0x94c9, cursor, "Mother Brain exact boundary record end");
            foreach (int bad in new[] {int.MinValue,-1,2,int.MaxValue})
            {
                AssertThrows<ArgumentOutOfRangeException>(() => shape.WordAt(bad,0), "Mother Brain boundary run bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => shape.NextX(bad), "Mother Brain boundary X bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => shape.NextY(bad), "Mother Brain boundary Y bounds");
            }
        }
    }

    /// <summary>Reads native boundary bytes for the geometry comparison.</summary>
    private static byte MotherBrainBoundaryGeometryRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Compares collision bits in both boundary records with native block words.</summary>
    internal static void VerifyMotherBrainBoundaryCollision(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyMotherBrainBoundaryCells), () => VerifyMotherBrainBoundaryCells(rom, 0));

    /// <summary>Compares visual bits of the fake-death wall record with native block words.</summary>
    internal static void VerifyMotherBrainWallVisuals(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyMotherBrainBoundaryCells), () => VerifyMotherBrainBoundaryCells(rom, 1));

    /// <summary>Compares visual bits of the fake-death door record with native block words.</summary>
    internal static void VerifyMotherBrainDoorVisuals(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyMotherBrainBoundaryCells), () => VerifyMotherBrainBoundaryCells(rom, 2));

    /// <summary>Checks the selected collision or visual field of each native boundary cell.</summary>
    private static void VerifyMotherBrainBoundaryCells(SuperMetroidAddressSpace rom, int field)
    {
        foreach (ushort pointer in new ushort[] {0x94a3,0x94b1})
        {
            if (field == 1 && pointer != 0x94a3 || field == 2 && pointer != 0x94b1) continue;
            MotherBrainFakeDeathPlmDrawDefinitions.TryDescribeBoundary(NativePlmDraws.At<MotherBrainFakeDeathDraw>(pointer), out var shape);
            MotherBrainFakeDeathPlmDrawDefinitions.TryGet(NativePlmDraws.At<MotherBrainFakeDeathDraw>(pointer), out var dto);
            int cursor = pointer;
            int mask = field == 0 ? 0xf000 : 0xfff;
            for (int run = 0; run < 2; run++)
            {
                int count = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                for (int block = 0; block < count; block++, cursor += 2)
                {
                    int expected = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & mask;
                    AssertEqual(expected, shape.WordAt(run,block) & mask, "Mother Brain native boundary field");
                    AssertEqual(expected, dto.Runs.Span[run].LevelWords.Span[block] & mask, "Mother Brain DTO boundary field");
                }
                cursor += 2;
            }
        }
    }
}
