using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the compiled intro Mother Brain collision room against its retail source, decoded by the import codec.</summary>
internal static class IntroMotherBrainCollisionSourceVerification
{
    /// <summary>
    /// Compares all 224 decoded source blocks and the empty rows past them, and confirms each flashback builds an
    /// independent room; returns the decoded room for other checks.
    /// </summary>
    internal static RoomTerrainBlock[] VerifyIntroMotherBrainCollisionSource(IImportCartridgeSource rom)
    {
        byte[] native = RomDataReader.ReadFixedBank(rom, 0x8cbec3, 448);
        RoomTerrainBlock[] actual = IntroMotherBrainCollision.CreateForeground();
        AssertEqual(16 * 16, actual.Length, "Mother Brain flashback room allocation");
        var expected = new RoomTerrainBlock[actual.Length];
        for (int index = 0; index < actual.Length; index++)
        {
            ushort word = index * sizeof(ushort) < native.Length
                ? LittleEndianBytes.Word(native[index * 2], native[index * 2 + 1]) : (ushort)0;
            expected[index] = RoomLevelWordCodec.Decode(word);
        }
        AssertTrue(actual.SequenceEqual(expected), "all 224 Mother Brain collision blocks match the decoded original");
        actual.AsSpan().Fill(IntroMotherBrainCollisionDefinitions.Solid);
        RoomTerrainBlock[] fresh = IntroMotherBrainCollision.CreateForeground();
        AssertTrue(!ReferenceEquals(actual, fresh) && fresh.SequenceEqual(expected),
            "collision room is built independently per flashback");
        return expected;
    }
}
