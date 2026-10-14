using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>Compares Tourian statue animated-object initialization with the
/// four native spawn calls and their initial instruction pointers.</summary>
internal static class LookupTourianSpawnVerification
{
    /// <summary>Reads the bounded bank-$8F spawn sequence and checks the
    /// production objects preserve its order, pointers, and initial timer.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Tourian statue spawn order")]
    internal static void VerifyTourianStatueSpawnOrder(ISnesAddressSpace rom)
    {
        var expected = new List<ushort>();
        int cursor = 0x8f91d7;
        while (rom.ReadCartridgeByte(cursor) != 0x60)
        {
            AssertTrue(expected.Count < 4, "Native statue setup has bounded spawn count");
            AssertEqual((byte)0xa0, rom.ReadCartridgeByte(cursor), "Original LDY immediate");
            expected.Add(ReadVerificationWord(rom, cursor + 1));
            AssertEqual((byte)0x22, rom.ReadCartridgeByte(cursor + 3), "Original JSL spawn");
            AssertEqual((ushort)0x8027, ReadVerificationWord(rom, cursor + 4), "Original animated spawn entry");
            AssertEqual((byte)0x87, rom.ReadCartridgeByte(cursor + 6), "Original animated spawn bank");
            cursor += 7;
        }
        AssertEqual(4, expected.Count, "Four native calls before RTS");

        var sequence = RepositoryInstallation.CreateRuntime(RepositoryInstallation.Installation.OpenRuntimeAddressSpace()).TourianStatues;
        sequence.SpawnAnimatedObjects();
        var objects = sequence.objects;
        AssertEqual(expected.Count, objects.Count, "Production spawn count");
        for (int index = 0; index < objects.Count; index++)
        {
            var tile = objects[index];
            var definition = tile.Definition;
            AssertEqual(expected[index], (ushort)definition.ObjectPointer, "Native spawn/handler order");
            AssertEqual(ReadVerificationWord(rom, 0x870000 | expected[index]),
                tile.Pointer, "Original initial program");
            AssertEqual((ushort)1, tile.Timer.RawValue, "Spawned instruction timer");
        }
    }
}
