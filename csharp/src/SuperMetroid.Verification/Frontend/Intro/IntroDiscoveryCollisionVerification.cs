using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>Compares the intro discovery room's collision with its retail collision source, decoded by the import codec.</summary>
internal static class IntroDiscoveryCollisionVerification
{
    /// <summary>
    /// Checks how every block of the built room collides against the decoded $300-byte source and its empty tail, and that
    /// each room is built independently; returns the decoded collision for other checks. The opening draws the room as
    /// pictures of its own, so what the source's blocks would show is not kept.
    /// </summary>
    internal static RoomCollisionType[] VerifyIntroDiscoveryCollision(IImportCartridgeSource rom)
    {
        byte[] native = RomDataReader.ReadFixedBank(rom, 0x8cc083, 0x300);
        RoomCollisionType[] actual = IntroBabyDiscoveryCollision.CreateCollision();
        AssertEqual(32 * 16, actual.Length, "discovery room allocation");
        var expected = new RoomCollisionType[actual.Length];
        for (int index = 0; index < actual.Length; index++)
        {
            ushort word = index < native.Length / 2
                ? LittleEndianBytes.Word(native[2 * index], native[2 * index + 1]) : (ushort)0;
            expected[index] = RoomLevelWordCodec.Decode(word).Collision;
            AssertEqual(expected[index], actual[index], "decoded original discovery collision and empty tail");
        }
        actual[0] = IntroBabyDiscoveryCollisionDefinitions.FinalRowBlock;
        actual[10 * 32] = IntroBabyDiscoveryCollisionDefinitions.EmptyBlock;
        RoomCollisionType[] fresh = IntroBabyDiscoveryCollision.CreateCollision();
        AssertTrue(!ReferenceEquals(actual, fresh), "collision rooms are built independently");
        AssertEqual(expected[0], fresh[0], "modified air block does not change next room");
        AssertEqual(expected[10 * 32], fresh[10 * 32], "modified strip does not change next room");
        return expected;
    }
}
