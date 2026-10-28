using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Draygon palette bands against retail colors and edited intro, hurt, flash, and health CGRAM paths.</summary>
internal static class EnemyTileArtworkDraygonColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Compares 105 RGB5 words with ROM, edits selected palette cells, and checks production Draygon color writes under a source-read guard.</summary>
    internal static void VerifyInstalledDraygonColors(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        DraygonColorCatalog native = stock.Bosses.DraygonColors ??
            throw new InvalidDataException("Installed enemy art has no Draygon colors.");
        Suite(nameof(VerifyBand), () => VerifyBand(DraygonSourceDefinitions.IntroColors, DraygonColorDefinitions.IntroCount,
            native.ResolveIntro, "intro"));
        Suite(nameof(VerifyBand), () => VerifyBand(DraygonSourceDefinitions.BackgroundColors,
            DraygonColorDefinitions.BackgroundCount, native.ResolveBackground, "background"));
        Suite(nameof(VerifyBand), () => VerifyBand(DraygonSourceDefinitions.SpriteColors,
            DraygonColorDefinitions.SpriteCount, native.ResolveSprite, "sprite"));
        Suite(nameof(VerifyBand), () => VerifyBand(DraygonSourceDefinitions.WhiteFlashColors,
            DraygonColorDefinitions.WhiteFlashCount, native.ResolveWhiteFlash, "white flash"));
        for (int band = 0; band < DraygonColorDefinitions.HealthBandCount; band++)
        for (int color = 0; color < DraygonColorDefinitions.HealthBandColorCount; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    DraygonSourceDefinitions.HealthBandColors +
                    (band * DraygonColorDefinitions.HealthBandColorCount + color) * sizeof(ushort)),
                native.ResolveHealthBand(band, color),
                $"installed Draygon health band {band} color {color} preserves native RGB5");

        string file = Path.Combine(stockDirectory, DraygonColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        DraygonColorDocument visual = JsonSerializer.Deserialize<DraygonColorDocument>(
            stockJson, CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Draygon color JSON is null.");
        visual.Intro[1] = ChangeRed(visual.Intro[1]);
        visual.Background[1] = ChangeRed(visual.Background[1]);
        visual.Sprite[1] = ChangeRed(visual.Sprite[1]);
        visual.WhiteFlash[1] = ChangeRed(visual.WhiteFlash[1]);
        visual.HealthBands[1][1] = ChangeRed(visual.HealthBands[1][1]);
        string overrides = Path.Combine(stockDirectory, "draygon-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, DraygonColorFormat.FileName);
        File.WriteAllBytes(overrideFile, DraygonColorFile.Write(visual));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        DraygonColorCatalog colors = edited.Bosses.DraygonColors ??
            throw new InvalidDataException("Edited enemy art has no Draygon colors.");
        CheckEditedBand(DraygonColorDefinitions.IntroCount,
            native.ResolveIntro, colors.ResolveIntro, "intro");
        CheckEditedBand(DraygonColorDefinitions.BackgroundCount,
            native.ResolveBackground, colors.ResolveBackground, "background");
        CheckEditedBand(DraygonColorDefinitions.SpriteCount,
            native.ResolveSprite, colors.ResolveSprite, "sprite");
        CheckEditedBand(DraygonColorDefinitions.WhiteFlashCount,
            native.ResolveWhiteFlash, colors.ResolveWhiteFlash, "white flash");
        for (int band = 0; band < DraygonColorDefinitions.HealthBandCount; band++)
        for (int color = 0; color < DraygonColorDefinitions.HealthBandColorCount; color++)
            if (band == 1 && color == 1)
                AssertTrue(native.ResolveHealthBand(band, color) !=
                    colors.ResolveHealthBand(band, color),
                    "Draygon edited health-band color changes");
            else
                AssertEqual(native.ResolveHealthBand(band, color),
                    colors.ResolveHealthBand(band, color),
                    $"Draygon health band {band} color {color} remains unchanged");
        AssertEqual(colors.ResolveHealthBand(1, 1),
            (EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .Bosses.DraygonColors ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrides) .DraygonColors.")).ResolveHealthBand(1, 1),
            "Draygon color override survives catalog reload");

        // Invoke the actual body initializer and hurt program with the visual source
        // windows forbidden. Only the resource may supply colors during installed play.
        CartridgeReadGuard guarded = new CartridgeReadGuard(rom).Forbid(
            address => address is >= DraygonSourceDefinitions.HealthBandColors and
                    < (DraygonSourceDefinitions.HealthBandColors +
                        DraygonColorDefinitions.HealthBandCount *
                        DraygonColorDefinitions.HealthBandColorCount * sizeof(ushort))
                    or >= DraygonSourceDefinitions.SpriteColors and
                    < (DraygonSourceDefinitions.WhiteFlashColors +
                        DraygonColorDefinitions.WhiteFlashCount * sizeof(ushort)),
            address => $"Draygon accessed migrated palette ROM ${address:X6}.");
        var cgram = new SamusColourRig();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
        Type type = typeof(RoomEnemySystem);
        enemies.Presentation.BindColours(cgram.Gameplay);
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.DraygonBody, body.Definition);
        enemies.DraygonBehavior.Parts.InitializeDraygonBody(body);
        DraygonEnemyState state = enemies.DraygonBehavior.Parts.Draygon ??
            throw new InvalidOperationException("Draygon body initialization lost its owner.");
        AssertEqual(DraygonAiFunction.IntroInitialDelay, state.Function,
            "installed Draygon colors leave intro mechanics unchanged");
        AssertBand(DraygonColorDestinations.IntroDestination,
            DraygonColorDefinitions.IntroCount, colors.ResolveIntro, "intro CGRAM");

        state.HealthPaletteBand = 1;
        enemies.DraygonBehavior.Combat.ApplyDraygonHurt(body, state, null);
        for (int color = 0; color < DraygonColorDefinitions.BackgroundCount; color++)
        {
            RgbColor expected = color is >= 9 and <= 12
                ? colors.ResolveHealthBand(1, color - 9)
                : colors.ResolveBackground(color);
            AssertEqual(expected,
                cgram.Colors[DraygonColorDestinations.BackgroundDestination + color],
                $"Draygon hurt background color {color}");
        }
        AssertBand(DraygonColorDestinations.SpriteDestination,
            DraygonColorDefinitions.SpriteCount, colors.ResolveSprite,
            "normal hurt sprite CGRAM");
        body.FlashTimer = WrappingCountdown.Start(2);
        enemies.DraygonBehavior.Combat.ApplyDraygonHurt(body, state, null);
        AssertBand(DraygonColorDestinations.BackgroundDestination,
            DraygonColorDefinitions.WhiteFlashCount, colors.ResolveWhiteFlash,
            "white hurt background CGRAM");
        AssertBand(DraygonColorDestinations.SpriteDestination,
            DraygonColorDefinitions.WhiteFlashCount, colors.ResolveWhiteFlash,
            "white hurt sprite CGRAM");
        for (int band = 0; band < DraygonColorDefinitions.HealthBandCount; band++)
        {
            state.HealthPaletteBand = band;
            enemies.DraygonBehavior.Combat.CopyDraygonHealthColors(state);
            for (int color = 0; color < DraygonColorDefinitions.HealthBandColorCount; color++)
                AssertEqual(colors.ResolveHealthBand(band, color),
                    cgram.Colors[DraygonColorDestinations.HealthDestination + color],
                    $"Draygon live health band {band} color {color}");
        }
        AssertEqual(DraygonAiFunction.IntroInitialDelay, state.Function,
            "visual health-band checks leave Draygon AI unchanged");
        AssertThrows<InvalidDataException>(() => colors.ApplyHealthBand(cgram.Gameplay, -1),
            "negative Draygon health band fails loudly");
        AssertThrows<InvalidDataException>(() => colors.ApplyHealthBand(cgram.Gameplay, 8),
            "out-of-range Draygon health-band index fails loudly");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Draygon color override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Draygon color property fails loudly");
        visual.HealthBands[1][1] = visual.HealthBands[1][1] with { Blue = 32 };
        AssertThrows<InvalidDataException>(() => DraygonColorFile.Write(visual),
            "Draygon RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Draygon colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Draygon colors: 105 native RGB5 words, live intro/hurt/flash/eight health bands, persistent override, ROM guard, and strict failures pass.");

        void VerifyBand(int source, int count, Func<int, RgbColor> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed Draygon {name} color {color} preserves native RGB5");
        }

        void AssertBand(int destination, int count, Func<int, RgbColor> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(resolve(color), cgram.Colors[destination + color],
                    $"installed Draygon {name} color {color}");
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };

        static void CheckEditedBand(int count, Func<int, RgbColor> original,
            Func<int, RgbColor> changed, string name)
        {
            AssertTrue(original(1) != changed(1),
                $"Draygon {name} edit changes selected color");
            for (int color = 0; color < count; color++)
                if (color != 1)
                    AssertEqual(original(color), changed(color),
                        $"Draygon {name} edit leaves color {color} unchanged");
        }
    }
}
