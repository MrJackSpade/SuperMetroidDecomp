using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>Confirms calculated Dachora speed and shine colors match native
/// palettes and that a single edited cell stays sparse.</summary>
internal static class LookupDachoraColorsVerification
{
    /// <summary>
    /// #1165: Dachora's speed and shine frames calculate from the normal colors. Confirms every
    /// native word, that the stock install stores no speed/shine colors, and that every
    /// single-cell edit resolves exactly while storing only that cell.
    /// </summary>
    internal static void VerifyLookupDachoraColors(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupDachoraColorsRomByte(rom, address), LookupDachoraColorsRomByte(rom, address + 1));
        ushort[] Row(int address) => Enumerable.Range(0, 16).Select(color => Word(address + color * 2)).ToArray();
        ushort[] normal = Row(EnemySpeciesSourceDefinitions.Dachora.DefaultColors);
        ushort[][] speed = Enumerable.Range(0, 4).Select(frame => Row(EnemySpeciesSourceDefinitions.Dachora.SpeedColors + frame * EnemySpeciesSourceDefinitions.Dachora.ImageBytes)).ToArray();
        ushort[][] shine = Enumerable.Range(0, 4).Select(frame => Row(EnemySpeciesSourceDefinitions.Dachora.ShineColors + frame * EnemySpeciesSourceDefinitions.Dachora.ImageBytes)).ToArray();

        byte[] source = DachoraColorExtractor.Extract(rom);
        var stock = DachoraColorFile.Read(new MemoryStream(source));
        AssertEqual(0, stock.selection.SpeedEdits.Count + stock.selection.ShineEdits.Count,
            "Stock Dachora speed and shine frames store no colors beyond the calculation");
        Check(stock, normal, speed, shine);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        foreach (DachoraPalettePhase phase in new[] { DachoraPalettePhase.Speed, DachoraPalettePhase.Shine })
        for (int frame = 0; frame < 4; frame++)
        for (int color = 0; color < 16; color++)
        {
            var document = RequiredJson.Deserialize<DachoraColorDocument>(source, options);
            PaletteRgb5[] row = phase == DachoraPalettePhase.Speed ? document.Speed[frame] : document.Shine[frame];
            row[color] = row[color] with { Red = row[color].Red ^ 1 };
            var edited = DachoraColorFile.Read(new MemoryStream(DachoraColorFile.Write(document)));
            ushort[][] expectedSpeed = speed.Select(values => (ushort[])values.Clone()).ToArray();
            ushort[][] expectedShine = shine.Select(values => (ushort[])values.Clone()).ToArray();
            (phase == DachoraPalettePhase.Speed ? expectedSpeed : expectedShine)[frame][color] ^= 1;
            Check(edited, normal, expectedSpeed, expectedShine);
            AssertEqual(1, edited.selection.SpeedEdits.Count + edited.selection.ShineEdits.Count,
                $"Dachora {phase} frame {frame} color {color} edit stores only that cell");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(DachoraPalettePhase.Speed, 4, 0), "Speed frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(DachoraPalettePhase.Default, 1, 0), "Default frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(DachoraPalettePhase.Shine, 0, 16), "Color bound");
        Console.WriteLine("Dachora colors: all 144 native words, 128 calculated speed/shine colors with no stored stock values, and all 128 single-cell edits pass.");

        static void Check(DachoraColorCatalog catalog, ushort[] normal, ushort[][] speed, ushort[][] shine)
        {
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(normal[color], catalog.Resolve(DachoraPalettePhase.Default, 0, color), $"Normal color {color}");
                for (int frame = 0; frame < 4; frame++)
                {
                    AssertEqual(speed[frame][color], catalog.Resolve(DachoraPalettePhase.Speed, frame, color), $"Speed {frame}/{color}");
                    AssertEqual(shine[frame][color], catalog.Resolve(DachoraPalettePhase.Shine, frame, color), $"Shine {frame}/{color}");
                }
            }
        }
    }

    /// <summary>Reads one cartridge byte for the exact native palette oracle.</summary>
    private static byte LookupDachoraColorsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
