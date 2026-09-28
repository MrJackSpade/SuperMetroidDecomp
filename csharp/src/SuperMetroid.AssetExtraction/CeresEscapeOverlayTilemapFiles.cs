using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports fixed Ceres warning tile words without exposing DMA control metadata.</summary>
internal static class CeresEscapeOverlayTilemapFiles
{
    internal static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var words = new Dictionary<string, ushort[]>();
        foreach (CeresEscapeOverlayTilemapDefinition page in
                 CeresEscapeOverlayTilemapDefinitions.All)
        {
            var source = new ushort[page.WordCount];
            for (int index = 0; index < source.Length; index++)
            {
                int address = page.SourceAddress + index * sizeof(ushort);
                source[index] = (ushort)(bus.ReadByte(address) |
                    bus.ReadByte(address + 1) << 8);
            }
            words.Add(page.Name, source);
        }
        byte[] json = CeresEscapeOverlayTilemapCatalog.Write(
            new CeresEscapeOverlayTilemapDocument
            {
                Version = CeresEscapeOverlayTilemapDefinitions.Version,
                Pages = words,
            });
        CeresEscapeOverlayTilemapCatalog roundtrip =
            CeresEscapeOverlayTilemapCatalog.Load(
                new MemoryStream(json, writable: false));
        foreach (CeresEscapeOverlayTilemapDefinition page in
                 CeresEscapeOverlayTilemapDefinitions.All)
        {
            if (!roundtrip.TryResolve(page.SourceAddress,
                    page.WordCount * sizeof(ushort), out ReadOnlyMemory<byte> data))
                throw new InvalidDataException(
                    $"Ceres escape tilemap {page.Name} did not reload.");
            for (int offset = 0; offset < data.Length; offset++)
                if (data.Span[offset] != bus.ReadByte(page.SourceAddress + offset))
                    throw new InvalidDataException(
                        $"Ceres escape tilemap {page.Name} changed byte {offset:X2} during export.");
        }
        return json;
    }
}
