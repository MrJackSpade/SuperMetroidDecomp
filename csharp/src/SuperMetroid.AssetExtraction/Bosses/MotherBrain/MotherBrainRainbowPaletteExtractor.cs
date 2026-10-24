using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Installs bank-$AD rainbow and grey-transition colors without copying native control tables.</summary>
[CpuBusBoundary("codec: ROM bank-$AD rainbow and grey-transition colors → Mother Brain rainbow palettes")]
internal static class MotherBrainRainbowPaletteExtractor
{
    /// <summary>Imports rainbow, drained, revival, fake-death, and beam colors
    /// into editable RGB5 sequences while checking native table terminators.</summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        using var json = new MemoryStream();
        MotherBrainRainbowPalettePresentation.Write(json, new MotherBrainRainbowPaletteDocument
        {
            Version = (int)MotherBrainRainbowPaletteVersion.Current,
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
            FakeDeathToGrey = ReadFakeDeathToGrey(),
            Normal = ReadFrame(MotherBrainRainbowPaletteRomData.NormalBrainSource,
                MotherBrainRainbowPaletteRomData.ColorCount,
                MotherBrainRainbowPaletteRomData.ColorCount, false,
                MotherBrainRainbowPaletteRomData.NormalSecondarySource),
            BeamInitial = PaletteRgb5.From(MotherBrainBeamRomData.InitialColor),
            BeamCycle = ReadBeamCycle(),
        });
        return json.ToArray();

        MotherBrainRainbowPaletteFrameDocument[] ReadTable(int table, int frames,
            int bodyCount, int legCount, bool trailing)
        {
            var values = new MotherBrainRainbowPaletteFrameDocument[frames];
            for (int frame = 0; frame < frames; frame++)
            {
                int pointerAddress = table + frame * sizeof(ushort);
                ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(pointerAddress);
                if (pointer == 0)
                    throw new InvalidDataException($"Mother Brain palette table ${table:X6} ends before frame {frame}.");
                values[frame] = ReadFrame(MotherBrainRainbowPaletteRomData.SourceBank | pointer,
                    bodyCount, legCount, trailing);
            }
            if (CartridgeImportSource.Require(bus).ReadCartridgeWord((table + frames * sizeof(ushort))) != 0)
                throw new InvalidDataException($"Mother Brain palette table ${table:X6} lacks its native terminator.");
            return values;
        }

        PaletteRgb5[][] ReadFakeDeathToGrey()
        {
            var frames = new PaletteRgb5[MotherBrainFakeDeathPaletteRomData.FrameCount][];
            for (int frame = 0; frame < frames.Length; frame++)
            {
                int address = MotherBrainFakeDeathPaletteRomData.ToGreyPointerTable +
                    frame * sizeof(ushort);
                ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
                if (pointer == 0)
                    throw new InvalidDataException($"Mother Brain fake-death fade ends before frame {frame}.");
                frames[frame] = ReadColors(MotherBrainRainbowPaletteRomData.SourceBank | pointer,
                    MotherBrainFakeDeathPaletteRomData.ColorCount);
            }
            if (CartridgeImportSource.Require(bus).ReadCartridgeWord((MotherBrainFakeDeathPaletteRomData.ToGreyPointerTable +
                frames.Length * sizeof(ushort))) != 0)
                throw new InvalidDataException("Mother Brain fake-death fade lacks its native terminator.");
            return frames;
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
            ushort word = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
            return ToColor(word);
        }

        PaletteRgb5[] ReadBeamCycle()
        {
            var colors = new PaletteRgb5[MotherBrainRainbowPaletteFormat.BeamCycleColorCount];
            for (int index = 0; index < colors.Length; index++)
            {
                ushort word = CartridgeImportSource.Require(bus).ReadCartridgeWord((MotherBrainBeamRomData.ColorTable +
                    index * MotherBrainBeamRomData.ColorStride));
                if (!Bgr555.IsColorWord(word))
                    throw new InvalidDataException($"Mother Brain beam cycle ends before color {index}.");
                colors[index] = ToColor(word);
            }
            if (CartridgeImportSource.Require(bus).ReadCartridgeWord((MotherBrainBeamRomData.ColorTable +
                colors.Length * MotherBrainBeamRomData.ColorStride)) != ushort.MaxValue)
                throw new InvalidDataException("Mother Brain beam cycle lacks its native signed terminator.");
            return colors;
        }

        static PaletteRgb5 ToColor(ushort word) => new()
        {
            Red = word & 31,
            Green = word >> 5 & 31,
            Blue = word >> 10 & 31,
        };
    }
}
