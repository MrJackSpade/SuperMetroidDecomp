using System.Text.Json;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Work Robot palette-cycle colors and author override handling.</summary>
internal static class EnemyTileArtworkWorkRobotPaletteVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Compares 24 retail RGB5 words, persistent single-color override, and invalid document rejection.</summary>
    internal static void VerifyInstalledWorkRobotPaletteCycle(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileSourceArtwork stock)
    {
        WorkRobotPaletteCycle native = stock.WorkRobotPaletteCycle ??
            throw new InvalidDataException("Installed enemy art has no Work Robot palette cycle.");
        for (int record = 0; record < WorkRobotPaletteTimingDefinitions.RecordCount; record++)
        for (int color = 0; color < WorkRobotPaletteDefinitions.ColorCount; color++)
        {
            int address = EnemyRomTablePointers.WorkRobot.PaletteAnimationRecords +
                record * WorkRobotPaletteTimingDefinitions.RecordByteCount +
                color * sizeof(ushort);
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), address),
                native.Resolve(record, color),
                $"installed Work Robot frame {record} color {color} preserves ROM RGB5");
        }

        string file = Path.Combine(stockDirectory, WorkRobotPaletteCycleFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        WorkRobotPaletteCycleDocument visual =
            JsonSerializer.Deserialize<WorkRobotPaletteCycleDocument>(stockJson,
                CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Work Robot palette JSON is null.");
        PaletteRgb5 prior = visual.Frames[2][1];
        visual.Frames[2][1] = prior with
        {
            Red = prior.Red == 31 ? 30 : prior.Red + 1,
        };
        string overrides = Path.Combine(stockDirectory, "work-robot-cycle-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, WorkRobotPaletteCycleFormat.FileName);
        File.WriteAllBytes(overrideFile, WorkRobotPaletteCycleFile.Write(visual));
        WorkRobotPaletteCycle cycle = EnemyTileArtworkFiles.Open(stockDirectory, overrides).WorkRobotPaletteCycle();
        AssertTrue(cycle.Resolve(2, 1) != native.Resolve(2, 1),
            "Work Robot color override changes the selected RGB5 word");
        AssertEqual(cycle.Resolve(2, 1),
            EnemyTileArtworkFiles.Open(stockDirectory, overrides).WorkRobotPaletteCycle().Resolve(2, 1),
            "Work Robot color override persists across reload");
        for (int record = 0; record < WorkRobotPaletteTimingDefinitions.RecordCount; record++)
        for (int color = 0; color < WorkRobotPaletteDefinitions.ColorCount; color++)
            if (record != 2 || color != 1)
                AssertEqual(native.Resolve(record, color), cycle.Resolve(record, color),
                    $"Work Robot edit leaves record {record} color {color} unchanged");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Open(stockDirectory, overrides).WorkRobotPaletteCycle(),
            "invalid Work Robot color override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1,\"frames\":[]}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Open(stockDirectory, overrides).WorkRobotPaletteCycle(),
            "duplicate Work Robot color property fails loudly");
        visual.Frames[2][1] = prior with { Green = 32 };
        AssertThrows<InvalidDataException>(() => WorkRobotPaletteCycleFile.Read(new MemoryStream(WorkRobotPaletteCycleFile.Write(visual))),
            "Work Robot RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);

        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Open(stockDirectory, null).WorkRobotPaletteCycle(),
            "corrupt stock Work Robot colors fail loudly where it is read");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Work Robot colors: 24 native RGB5 words, persistent override and strict stock/override failures pass.");
    }
}
