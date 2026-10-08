using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Import/reference-only decoding of native PLM placements. The resulting typed
/// population can be consumed by Core without a cartridge-capable address space.
/// </summary>
internal static class RoomPlmPopulationImporter
{
    public static RoomPlmPopulationDefinition Read(ISnesAddressSpace source, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source is not IImportCartridgeSource cartridge)
            throw new ArgumentException("PLM import requires an explicit cartridge-import source.", nameof(source));
        var records = new List<RoomPlmPlacement>();
        ushort cursor = pointer;
        for (int index = 0; index < RoomPlmPopulationFormat.MaximumParserIterations; index++)
        {
            ushort header = Word(0x8f, cursor);
            if (header == 0)
                return new RoomPlmPopulationDefinition(pointer, records);
            var metadata = new RoomPlmHeaderDefinition(header,
                Word(0x84, header), Word(0x84, unchecked((ushort)(header + 2))));
            ushort argument = Word(0x8f, unchecked((ushort)(cursor + 4)));
            byte[] scroll = header == RoomPlmHeaders.ScrollTrigger ? Scroll(argument) : [];
            RoomPlmDynamicCollectibleGraphic? graphic = null;
            if (RoomPlmSystem.TryIdentifyPermanentCollectible(header, out var kind, out _) &&
                kind >= InWorldCollectibleKind.Bombs)
            {
                ushort instruction = metadata.InitialInstruction;
                if (Word(0x84, instruction) != RoomPlmInstructionCodes.LoadItemGraphics)
                    throw new InvalidDataException($"Imported collectible $84:{header:X4} lacks its item-graphics instruction.");
                ushort graphicsPointer = Word(0x84, unchecked((ushort)(instruction + 2)));
                var palettes = new byte[8];
                var tiles = new byte[0x100];
                for (int offset = 0; offset < palettes.Length; offset++)
                    palettes[offset] = Byte(0x84, unchecked((ushort)(instruction + 4 + offset)));
                for (int offset = 0; offset < tiles.Length; offset++)
                    tiles[offset] = Byte(0x89, unchecked((ushort)(graphicsPointer + offset)));
                graphic = new RoomPlmDynamicCollectibleGraphic(kind, graphicsPointer, palettes, tiles);
            }
            records.Add(new RoomPlmPlacement(metadata,
                Byte(0x8f, unchecked((ushort)(cursor + 2))),
                Byte(0x8f, unchecked((ushort)(cursor + 3))), argument, scroll, graphic));
            cursor = unchecked((ushort)(cursor + RoomPlmPopulationFormat.RecordByteCount));
        }
        // Preserve the loader's bounded parser: a terminator beyond record 256
        // is not accepted merely because the importer could keep reading.
        throw new InvalidDataException($"Imported PLM population $8F:{pointer:X4} has no bounded zero terminator.");

        byte Byte(byte bank, ushort offset) => cartridge.ReadCartridgeByte(bank << 16 | offset);
        ushort Word(byte bank, ushort offset) =>
            (ushort)(Byte(bank, offset) | Byte(bank, unchecked((ushort)(offset + 1))) << 8);
        byte[] Scroll(ushort argument)
        {
            var pairs = new List<byte>();
            for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
            {
                byte cell = Byte(0x8f, unchecked((ushort)(argument + pairs.Count)));
                pairs.Add(cell);
                if ((cell & 0x80) != 0)
                    return pairs.ToArray();
                pairs.Add(Byte(0x8f, unchecked((ushort)(argument + pairs.Count))));
            }
            throw new InvalidDataException($"Imported scroll program $8F:{argument:X4} has no bounded negative terminator.");
        }
    }
}
