using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using System.Text.Json;
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

/// <summary>Checks installed Mother Brain death palettes against retail data and verifies editable colors reach CGRAM without cartridge reads.</summary>
internal static class EnemyTileArtworkMotherBrainDeathColorsVerification
{
    /// <summary>$AD:E9E8, the body fade pointers the cartridge selects through, followed by a null word.</summary>
    private const int BodyFadeTable = 0xade9e8;

    /// <summary>$AD:F107, the corpse fade pointers the cartridge selects through, followed by a null word.</summary>
    private const int CorpseFadeTable = 0xadf107;

    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Compares all fade selectors and RGB5 words, edits a palette channel, and checks live body, corpse, and door destinations.</summary>
    internal static void VerifyInstalledMotherBrainDeathColors(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        MotherBrainDeathColorCatalog native = stock.Bosses.MotherBrainDeathColors ??
            throw new InvalidDataException("Installed enemy art has no Mother Brain death colors.");
        for (int frame = 0; frame < MotherBrainDeathDefinitions.BodyFadeFrameCount; frame++)
        {
            int source = MotherBrainSourceDefinitions.Death.BodyFadeColors(frame);
            AssertEqual(unchecked((ushort)source),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    BodyFadeTable + frame * sizeof(ushort)),
                $"native Mother Brain body death-fade selector {frame}");
            for (int color = 0; color < MotherBrainDeathDefinitions.BodyColorCount; color++)
            {
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), source + color * 2),
                    native.BodyColor(frame, color),
                    $"installed Mother Brain body death frame {frame} color {color}");
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                        source + (MotherBrainDeathDefinitions.BodyColorCount + color) * 2),
                    native.LegColor(frame, color),
                    $"installed Mother Brain leg death frame {frame} color {color}");
            }
        }
        for (int frame = 0; frame < MotherBrainDeathDefinitions.CorpseFadeFrameCount; frame++)
        {
            int source = MotherBrainSourceDefinitions.Death.CorpseFadeColors(frame);
            AssertEqual(unchecked((ushort)source),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    CorpseFadeTable + frame * sizeof(ushort)),
                $"native Mother Brain corpse death-fade selector {frame}");
            for (int color = 0; color < MotherBrainDeathDefinitions.CorpseColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), source + color * 2),
                    native.CorpseColor(frame, color),
                    $"installed Mother Brain corpse death frame {frame} color {color}");
        }
        for (int color = 0; color < MotherBrainDeathDefinitions.BodyColorCount; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    MotherBrainSourceDefinitions.Death.ExplodedDoorColors + color * 2),
                native.ExplodedDoorColor(color),
                $"installed Mother Brain exploded-door color {color}");

        string file = Path.Combine(stockDirectory, MotherBrainDeathColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        MotherBrainDeathColorDocument visual =
            JsonSerializer.Deserialize<MotherBrainDeathColorDocument>(stockJson,
                CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Mother Brain death color JSON is null.");
        foreach (PaletteRgb5[] frame in visual.BodyFade)
            frame[5] = ChangeRed(frame[5]);
        foreach (PaletteRgb5[] frame in visual.LegFade)
            frame[5] = ChangeRed(frame[5]);
        foreach (PaletteRgb5[] frame in visual.CorpseFade)
            frame[5] = ChangeRed(frame[5]);
        visual.ExplodedDoor[5] = ChangeRed(visual.ExplodedDoor[5]);
        string overrides = Path.Combine(stockDirectory,
            "mother-brain-death-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides,
            MotherBrainDeathColorFormat.FileName);
        File.WriteAllBytes(overrideFile, MotherBrainDeathColorFile.Write(visual));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        MotherBrainDeathColorCatalog colors = edited.Bosses.MotherBrainDeathColors ??
            throw new InvalidDataException("Edited enemy art has no Mother Brain death colors.");
        for (int frame = 0; frame < MotherBrainDeathDefinitions.BodyFadeFrameCount; frame++)
        {
            AssertTrue(native.BodyColor(frame, 5) != colors.BodyColor(frame, 5),
                $"Mother Brain body death frame {frame} edit takes effect");
            AssertTrue(native.LegColor(frame, 5) != colors.LegColor(frame, 5),
                $"Mother Brain leg death frame {frame} edit takes effect");
            AssertEqual(native.BodyColor(frame, 4), colors.BodyColor(frame, 4),
                $"Mother Brain body death frame {frame} adjacent color stays native");
        }
        for (int frame = 0; frame < MotherBrainDeathDefinitions.CorpseFadeFrameCount; frame++)
            AssertTrue(native.CorpseColor(frame, 5) != colors.CorpseColor(frame, 5),
                $"Mother Brain corpse death frame {frame} edit takes effect");
        AssertTrue(native.ExplodedDoorColor(5) != colors.ExplodedDoorColor(5),
            "Mother Brain exploded-door edit takes effect");
        AssertEqual(colors.CorpseColor(7, 5),
            (EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .Bosses.MotherBrainDeathColors ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrides) .MotherBrainDeathColors.")).CorpseColor(7, 5),
            "Mother Brain death color override survives catalog reload");

        CartridgeReadGuard guard = new CartridgeReadGuard(rom).Forbid(
            address => address >= BodyFadeTable &&
                    address < MotherBrainSourceDefinitions.Death.BodyFadeColors(
                        MotherBrainDeathDefinitions.BodyFadeFrameCount - 1) +
                        MotherBrainDeathDefinitions.BodyColorCount * 2 * sizeof(ushort) ||
                address >= CorpseFadeTable &&
                    address < MotherBrainSourceDefinitions.Death.CorpseFadeColors(
                        MotherBrainDeathDefinitions.CorpseFadeFrameCount - 1) +
                        MotherBrainDeathDefinitions.CorpseColorCount * sizeof(ushort) ||
                address is >= MotherBrainSourceDefinitions.Death.ExplodedDoorColors and
                    < (MotherBrainSourceDefinitions.Death.ExplodedDoorColors +
                        MotherBrainDeathDefinitions.BodyColorCount * sizeof(ushort)),
            address => $"Live Mother Brain death read migrated ROM ${address:X6}.");
        var cgram = new SamusColourRig();
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
        Type type = typeof(RoomEnemySystem);
        enemies.Presentation.BindColours(cgram.Gameplay);
        cgram.SetColor(MotherBrainDeathColorDestinations.BodyColors - 1, RgbColor.FromWord(0x1234));
        cgram.SetColor(MotherBrainDeathColorDestinations.BrainColors - 1, RgbColor.FromWord(0x1234));
        cgram.SetColor(MotherBrainDeathColorDestinations.LegColors - 1, RgbColor.FromWord(0x1234));
        cgram.SetColor(MotherBrainDeathColorDestinations.CorpseColors - 1, RgbColor.FromWord(0x1234));
        for (int frame = 0; frame < MotherBrainDeathDefinitions.BodyFadeFrameCount; frame++)
        {
            enemies.MotherBrainBehavior.RainbowAndDeath.LoadMotherBrainDeathBodyFade(frame);
            for (int color = 0; color < MotherBrainDeathDefinitions.BodyColorCount; color++)
            {
                RgbColor shared = colors.BodyColor(frame, color);
                AssertEqual(shared, cgram.Colors[MotherBrainDeathColorDestinations.BodyColors + color],
                    $"live Mother Brain body death frame {frame} color {color}");
                AssertEqual(shared, cgram.Colors[MotherBrainDeathColorDestinations.BrainColors + color],
                    $"live Mother Brain brain death frame {frame} color {color}");
                AssertEqual(colors.LegColor(frame, color),
                    cgram.Colors[MotherBrainDeathColorDestinations.LegColors + color],
                    $"live Mother Brain leg death frame {frame} color {color}");
            }
        }
        for (int frame = 0; frame < MotherBrainDeathDefinitions.CorpseFadeFrameCount; frame++)
        {
            enemies.MotherBrainBehavior.RainbowAndDeath.LoadMotherBrainDeathCorpseFade(frame);
            for (int color = 0; color < MotherBrainDeathDefinitions.CorpseColorCount; color++)
                AssertEqual(colors.CorpseColor(frame, color),
                    cgram.Colors[MotherBrainDeathColorDestinations.CorpseColors + color],
                    $"live Mother Brain corpse death frame {frame} color {color}");
        }
        enemies.MotherBrainBehavior.RainbowAndDeath.LoadMotherBrainDeathDoorPalette();
        for (int color = 0; color < MotherBrainDeathDefinitions.BodyColorCount; color++)
            AssertEqual(colors.ExplodedDoorColor(color),
                cgram.Colors[MotherBrainDeathColorDestinations.BrainColors + color],
                $"live Mother Brain exploded-door color {color}");
        foreach (int destination in new[] { MotherBrainDeathColorDestinations.BodyColors,
            MotherBrainDeathColorDestinations.BrainColors, MotherBrainDeathColorDestinations.LegColors,
            MotherBrainDeathColorDestinations.CorpseColors })
            AssertEqual(0x1234, cgram.Colors[destination - 1],
                $"Mother Brain death palette preserves adjacent color {destination - 1}");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live Mother Brain death palettes avoid migrated RGB5/selector ROM reads");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Mother Brain death override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Mother Brain death property fails loudly");
        visual.BodyFade[0][5] = visual.BodyFade[0][5] with { Blue = 32 };
        AssertThrows<InvalidDataException>(
            () => MotherBrainDeathColorFile.Write(visual),
            "Mother Brain death RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Mother Brain death colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Mother Brain death colors: 582 native RGB5 words, 24 selectors, live body/leg/corpse/door CGRAM, persistent override, ROM guard and strict failures pass.");

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };
    }
}
