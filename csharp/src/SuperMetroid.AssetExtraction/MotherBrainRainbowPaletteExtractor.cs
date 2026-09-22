using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installs bank-$AD rainbow and grey-transition colors without copying native control tables.</summary>
internal static class MotherBrainRainbowPaletteExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        using var json = new MemoryStream();
        MotherBrainRainbowPalettePresentation.Write(json, new MotherBrainRainbowPaletteDocument
        {
            Version = MotherBrainRainbowPaletteFormat.Version,
            Rainbow = ReadTable(MotherBrainRainbowPaletteRomData.PointerTable,
                MotherBrainRainbowPaletteFormat.RainbowFrameCount,
                MotherBrainRainbowPaletteRomData.ColorCount,
                MotherBrainRainbowPaletteRomData.ColorCount, false),
            ToGrey = ReadTable(MotherBrainDrainedPaletteRomData.ToGreyTable,
                MotherBrainRainbowPaletteFormat.GreyFrameCount,
                MotherBrainDrainedPaletteRomData.DrainedColors,
                MotherBrainDrainedPaletteRomData.BackLegCount, true),
            FromGrey = ReadTable(MotherBrainDrainedPaletteRomData.FromGreyTable,
                MotherBrainRainbowPaletteFormat.GreyFrameCount,
                MotherBrainDrainedPaletteRomData.RevivalColors,
                MotherBrainDrainedPaletteRomData.BackLegCount, true),
            Normal = ReadFrame(MotherBrainRainbowPaletteRomData.NormalBrainSource,
                MotherBrainRainbowPaletteRomData.ColorCount,
                MotherBrainRainbowPaletteRomData.ColorCount, false,
                MotherBrainRainbowPaletteRomData.NormalSecondarySource),
        });
        return json.ToArray();

        MotherBrainRainbowPaletteFrameDocument[] ReadTable(int table, int frames,
            int bodyCount, int legCount, bool trailing)
        {
            var values = new MotherBrainRainbowPaletteFrameDocument[frames];
            for (int frame = 0; frame < frames; frame++)
            {
                int pointerAddress = table + frame * sizeof(ushort);
                ushort pointer = ReadWord(pointerAddress);
                if (pointer == 0)
                    throw new InvalidDataException($"Mother Brain palette table ${table:X6} ends before frame {frame}.");
                values[frame] = ReadFrame(MotherBrainRainbowPaletteRomData.SourceBank | pointer,
                    bodyCount, legCount, trailing);
            }
            if (ReadWord(table + frames * sizeof(ushort)) != 0)
                throw new InvalidDataException($"Mother Brain palette table ${table:X6} lacks its native terminator.");
            return values;
        }

        MotherBrainRainbowPaletteFrameDocument ReadFrame(int source, int bodyCount,
            int legCount, bool trailing, int? legSource = null) => new()
        {
            Body = ReadColors(source, bodyCount),
            BackLegs = ReadColors(legSource ?? source + bodyCount * sizeof(ushort), legCount),
            TrailingColor = trailing ? ReadColor(source + (bodyCount + legCount) * sizeof(ushort)) : null,
        };

        PaletteRgb5[] ReadColors(int source, int count)
        {
            var values = new PaletteRgb5[count];
            for (int color = 0; color < count; color++)
                values[color] = ReadColor(source + color * sizeof(ushort));
            return values;
        }

        PaletteRgb5 ReadColor(int address)
        {
            ushort word = ReadWord(address);
            return new PaletteRgb5
            {
                Red = word & 31,
                Green = word >> 5 & 31,
                Blue = word >> 10 & 31,
            };
        }

        ushort ReadWord(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
    }
}
