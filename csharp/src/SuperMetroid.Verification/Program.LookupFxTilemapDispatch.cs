using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyFxTilemapPageDispatch(ISnesAddressSpace rom, RoomFxLayer3TilemapCatalog stock)
    {
        var native = new Dictionary<RoomFxType, byte[]>();
        var markers = new Dictionary<RoomFxType, ushort>();
        var document = new RoomFxLayer3TilemapDocument { Version = 1, Pages = [] };
        for (int index = 0; index < OriginalFxTilemapTypes.Length; index++)
        {
            RoomFxType type = OriginalFxTilemapTypes[index];
            int source = 0x8a0000 | ReadVerificationWord(rom, 0x83abf0 + (ushort)type);
            var bytes = new byte[0x840];
            for (int offset = 0; offset < bytes.Length; offset++) bytes[offset] = rom.ReadByte(source + offset);
            native.Add(type, bytes);
            ushort marker = (ushort)(index + 1);
            markers.Add(type, marker);
            document.Pages.Add(type.ToString(), Enumerable.Repeat(TilemapContractCell(marker), 32 * 33).ToArray());
        }
        RoomFxLayer3TilemapCatalog Load() => RoomFxLayer3TilemapCatalog.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document)));
        var edited = Load();
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            var type = (RoomFxType)value;
            if (native.TryGetValue(type, out byte[]? expected))
            {
                if (type is RoomFxType.Rain or RoomFxType.Fog)
                    AssertTrue(expected.AsSpan().SequenceEqual(stock.Resolve(type).Span), "Every original non-liquid page byte selects correctly");
                if (type == RoomFxType.Spores)
                {
                    ReadOnlySpan<byte> compiled = stock.Resolve(type).Span;
                    for (int offset = 0; offset < expected.Length; offset += 2)
                        AssertEqual(BinaryPrimitives.ReadUInt16LittleEndian(expected.AsSpan(offset)) & 0x3ff,
                            BinaryPrimitives.ReadUInt16LittleEndian(compiled[offset..]) & 0x3ff,
                            "Retained spore character composition matches original");
                }
                ReadOnlySpan<byte> actual = edited.Resolve(type).Span;
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
}
