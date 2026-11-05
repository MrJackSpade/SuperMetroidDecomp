using SuperMetroid.Verification.Harness;
using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Fx;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Fx.LookupFxTilemapSourcesVerification;
using static SuperMetroid.Verification.Rooms.TilemapJsonContractsVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks FX layer-3 page dispatch for native sources, document page keys, and unsupported identities.</summary>
internal static class LookupFxTilemapDispatchVerification
{
    /// <summary>Compares native page bytes, rejects unsupported page identities, and rejects missing or unknown page keys.</summary>
    internal static void VerifyFxTilemapPageDispatch(ISnesAddressSpace rom, RoomFxLayer3Pages stock)
    {
        var native = new Dictionary<RoomFxType, byte[]>();
        var document = new RoomFxLayer3TilemapDocument { Version = 1, Pages = [] };
        for (int index = 0; index < OriginalFxTilemapTypes.Length; index++)
        {
            RoomFxType type = OriginalFxTilemapTypes[index];
            int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Word(type));
            var bytes = new byte[0x840];
            for (int offset = 0; offset < bytes.Length; offset++) bytes[offset] = FxTilemapPageDispatchRomByte(rom, source + offset);
            native.Add(type, bytes);
            document.Pages.Add(type.ToString(), Enumerable.Repeat(TilemapContractCell((ushort)(index + 1)), 32 * 33).ToArray());
        }
        RoomFxLayer3Pages Load() => RoomFxLayer3Pages.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document)));
        native.Add(RoomFxType.TourianEntranceStatue, native[RoomFxType.Water]);
        // A page's resolution is a pure function of the immutable stock pages and its identity, so the identity domain is
        // swept across the cores (#1532).
        ParallelSweep.For(0, ushort.MaxValue + 1, value =>
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
            }
            else AssertThrows<InvalidDataException>(() => stock.Resolve(type), "Unsupported stock type rejects");
        });
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
