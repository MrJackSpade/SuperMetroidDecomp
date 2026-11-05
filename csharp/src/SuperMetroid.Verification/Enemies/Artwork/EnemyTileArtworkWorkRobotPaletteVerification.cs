using System.Text.Json;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Work Robot palette-cycle colors and malformed-channel rejection.</summary>
internal static class EnemyTileArtworkWorkRobotPaletteVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Compares 24 retail RGB5 words and invalid document rejection.</summary>
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
        visual.Frames[2][1] = prior with { Green = 32 };
        AssertThrows<InvalidDataException>(() => WorkRobotPaletteCycleFile.Read(new MemoryStream(WorkRobotPaletteCycleFile.Write(visual))),
            "Work Robot RGB5 channel outside five-bit precision is rejected");
        Console.WriteLine("  Work Robot colors: 24 native RGB5 words and channel rejection pass.");
    }
}
