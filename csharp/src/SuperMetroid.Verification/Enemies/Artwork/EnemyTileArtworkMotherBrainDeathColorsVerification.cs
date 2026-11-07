using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using System.Text.Json;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.Verification.Bosses;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Mother Brain death palettes against retail data and checks their live use without cartridge reads.</summary>
internal static class EnemyTileArtworkMotherBrainDeathColorsVerification
{
    /// <summary>$AD:E9E8, the body fade pointers the cartridge selects through, followed by a null word.</summary>
    private const int BodyFadeTable = 0xade9e8;

    /// <summary>$AD:F107, the corpse fade pointers the cartridge selects through, followed by a null word.</summary>
    private const int CorpseFadeTable = 0xadf107;

    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Compares all fade selectors and RGB5 words and checks live body, corpse, and door destinations.</summary>
    internal static void VerifyInstalledMotherBrainDeathColors(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        MotherBrainDeathColorCatalog native = stock.Bosses.MotherBrainDeathColors ??
            throw new InvalidDataException("Installed enemy art has no Mother Brain death colors.");
        for (int frame = 0; frame < MotherBrainDeathColorFile.BodyFadeFrameCount; frame++)
        {
            int source = MotherBrainSourceDefinitions.Death.BodyFadeColors(frame);
            AssertEqual(unchecked((ushort)source),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    BodyFadeTable + frame * sizeof(ushort)),
                $"native Mother Brain body death-fade selector {frame}");
            for (int color = 0; color < MotherBrainDeathColorFormat.BodyColorCount; color++)
            {
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), source + color * 2),
                    native.BodyColor(frame, color),
                    $"installed Mother Brain body death frame {frame} color {color}");
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                        source + (MotherBrainDeathColorFormat.BodyColorCount + color) * 2),
                    native.LegColor(frame, color),
                    $"installed Mother Brain leg death frame {frame} color {color}");
            }
        }
        for (int frame = 0; frame < MotherBrainDeathColorFile.CorpseFadeFrameCount; frame++)
        {
            int source = MotherBrainSourceDefinitions.Death.CorpseFadeColors(frame);
            AssertEqual(unchecked((ushort)source),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    CorpseFadeTable + frame * sizeof(ushort)),
                $"native Mother Brain corpse death-fade selector {frame}");
            for (int color = 0; color < MotherBrainDeathColorFormat.CorpseColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), source + color * 2),
                    native.CorpseColor(frame, color),
                    $"installed Mother Brain corpse death frame {frame} color {color}");
        }
        for (int color = 0; color < MotherBrainDeathColorFormat.BodyColorCount; color++)
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
        MotherBrainDeathColorCatalog colors = native;

        CartridgeReadGuard guard = new CartridgeReadGuard(rom).Forbid(
            address => address >= BodyFadeTable &&
                    address < MotherBrainSourceDefinitions.Death.BodyFadeColors(
                        MotherBrainDeathColorFile.BodyFadeFrameCount - 1) +
                        MotherBrainDeathColorFormat.BodyColorCount * 2 * sizeof(ushort) ||
                address >= CorpseFadeTable &&
                    address < MotherBrainSourceDefinitions.Death.CorpseFadeColors(
                        MotherBrainDeathColorFile.CorpseFadeFrameCount - 1) +
                        MotherBrainDeathColorFormat.CorpseColorCount * sizeof(ushort) ||
                address is >= MotherBrainSourceDefinitions.Death.ExplodedDoorColors and
                    < (MotherBrainSourceDefinitions.Death.ExplodedDoorColors +
                        MotherBrainDeathColorFormat.BodyColorCount * sizeof(ushort)),
            address => $"Live Mother Brain death read migrated ROM ${address:X6}.");
        // The import pre-colours her body layer and her sprites in each death fade step, her detached brain in each step of
        // the corpse's fade, and her brain in the exploded door's colours, from the installed colours (#1512).
        IReadOnlyList<SheetShade> background = MotherBrainSheetShades.Background(InstalledFixtures.Maps, stock.Bosses);
        IReadOnlyList<SheetShade> sprites = MotherBrainSheetShades.Sprites(InstalledFixtures.Maps, stock.Bosses, InstalledFixtures.RoomPalettes);
        ushort[] Shown(PictureShade shade)
        {
            ushort[] colours = BossShadeColours.Filled(0x1234);
            if (background.Any(candidate => candidate.Shade == shade))
                BossShadeColours.Lay(colours, background, shade, BossShadeColours.Room);
            return BossShadeColours.Lay(colours, sprites, shade, BossShadeColours.Sprites);
        }
        for (int frame = 0; frame < MotherBrainDeathColorFile.BodyFadeFrameCount; frame++)
        {
            ushort[] cgram = Shown(MotherBrainShades.DeathBody(frame));
            for (int color = 0; color < MotherBrainDeathColorFormat.BodyColorCount; color++)
            {
                ushort shared = colors.BodyColor(frame, color).ToWord();
                AssertEqual(shared, cgram[MotherBrainDeathColorDestinations.BodyColors + color],
                    $"live Mother Brain body death frame {frame} color {color}");
                AssertEqual(shared, cgram[MotherBrainDeathColorDestinations.BrainColors + color],
                    $"live Mother Brain brain death frame {frame} color {color}");
                AssertEqual(colors.LegColor(frame, color).ToWord(),
                    cgram[MotherBrainDeathColorDestinations.LegColors + color],
                    $"live Mother Brain leg death frame {frame} color {color}");
            }
            foreach (int destination in new[] { MotherBrainDeathColorDestinations.BodyColors,
                MotherBrainDeathColorDestinations.BrainColors, MotherBrainDeathColorDestinations.LegColors })
                AssertEqual(0x1234, cgram[destination - 1],
                    $"Mother Brain death palette preserves adjacent color {destination - 1}");
        }
        for (int frame = 0; frame < MotherBrainDeathColorFile.CorpseFadeFrameCount; frame++)
        {
            ushort[] cgram = Shown(MotherBrainShades.CorpseFade(frame));
            for (int color = 0; color < MotherBrainDeathColorFormat.CorpseColorCount; color++)
                AssertEqual(colors.CorpseColor(frame, color).ToWord(),
                    cgram[MotherBrainDeathColorDestinations.BrainColors + color],
                    $"live Mother Brain corpse death frame {frame} color {color}, shown over her brain's pictures");
        }
        ushort[] door = Shown(MotherBrainShades.ExplodedDoor);
        for (int color = 0; color < MotherBrainDeathColorFormat.BodyColorCount; color++)
            AssertEqual(colors.ExplodedDoorColor(color).ToWord(),
                door[MotherBrainDeathColorDestinations.BrainColors + color],
                $"live Mother Brain exploded-door color {color}");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live Mother Brain death palettes avoid migrated RGB5/selector ROM reads");

        visual.BodyFade[0][5] = visual.BodyFade[0][5] with { Blue = 32 };
        AssertThrows<InvalidDataException>(
            () => MotherBrainDeathColorFile.Read(new MemoryStream(MotherBrainDeathColorFile.Write(visual), writable: false)),
            "reading Mother Brain death colors rejects an RGB5 channel outside five-bit precision");
        Console.WriteLine("  Mother Brain death colors: 582 native RGB5 words, 24 selectors, live body/leg/corpse/door CGRAM, ROM guard and strict failures pass.");
    }
}
