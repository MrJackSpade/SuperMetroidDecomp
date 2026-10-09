using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports all six contiguous bank-$8A room-FX BG3 pages as named tile cells.</summary>
public static class RoomFxLayer3TilemapExtractor
{
    /// <summary>Builds the six named room-FX BG3 pages after checking their native tilemap-pointer identities.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying the FX pointer table and bank-$8A tilemap sources.</param>
    /// <returns>New UTF-8 JSON bytes containing ordered 32x33 pages for lava, acid, water, spores, rain, and fog, with tile references and BG attributes.</returns>
    /// <remarks>Liquid words come from compiled stock definitions; spore attributes and calculated rain/fog fields are combined with imported tilemap data. Scrolling and FX timing are not exported.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A native pointer does not select its compiled source or the assembled tilemap document fails validation.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var pages = new Dictionary<string, RoomBackgroundTilemapCell[]>();
        foreach (RoomFxType type in RoomFxLayer3TilemapFormat.Types)
        {
            int source = RoomFxLayer3TilemapFormat.SourceAddress(type);
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                RoomFxRomData.Tables.Layer3TilemapPointers + (int)type);
            if ((RoomFxRomData.Banks.Tilemaps | pointer) != source)
                throw new InvalidDataException(
                    $"Room-FX {type} tilemap pointer ${pointer:X4} does not select ${source:X6}.");
            byte[] native = type is RoomFxType.Lava or RoomFxType.Acid or RoomFxType.Water
                ? RoomFxLiquidTilemapDefinitions.CreateTransfer(type)
                : RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), source,
                    RoomFxLayer3TilemapFormat.PageByteCount);
            var cells = new RoomBackgroundTilemapCell[RoomFxLayer3TilemapFormat.CellsPerPage];
            for (int index = 0; index < cells.Length; index++)
            {
                ushort raw = BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(index * sizeof(ushort)));
                if (type == RoomFxType.Spores)
                    raw = (ushort)((raw & 0x03ff) | RoomFxSporeTilemapDefinitions.Attributes(index));
                if (type is RoomFxType.Rain or RoomFxType.Fog)
                    raw = (ushort)((raw & ~RoomFxAtmosphereTilemapDefinitions.CalculatedMask(type)) |
                        RoomFxAtmosphereTilemapDefinitions.CalculatedFields(type, index));
                var word = new SnesBgTilemapWord(raw);
                cells[index] = new RoomBackgroundTilemapCell
                {
                    TileColumn = word.CharacterIndex % RoomBackgroundTilemapFormat.TileColumns,
                    TileRow = word.CharacterIndex / RoomBackgroundTilemapFormat.TileColumns,
                    Palette = word.PaletteIndex,
                    Priority = word.HasPriority,
                    FlipX = word.FlipHorizontally,
                    FlipY = word.FlipVertically,
                };
            }
            pages.Add(type.ToString(), cells);
        }
        using var output = new MemoryStream();
        RoomFxLayer3TilemapCatalog.Write(output, new RoomFxLayer3TilemapDocument
        {
            Version = RoomFxLayer3TilemapFormat.Version,
            Pages = pages,
        });
        return output.ToArray();
    }
}
