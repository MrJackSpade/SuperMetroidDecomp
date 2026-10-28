using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// Import/reference-only decoding of native PLM placements. The resulting typed
/// population can be consumed by Core without a cartridge-capable address space.
/// </summary>
internal static class RoomPlmPopulationImporter
{
    /// <summary>Imports bounded PLM placements, header words and scroll programs.</summary>
    public static RoomPlmPopulation Read(ISnesAddressSpace source, ushort pointer)
    {
        Ensure.NotNull(source);
        if (source is not IImportCartridgeSource cartridge)
            throw new ArgumentException("PLM import requires an explicit cartridge-import source.", nameof(source));
        var records = new List<RoomPlmPlacement>();
        ushort cursor = pointer;
        for (int index = 0; index < RoomPlmPopulationFormat.MaximumParserIterations; index++)
        {
            ushort headerWord = Word(0x8f, cursor);
            if (headerWord == 0)
                return new RoomPlmPopulation($"$8F:{pointer:X4}", records);
            ushort argument = Word(0x8f, unchecked((ushort)(cursor + 4)));
            byte blockX = Byte(0x8f, unchecked((ushort)(cursor + 2)));
            byte blockY = Byte(0x8f, unchecked((ushort)(cursor + 3)));
            // PlmHeaderId's values are arbitrary (#1414): a word no member names cannot enter the population.
            if (!PlmNativeWords.Headers.TryDecode(headerWord, out PlmHeaderId header))
                throw new InvalidDataException(
                    $"PLM population $8F:{pointer:X4} record {index} names unlabelled header $84:{headerWord:X4} " +
                    $"at block ({blockX},{blockY}) with argument ${argument:X4}.");
            var metadata = new RoomPlmHeaderDefinition(header);
            byte[] scroll = header == PlmHeaderId.ScrollTrigger ? Scroll(argument) : [];
            records.Add(new RoomPlmPlacement(metadata,
                new(blockX, blockY), PlmRoomArgumentWords.Decode(header, argument), scroll,
                DraygonCannonPlms.IsDraygonCannonHeader(header) ? DraygonCannonControlWordAddresses.Decode(argument) : null));
            cursor = unchecked((ushort)(cursor + RoomPlmPopulationFormatConstants.RecordByteCount));
        }
        // Preserve the loader's bounded parser: a terminator beyond record 256
        // is not accepted merely because the importer could keep reading.
        throw new InvalidDataException($"Imported PLM population $8F:{pointer:X4} has no bounded zero terminator.");

        byte Byte(byte bank, ushort offset) => cartridge.ReadCartridgeByte(((int)new SnesAddress(bank, offset)));
        ushort Word(byte bank, ushort offset) =>
            LittleEndianBytes.Word(Byte(bank, offset), Byte(bank, unchecked((ushort)(offset + 1))));
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
