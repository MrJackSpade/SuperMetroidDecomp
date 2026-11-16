using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Checks the intro Mother Brain room's collision against its retail source, decoded by the import codec.</summary>
internal static class IntroMotherBrainCollisionSourceVerification
{
    /// <summary>
    /// Compares how all 224 decoded source blocks and the empty rows past them collide, and confirms each flashback builds
    /// an independent room; returns the decoded collision for other checks. The flashback draws the room as pictures of its
    /// own, so what the source's blocks would show is not kept.
    /// </summary>
    internal static RoomCollisionType[] VerifyIntroMotherBrainCollisionSource(IImportCartridgeSource rom)
    {
        byte[] native = RomDataReader.ReadFixedBank(rom, 0x8cbec3, 448);
        RoomCollisionType[] actual = IntroMotherBrainCollision.CreateCollision();
        AssertEqual(16 * 16, actual.Length, "Mother Brain flashback room allocation");
        var expected = new RoomCollisionType[actual.Length];
        for (int index = 0; index < actual.Length; index++)
        {
            ushort word = index * sizeof(ushort) < native.Length
                ? LittleEndianBytes.Word(native[index * 2], native[index * 2 + 1]) : (ushort)0;
            expected[index] = RoomLevelWordCodec.Decode(word).Collision;
        }
        AssertTrue(actual.SequenceEqual(expected), "all 224 Mother Brain collision blocks match the decoded original");
        actual.AsSpan().Fill(IntroMotherBrainCollisionDefinitions.Solid);
        RoomCollisionType[] fresh = IntroMotherBrainCollision.CreateCollision();
        AssertTrue(!ReferenceEquals(actual, fresh) && fresh.SequenceEqual(expected),
            "collision room is built independently per flashback");
        return expected;
    }
}
