using System.Buffers.Binary;
using System.Text.Json;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Fx.LookupFxTilemapSourcesVerification;
using static SuperMetroid.Verification.Rooms.TilemapJsonContractsVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks FX layer-3 page dispatch for native sources, editable pages, and unsupported identities.</summary>
internal static class LookupFxTilemapDispatchVerification
{
    /// <summary>Compares native page bytes, verifies independent edited pages, and rejects missing or unknown page keys.</summary>
    internal static void VerifyFxTilemapPageDispatch(ISnesAddressSpace rom, RoomFxLayer3Pages stock)
    {
        var native = new Dictionary<RoomFxType, byte[]>();
        var markers = new Dictionary<RoomFxType, ushort>();
        var document = new RoomFxLayer3TilemapDocument { Version = 1, Pages = [] };
        for (int index = 0; index < OriginalFxTilemapTypes.Length; index++)
        {
            RoomFxType type = OriginalFxTilemapTypes[index];
            int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Word(type));
            var bytes = new byte[0x840];
            for (int offset = 0; offset < bytes.Length; offset++) bytes[offset] = FxTilemapPageDispatchRomByte(rom, source + offset);
            native.Add(type, bytes);
            ushort marker = (ushort)(index + 1);
            markers.Add(type, marker);
            document.Pages.Add(type.ToString(), Enumerable.Repeat(TilemapContractCell(marker), 32 * 33).ToArray());
        }
        RoomFxLayer3Pages Load() => RoomFxLayer3Pages.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document)));
        var edited = Load();
        native.Add(RoomFxType.TourianEntranceStatue, native[RoomFxType.Water]);
        markers.Add(RoomFxType.TourianEntranceStatue, markers[RoomFxType.Water]);
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            var type = (RoomFxType)value;
            if (native.TryGetValue(type, out byte[]? expected))
            {
                if (type is RoomFxType.Rain or RoomFxType.Fog)
                {
                    int preservedMask = type == RoomFxType.Rain ? 0xe3ff : 0xc3ff;
                    ReadOnlySpan<byte> compiled = stock.ResolveBytes(type);
                    for (int offset = 0; offset < expected.Length; offset += 2)
                        AssertEqual(BinaryPrimitives.ReadUInt16LittleEndian(expected.AsSpan(offset)) & preservedMask,
                            BinaryPrimitives.ReadUInt16LittleEndian(compiled[offset..]) & preservedMask,
                            "Other atmosphere fields preserve their original values during separate review");
                }
                if (type == RoomFxType.Spores)
                {
                    ReadOnlySpan<byte> compiled = stock.ResolveBytes(type);
                    for (int offset = 0; offset < expected.Length; offset += 2)
                        AssertEqual(BinaryPrimitives.ReadUInt16LittleEndian(expected.AsSpan(offset)) & 0x3ff,
                            BinaryPrimitives.ReadUInt16LittleEndian(compiled[offset..]) & 0x3ff,
                            "Retained spore character composition matches original");
                }
                ReadOnlySpan<byte> actual = edited.ResolveBytes(type);
                AssertEqual(expected.Length, actual.Length, "Edited page retains full transfer length");
                for (int offset = 0; offset < actual.Length; offset += 2)
                    AssertEqual(markers[type], BinaryPrimitives.ReadUInt16LittleEndian(actual[offset..]),
                        "Each named edited page remains independent of stock and other pages");
            }
            else
            {
                AssertThrows<InvalidDataException>(() => stock.Resolve(type), "Unsupported stock type rejects");
                AssertThrows<InvalidDataException>(() => edited.Resolve(type), "Unsupported edited type rejects");
            }
        }
        foreach (RoomFxType type in OriginalFxTilemapTypes)
        {
            var cells = document.Pages[type.ToString()];
            document.Pages.Remove(type.ToString());
            document.Pages.Add("Unknown", cells);
            AssertThrows<InvalidDataException>(() => Load(), "No named page may be replaced by an unknown sixth key");
            document.Pages.Remove("Unknown");
            document.Pages.Add(type.ToString(), cells);
        }
    }

    /// <summary>Reads one cartridge byte for the native FX tilemap page oracle.</summary>
    private static byte FxTilemapPageDispatchRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
