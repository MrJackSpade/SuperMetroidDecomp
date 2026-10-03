using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static readonly RoomFxType[] OriginalFxTilemapTypes =
        [RoomFxType.Lava, RoomFxType.Acid, RoomFxType.Water, RoomFxType.Spores, RoomFxType.Rain, RoomFxType.Fog];

    private static void VerifyFxTilemapTypeEnumeration()
    {
        AssertEqual(OriginalFxTilemapTypes.Length, RoomFxLayer3TilemapFormat.Types.Count, "Original page type count");
        AssertTrue(OriginalFxTilemapTypes.SequenceEqual(RoomFxLayer3TilemapFormat.Types), "Original enumerated page order");
        for (int index = 0; index < OriginalFxTilemapTypes.Length; index++)
            AssertEqual(OriginalFxTilemapTypes[index], RoomFxLayer3TilemapFormat.Types[index], "Original indexed page identity");
        foreach (int invalid in new[] { int.MinValue, -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = RoomFxLayer3TilemapFormat.Types[invalid]; },
                "Original read-only type list bounds");
    }

    private static void VerifyFxTilemapSourceAddresses(ISnesAddressSpace rom)
    {
        var original = OriginalFxTilemapTypes.ToDictionary(type => type,
            type => 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + (ushort)type));
        original.Add(RoomFxType.TourianEntranceStatue, 0x8a0000 | ReadVerificationWord(rom, 0x83ac16));
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            var type = (RoomFxType)value;
            if (original.TryGetValue(type, out int expected))
                AssertEqual(expected, RoomFxLayer3TilemapFormat.SourceAddress(type), "Original page pointer");
            else
                AssertThrows<InvalidDataException>(() => RoomFxLayer3TilemapFormat.SourceAddress(type),
                    "All unsupported ushort identities reject, including odd types");
        }
    }
}
