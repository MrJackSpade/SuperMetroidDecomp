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
            SporeSpawnColorDefinitions.HealthFrameCount, native.ResolveHealth, "health"));
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
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { BossArtwork = stock.Bosses };
        enemies.VerificationServices().IsAreaMiniBossDefeated = () => false;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.SporeSpawn, body.Definition);
        body.Bind(body.DefinitionId, default(RoomEnemyDefinition));
        body.Position = body.Position.WithWhole(new(128, 624));
        body.Health = 960;
        enemies.SporeSpawnBehavior.InitializeSporeSpawn(body);
        SporeSpawnEnemyState state = enemies.SporeSpawn ??
            throw new InvalidDataException("Live Spore Spawn initializer did not install state.");
        // His live colour handlers name shades of his sprites and room (#1512); the import pre-colours each from the
        // installed colours.
        IReadOnlyList<SheetShade> sprites = SporeSpawnSheetShades.Sprites(stock.Bosses);
        IReadOnlyList<SheetShade> room = SporeSpawnSheetShades.Room(stock.Bosses);
        AssertEqual(PictureShading.Shaded(SporeSpawnShades.Spores), body.Shading, "live Spore Spawn starts in his spores' colours");
        AssertShade(sprites, SporeSpawnShades.Spores, colors.ResolveSpore, "live spore colours");
        AssertEqual(new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(SporeSpawnBodySegment.InitialAlive), AnimationPlace.StartOfAnimation),
            state.BodyPlayback, "Spore Spawn starts its living animation");

        for (int frame = 0; frame < SporeSpawnColorDefinitions.HealthFrameCount; frame++)
        {
            int shown = frame;
            enemies.SporeSpawnBehavior.ShowSporeSpawnHealthShade(frame);
            AssertEqual(PictureShading.Shaded(SporeSpawnShades.Spores).With(SporeSpawnShades.Health(frame)), body.Shading,
                $"live Spore Spawn health frame {frame}");
            AssertShade(sprites, SporeSpawnShades.Health(frame), colour => colors.ResolveHealth(shown, colour), $"live Spore Spawn health frame {frame}");
        }

        for (int frame = 0; frame < SporeSpawnColorFormat.DeathSceneFrameCount; frame++)
        {
            int shown = frame;
            enemies.SporeSpawnBehavior.ShowSporeSpawnDeathShade(frame);
            AssertEqual(PictureShading.Shaded(SporeSpawnShades.Spores).With(SporeSpawnShades.DeathSprite(frame)), body.Shading,
                $"live Spore Spawn death frame {frame}");
            AssertShade(sprites, SporeSpawnShades.DeathSprite(frame), colour => colors.ResolveDeath(SporeSpawnDeathPaletteLayer.Sprite, shown, colour),
                $"live Spore Spawn Sprite death frame {frame}");
            AssertShade(room, SporeSpawnShades.DeathLevel(frame), colour => colors.ResolveDeath(SporeSpawnDeathPaletteLayer.Level, shown, colour),
                $"live Spore Spawn Level death frame {frame}");
            AssertShade(room, SporeSpawnShades.DeathBackground(frame), colour => colors.ResolveDeath(SporeSpawnDeathPaletteLayer.Background, shown, colour),
                $"live Spore Spawn Background death frame {frame}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "live Spore Spawn palette consumers avoid all migrated ROM colors");

        visual.Health[0][5] = visual.Health[0][5] with { Green = 32 };
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
        for (int color = 0; color < found.Colours.Length; color++)
            AssertEqual(expected(color), found.Colours[color], $"{label} colour {color}");
    }
}
