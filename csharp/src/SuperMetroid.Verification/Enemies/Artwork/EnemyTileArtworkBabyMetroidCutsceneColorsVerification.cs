using SuperMetroid.AssetExtraction.Video;
using System.Text.Json;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Bosses;
using SuperMetroid.Verification.Bosses.MotherBrain;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Baby Metroid cutscene colors and fade palettes in live CGRAM.</summary>
internal static class EnemyTileArtworkBabyMetroidCutsceneColorsVerification
{
    /// <summary>$AD:E8E2, the seven fade-image pointers the cartridge selects through; index zero is never displayed.</summary>
    private const int FadePointerTable = 0xade8e2;

    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Compares native colors and selectors, the live palette loads, and strict document failures.</summary>
    internal static void VerifyInstalledBabyMetroidCutsceneColors(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        BabyMetroidCutsceneColorCatalog native = stock.Bosses.BabyMetroidCutsceneColors ??
            throw new InvalidDataException("Installed enemy art has no cutscene Baby colors.");
        for (int color = 0; color < BabyMetroidCutsceneColorFormat.InitialColorCount; color++)
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
            for (int color = 0; color < BabyMetroidCutsceneColorFormat.FadeColorCount; color++)
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
        BabyMetroidCutsceneColorCatalog colors = native;

        var guard = NewBabyMetroidCutsceneColorReadGuard(rom);
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses };
        // The Baby's colours load while Mother Brain's encounter is loaded, which lays them over its sprites (#1512).
        var motherBrain = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]);
        enemies.MotherBrainEncounter.MotherBrain = motherBrain;
        Type type = typeof(RoomEnemySystem);
        // The Baby's colours are sprite palette seven from colour one, CGRAM byte $01E2.
        int destination = 0x01e2 / 2;
        ushort[] Shown() => MotherBrainLook.Over(motherBrain, InstalledFixtures.Maps, stock.Bosses, BossShadeColours.Filled(0x1234));
        enemies.BabyMetroidCutsceneBehavior.LoadBabyMetroidCutsceneInitialPalette();
        ushort[] cgram = Shown();
        AssertEqual(0x1234, cgram[destination - 1],
            "cutscene Baby initial load preserves palette color zero");
        for (int color = 0; color < BabyMetroidCutsceneColorFormat.InitialColorCount; color++)
            AssertEqual(colors.InitialColor(color).ToWord(), cgram[destination + color],
                $"live cutscene Baby initial color {color}");
        ushort initialLastColor = cgram[destination + 14];

        for (int index = 1; index <= BabyMetroidCutsceneColorDefinitions.FadeFrameCount; index++)
        {
            enemies.BabyMetroidCutsceneBehavior.LoadBabyMetroidCutsceneFadePalette(index);
            cgram = Shown();
            for (int color = 0; color < BabyMetroidCutsceneColorFormat.FadeColorCount; color++)
                AssertEqual(colors.FadeColor(index, color).ToWord(), cgram[destination + color],
                    $"live cutscene Baby fade {index} color {color}");
            AssertEqual(initialLastColor, cgram[destination + 14],
                $"cutscene Baby fade {index} preserves color fifteen");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live cutscene Baby palette consumers avoid migrated RGB5 ROM reads");

        visual.Fade[0][5] = visual.Fade[0][5] with { Blue = 32 };
        AssertThrows<InvalidDataException>(
            () => BabyMetroidCutsceneColorFile.Read(new MemoryStream(BabyMetroidCutsceneColorFile.Write(visual), writable: false)),
            "reading cutscene Baby colors rejects an RGB5 channel outside five-bit precision");
        Console.WriteLine("  Cutscene Baby colors: 99 native RGB5 words, six selectors, live initial/fade CGRAM, ROM guard and strict failures pass.");
    }

    /// <summary>Blocks migrated initial and fade color ranges while the cutscene applies palettes.</summary>
    private static CartridgeReadGuard NewBabyMetroidCutsceneColorReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address =>
                    address is >= MotherBrainSourceDefinitions.BabyMetroid.InitialColors and
                        < (MotherBrainSourceDefinitions.BabyMetroid.InitialColors +
                            BabyMetroidCutsceneColorFormat.InitialColorCount * sizeof(ushort)) ||
                    address >= MotherBrainSourceDefinitions.BabyMetroid.FirstFadeColors &&
                    address < MotherBrainSourceDefinitions.BabyMetroid.FadeColors(
                        BabyMetroidCutsceneColorDefinitions.FadeFrameCount) +
                        BabyMetroidCutsceneColorFormat.FadeColorCount * sizeof(ushort),
                address => $"Live cutscene Baby read migrated color ROM ${address:X6}.");
}
