using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using System.Text.Json;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Artwork;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;
using SuperMetroid.AssetExtraction.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
/// <summary>Checks installed Spore Spawn spore, health, and death colors against ROM and live current and target palettes.</summary>
internal static class EnemyTileArtworkSporeSpawnColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Compares 432 native RGB5 words and checks production shades and fade targets without source reads.</summary>
    internal static void VerifyInstalledSporeSpawnColors(
        ISnesAddressSpace rom, string stockDirectory, ImportEnemyArtwork stock)
    {
        SporeSpawnColorCatalog native = stock.Bosses.SporeSpawnColors ??
            throw new InvalidDataException("Installed enemy art has no Spore Spawn colors.");
        Suite(nameof(VerifyFrame), () => VerifyFrame(SporeSpawnSourceDefinitions.SporeColors, native.ResolveSpore,
            "spore initialization"));
        Suite(nameof(VerifyFrames), () => VerifyFrames(SporeSpawnSourceDefinitions.HealthColors,
            SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnHealthBandDefinitions.HighestFirst.Count, (band, colour) => native.ResolveHealth(SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnHealthBandDefinitions.HighestFirst[band], colour), "health"));
        Suite(nameof(VerifyFrames), () => VerifyFrames(SporeSpawnSourceDefinitions.DeathSpriteColors,
            SporeSpawnColorFormat.DeathSpriteFrameCount, native.ResolveDeathSprite,
            "death sprite"));
        Suite(nameof(VerifyFrames), () => VerifyFrames(SporeSpawnSourceDefinitions.DeathLevelColors,
            SporeSpawnColorFormat.DeathSceneFrameCount, native.ResolveDeathLevel,
            "death level"));
        Suite(nameof(VerifyFrames), () => VerifyFrames(SporeSpawnSourceDefinitions.DeathBackgroundColors,
            SporeSpawnColorFormat.DeathSceneFrameCount, native.ResolveDeathBackground,
            "death background"));

        string file = Path.Combine(stockDirectory, SporeSpawnColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        SporeSpawnColorDocument visual =
            JsonSerializer.Deserialize<SporeSpawnColorDocument>(stockJson,
                CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Spore Spawn color JSON is null.");
        SporeSpawnColorCatalog colors = native;

        CartridgeReadGuard guarded = new CartridgeReadGuard(rom).Forbid(
            address => address is >= SporeSpawnSourceDefinitions.SporeColors and
                < (SporeSpawnSourceDefinitions.DeathBackgroundColors +
                    SporeSpawnColorFormat.DeathSceneFrameCount *
                    SporeSpawnSourceDefinitions.ImageBytes),
            address => $"Live Spore Spawn read migrated color ROM ${address:X6}.");
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null), installedBosses: stock.Bosses);
        enemies.VerificationServices().IsAreaMiniBossDefeated = () => false;
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.SporeSpawn, body.KeptDefinition);
        body.Bind(body.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        body.Position = body.Position.WithWhole(new(128, 624));
        body.Health = 960;
        enemies.SporeSpawnBehavior.InitializeSporeSpawn(body.HeldOccupant(), body);
        SporeSpawnEnemyState state = body.RequireSpeciesState<SporeSpawnEnemyState>();
        // His live colour handlers name shades of his sprites and room (#1512); the import pre-colours each from the
        // installed colours.
        IReadOnlyList<SheetShade> sprites = SporeSpawnSheetShades.Sprites(stock.Bosses);
        IReadOnlyList<SheetShade> room = SporeSpawnSheetShades.Room(stock.Bosses);
        AssertEqual(PictureShading.Shaded(SporeSpawnShades.Spores), body.Shading, "live Spore Spawn starts in his spores' colours");
        AssertShade(sprites, SporeSpawnShades.Spores, colors.ResolveSpore, "live spore colours");
        AssertEqual(new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.AwaitingDescent), AnimationPlace.StartOfAnimation),
            state.BodyPlayback, "Spore Spawn starts its living animation");

        // His installed health colours hold his bands highest first.
        for (int frame = 0; frame < SporeSpawnHealthBandDefinitions.HighestFirst.Count; frame++)
        {
            int shown = frame;
            SporeSpawnHealthBand band = SporeSpawnHealthBandDefinitions.HighestFirst[frame];
            SporeSpawnBehavior.ShowSporeSpawnHealthShade(body, state, band);
            AssertEqual(PictureShading.Shaded(SporeSpawnShades.Spores).With(SporeSpawnShades.Health(band)), body.Shading,
                $"live Spore Spawn health band {band}");
            AssertShade(sprites, SporeSpawnShades.Health(band), colour => colors.ResolveHealth(SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnHealthBandDefinitions.HighestFirst[shown], colour), $"live Spore Spawn health band {band}");
        }

        foreach (SporeSpawnDeathFadeStep step in SporeSpawnDeathFadeStep.All)
        {
            int frame = step.Step;
            SporeSpawnBehavior.ShowSporeSpawnDeathShade(body, state, step);
            AssertEqual(PictureShading.Shaded(SporeSpawnShades.Spores).With(SporeSpawnShades.DeathSprite(step)), body.Shading,
                $"live Spore Spawn death frame {frame}");
            AssertShade(sprites, SporeSpawnShades.DeathSprite(step), colour => colors.ResolveDeath(SporeSpawnDeathPaletteLayer.Sprite, frame, colour),
                $"live Spore Spawn Sprite death frame {frame}");
            AssertShade(room, SporeSpawnShades.DeathLevel(step), colour => colors.ResolveDeath(SporeSpawnDeathPaletteLayer.Level, frame, colour),
                $"live Spore Spawn Level death frame {frame}");
            AssertShade(room, SporeSpawnShades.DeathBackground(step), colour => colors.ResolveDeath(SporeSpawnDeathPaletteLayer.Background, frame, colour),
                $"live Spore Spawn Background death frame {frame}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "live Spore Spawn palette consumers avoid all migrated ROM colors");

        visual.Health.Values.First()[5] = visual.Health.Values.First()[5] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => SporeSpawnColorFile.Read(new MemoryStream(SporeSpawnColorFile.Write(visual), writable: false)),
            "reading Spore Spawn colors rejects an RGB5 channel outside five-bit precision");
        Console.WriteLine("  Spore Spawn colors: 432 native RGB5 words, initial/health/seven death frames, ROM guard and strict failures pass.");

        void VerifyFrame(int source, Func<int, RgbColor> resolve, string name)
        {
            for (int color = 0; color < SporeSpawnColorFormat.ColorsPerFrame; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    source + color * SporeSpawnSourceDefinitions.ColorWordBytes), resolve(color),
                    $"installed {name} color {color} matches cartridge");
        }

        void VerifyFrames(int source, int count, Func<int, int, RgbColor> resolve, string name)
        {
            for (int frame = 0; frame < count; frame++)
                VerifyFrame(source + frame * SporeSpawnSourceDefinitions.ImageBytes,
                    color => resolve(frame, color), $"{name} frame {frame}");
        }
    }

    /// <summary>Asserts that the import's <paramref name="shade"/> among <paramref name="made"/> shows <paramref name="expected"/>'s colours.</summary>
    private static void AssertShade(IReadOnlyList<SheetShade> made, PictureShade shade, Func<int, RgbColor> expected, string label)
    {
        SheetShade found = made.Single(candidate => candidate.Shade == shade);
        int colour = 0;
        foreach (RgbColor shown in found.Colours)
        {
            AssertEqual(expected(colour), shown, $"{label} colour {colour}");
            colour++;
        }
    }
}
