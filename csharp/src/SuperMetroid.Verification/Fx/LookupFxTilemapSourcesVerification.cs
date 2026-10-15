using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks the six ordinary layer-3 FX pages and Tourian statue source against retail pointer tables.</summary>
internal static class LookupFxTilemapSourcesVerification
{
    /// <summary>Native page order before the separately selected Tourian entrance statue tilemap.</summary>
    internal static readonly RoomFxType[] OriginalFxTilemapTypes =
        [RoomFxType.Lava, RoomFxType.Acid, RoomFxType.Water, RoomFxType.Spores, RoomFxType.Rain, RoomFxType.Fog];

    /// <summary>Checks the six-page iteration order and rejects indexes outside the typed view.</summary>
    internal static void VerifyFxTilemapTypeEnumeration()
    {
        AssertEqual(OriginalFxTilemapTypes.Length, RoomFxLayer3TilemapFormat.Types.Count, "Original page type count");
        AssertTrue(OriginalFxTilemapTypes.SequenceEqual(RoomFxLayer3TilemapFormat.Types), "Original enumerated page order");
        for (int index = 0; index < OriginalFxTilemapTypes.Length; index++)
            AssertEqual(OriginalFxTilemapTypes[index], RoomFxLayer3TilemapFormat.Types[index], "Original indexed page identity");
        foreach (int invalid in new[] { int.MinValue, -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = RoomFxLayer3TilemapFormat.Types[invalid]; },
                "Original read-only type list bounds");
    }

    /// <summary>Compares the seven admitted bank-$8A source addresses and rejects all other 16-bit FX identities.</summary>
    internal static void VerifyFxTilemapSourceAddresses(ISnesAddressSpace rom)
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
