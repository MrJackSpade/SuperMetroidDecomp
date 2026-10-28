using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Installs bank-$AD rainbow and grey-transition colors without copying native control tables.</summary>
internal static class MotherBrainRainbowPaletteExtractor
{
    /// <summary>Imports rainbow, drained, revival, fake-death, and beam colors
    /// into editable RGB5 sequences while checking native table terminators.</summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        using var json = new MemoryStream();
        MotherBrainRainbowPaletteFile.Write(json, new MotherBrainRainbowPaletteDocument
        {
            Version = (int)MotherBrainRainbowPaletteVersion.Current,
            Rainbow = ReadTable(MotherBrainSourceDefinitions.Palettes.RainbowPointerTable,
                MotherBrainRainbowPaletteFormat.RainbowFrameCount,
                MotherBrainRainbowPaletteDefinitions.ColorCount,
                MotherBrainRainbowPaletteDefinitions.ColorCount, false),
            ToGrey = ReadTable(MotherBrainSourceDefinitions.Palettes.DrainedToGreyPointerTable,
                MotherBrainRainbowPaletteFormat.GreyFrameCount,
                MotherBrainDrainedPaletteDefinitions.DrainedColors,
                MotherBrainDrainedPaletteDefinitions.BackLegCount, true),
            FromGrey = ReadTable(MotherBrainSourceDefinitions.Palettes.RevivalFromGreyPointerTable,
                MotherBrainRainbowPaletteFormat.GreyFrameCount,
                MotherBrainDrainedPaletteDefinitions.RevivalColors,
                MotherBrainDrainedPaletteDefinitions.BackLegCount, true),
            FakeDeathToGrey = ReadFakeDeathToGrey(),
            Normal = ReadFrame(MotherBrainSourceDefinitions.Palettes.NormalBrainColors,
                MotherBrainRainbowPaletteDefinitions.ColorCount,
                MotherBrainRainbowPaletteDefinitions.ColorCount, false,
                MotherBrainSourceDefinitions.Palettes.NormalRearLegColors),
            BeamInitial = PaletteRgb5.From(MotherBrainBeamDefinitions.InitialColor),
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
                values[frame] = ReadFrame(MotherBrainSourceDefinitions.Palettes.PaletteBank | pointer,
                    bodyCount, legCount, trailing);
            }
            if (CartridgeImportSource.Require(bus).ReadCartridgeWord((table + frames * sizeof(ushort))) != 0)
                throw new InvalidDataException($"Mother Brain palette table ${table:X6} lacks its native terminator.");
            return values;
        }

        PaletteRgb5[][] ReadFakeDeathToGrey()
        {
            var frames = new PaletteRgb5[MotherBrainFakeDeathPaletteDefinitions.FrameCount][];
            for (int frame = 0; frame < frames.Length; frame++)
            {
                int address = MotherBrainSourceDefinitions.Palettes.FakeDeathToGreyPointerTable +
                    frame * sizeof(ushort);
                ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
                if (pointer == 0)
                    throw new InvalidDataException($"Mother Brain fake-death fade ends before frame {frame}.");
                frames[frame] = ReadColors(MotherBrainSourceDefinitions.Palettes.PaletteBank | pointer,
                    MotherBrainFakeDeathPaletteDefinitions.ColorCount);
            }
            if (CartridgeImportSource.Require(bus).ReadCartridgeWord((MotherBrainSourceDefinitions.Palettes.FakeDeathToGreyPointerTable +
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
                ushort word = CartridgeImportSource.Require(bus).ReadCartridgeWord((MotherBrainSourceDefinitions.Beam.ColorTable +
                    index * MotherBrainSourceDefinitions.Beam.ColorEntryBytes));
                if (!RgbColor.IsColorWord(word))
                    throw new InvalidDataException($"Mother Brain beam cycle ends before color {index}.");
                colors[index] = ToColor(word);
            }
            if (CartridgeImportSource.Require(bus).ReadCartridgeWord((MotherBrainSourceDefinitions.Beam.ColorTable +
                colors.Length * MotherBrainSourceDefinitions.Beam.ColorEntryBytes)) != ushort.MaxValue)
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
