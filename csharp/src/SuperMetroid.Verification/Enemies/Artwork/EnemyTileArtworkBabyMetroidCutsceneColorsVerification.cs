using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Baby Metroid cutscene colors and fade palettes in live CGRAM.</summary>
internal static class EnemyTileArtworkBabyMetroidCutsceneColorsVerification
{
    /// <summary>$AD:E8E2, the seven fade-image pointers the cartridge selects through; index zero is never displayed.</summary>
    private const int FadePointerTable = 0xade8e2;

    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Compares native colors and selectors, edits one channel, and checks strict override failures.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyInstalledBabyMetroidCutsceneColors(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        BabyMetroidCutsceneColorCatalog native = stock.Bosses.BabyMetroidCutsceneColors ??
            throw new InvalidDataException("Installed enemy art has no cutscene Baby colors.");
        for (int color = 0; color < BabyMetroidCutsceneColorDefinitions.InitialColorCount; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    MotherBrainSourceDefinitions.BabyMetroid.InitialColors + color * 2),
                native.InitialColor(color),
                $"installed cutscene Baby initial color {color}");
        for (int index = 1; index <= BabyMetroidCutsceneColorDefinitions.FadeFrameCount; index++)
        {
            int source = MotherBrainSourceDefinitions.BabyMetroid.FadeColors(index);
            AssertEqual(unchecked((ushort)source),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    FadePointerTable + index * 2),
                $"native cutscene Baby fade selector {index}");
            for (int color = 0; color < BabyMetroidCutsceneColorDefinitions.FadeColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), source + color * 2),
                    native.FadeColor(index, color),
                    $"installed cutscene Baby fade {index} color {color}");
        }

        string file = Path.Combine(stockDirectory,
            BabyMetroidCutsceneColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        BabyMetroidCutsceneColorDocument visual =
            JsonSerializer.Deserialize<BabyMetroidCutsceneColorDocument>(stockJson,
                CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock cutscene Baby color JSON is null.");
        visual.Initial[5] = ChangeRed(visual.Initial[5]);
        foreach (PaletteRgb5[] frame in visual.Fade)
            frame[5] = ChangeRed(frame[5]);
        string overrides = Path.Combine(stockDirectory,
            "baby-metroid-cutscene-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides,
            BabyMetroidCutsceneColorFormat.FileName);
        File.WriteAllBytes(overrideFile, BabyMetroidCutsceneColorFile.Write(visual));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        BabyMetroidCutsceneColorCatalog colors = edited.Bosses.BabyMetroidCutsceneColors ??
            throw new InvalidDataException("Edited enemy art has no cutscene Baby colors.");
        AssertTrue(native.InitialColor(5) != colors.InitialColor(5),
            "cutscene Baby initial edit takes effect");
        AssertEqual(native.InitialColor(4), colors.InitialColor(4),
            "cutscene Baby initial adjacent color stays native");
        for (int index = 1; index <= BabyMetroidCutsceneColorDefinitions.FadeFrameCount; index++)
        {
            AssertTrue(native.FadeColor(index, 5) != colors.FadeColor(index, 5),
                $"cutscene Baby fade {index} edit takes effect");
            AssertEqual(native.FadeColor(index, 4), colors.FadeColor(index, 4),
                $"cutscene Baby fade {index} adjacent color stays native");
        }
        AssertEqual(colors.FadeColor(6, 5),
            (EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .Bosses.BabyMetroidCutsceneColors ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrides) .BabyMetroidCutsceneColors.")).FadeColor(6, 5),
            "cutscene Baby color override survives catalog reload");

        var guard = NewBabyMetroidCutsceneColorReadGuard(rom);
        var cgram = new SamusColourRig();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
        Type type = typeof(RoomEnemySystem);
        enemies.Presentation.BindColours(cgram.Gameplay);
        // The Baby's colours are sprite palette seven from colour one, CGRAM byte $01E2.
        int destination = 0x01e2 / 2;
        cgram.SetColor(destination - 1, RgbColor.FromWord(0x1234));
        enemies.BabyMetroidCutsceneBehavior.LoadBabyMetroidCutsceneInitialPalette();
        AssertEqual(0x1234, cgram.Colors[destination - 1],
            "cutscene Baby initial load preserves palette color zero");
        for (int color = 0; color < BabyMetroidCutsceneColorDefinitions.InitialColorCount; color++)
            AssertEqual(colors.InitialColor(color), cgram.Colors[destination + color],
                $"live cutscene Baby initial color {color}");
        RgbColor initialLastColor = cgram.Colors[destination + 14];

        for (int index = 1; index <= BabyMetroidCutsceneColorDefinitions.FadeFrameCount; index++)
        {
            enemies.BabyMetroidCutsceneBehavior.LoadBabyMetroidCutsceneFadePalette(index);
            for (int color = 0; color < BabyMetroidCutsceneColorDefinitions.FadeColorCount; color++)
                AssertEqual(colors.FadeColor(index, color), cgram.Colors[destination + color],
                    $"live cutscene Baby fade {index} color {color}");
            AssertEqual(initialLastColor, cgram.Colors[destination + 14],
                $"cutscene Baby fade {index} preserves color fifteen");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live cutscene Baby palette consumers avoid migrated RGB5 ROM reads");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid cutscene Baby color override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate cutscene Baby color property fails loudly");
        visual.Fade[0][5] = visual.Fade[0][5] with { Blue = 32 };
        AssertThrows<InvalidDataException>(
            () => BabyMetroidCutsceneColorFile.Write(visual),
            "cutscene Baby RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock cutscene Baby colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Cutscene Baby colors: 99 native RGB5 words, six selectors, live initial/fade CGRAM, persistent override, ROM guard and strict failures pass.");

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };
    }

    /// <summary>Blocks migrated initial and fade color ranges while the cutscene applies palettes.</summary>
    private static CartridgeReadGuard NewBabyMetroidCutsceneColorReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                    address is >= MotherBrainSourceDefinitions.BabyMetroid.InitialColors and
                        < (MotherBrainSourceDefinitions.BabyMetroid.InitialColors +
                            BabyMetroidCutsceneColorDefinitions.InitialColorCount * sizeof(ushort)) ||
                    address >= MotherBrainSourceDefinitions.BabyMetroid.FirstFadeColors &&
                    address < MotherBrainSourceDefinitions.BabyMetroid.FadeColors(
                        BabyMetroidCutsceneColorDefinitions.FadeFrameCount) +
                        BabyMetroidCutsceneColorDefinitions.FadeColorCount * sizeof(ushort),
                address => $"Live cutscene Baby read migrated color ROM ${address:X6}.");
}
