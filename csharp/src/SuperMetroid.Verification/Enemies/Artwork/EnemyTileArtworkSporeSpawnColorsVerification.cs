using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Programs;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.Artwork;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Checks installed Spore Spawn spore, health, and death colors against ROM and live current and target palettes.</summary>
internal static class EnemyTileArtworkSporeSpawnColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Compares 432 native RGB5 words, edits one color per palette family, and checks production CGRAM and fade targets without source reads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Spore Spawn colors")]
    internal static void VerifyInstalledSporeSpawnColors(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        SporeSpawnColorCatalog native = stock.Bosses.SporeSpawnColors ??
            throw new InvalidDataException("Installed enemy art has no Spore Spawn colors.");
        Suite(nameof(VerifyFrame), () => VerifyFrame(SporeSpawnSourceDefinitions.SporeColors, native.ResolveSpore,
            "spore initialization"));
        Suite(nameof(VerifyFrames), () => VerifyFrames(SporeSpawnSourceDefinitions.HealthColors,
            SporeSpawnColorDefinitions.HealthFrameCount, native.ResolveHealth, "health"));
        Suite(nameof(VerifyFrames), () => VerifyFrames(SporeSpawnSourceDefinitions.DeathSpriteColors,
            SporeSpawnColorDefinitions.DeathSpriteFrameCount, native.ResolveDeathSprite,
            "death sprite"));
        Suite(nameof(VerifyFrames), () => VerifyFrames(SporeSpawnSourceDefinitions.DeathLevelColors,
            SporeSpawnColorDefinitions.DeathSceneFrameCount, native.ResolveDeathLevel,
            "death level"));
        Suite(nameof(VerifyFrames), () => VerifyFrames(SporeSpawnSourceDefinitions.DeathBackgroundColors,
            SporeSpawnColorDefinitions.DeathSceneFrameCount, native.ResolveDeathBackground,
            "death background"));

        string file = Path.Combine(stockDirectory, SporeSpawnColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        SporeSpawnColorDocument visual =
            JsonSerializer.Deserialize<SporeSpawnColorDocument>(stockJson,
                CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Spore Spawn color JSON is null.");
        visual.Spores[5] = ChangeRed(visual.Spores[5]);
        foreach (PaletteRgb5[] frame in visual.Health)
            frame[5] = ChangeRed(frame[5]);
        foreach (PaletteRgb5[] frame in visual.DeathSprite)
            frame[5] = ChangeRed(frame[5]);
        foreach (PaletteRgb5[] frame in visual.DeathLevel)
            frame[5] = ChangeRed(frame[5]);
        foreach (PaletteRgb5[] frame in visual.DeathBackground)
            frame[5] = ChangeRed(frame[5]);
        string overrides = Path.Combine(stockDirectory, "spore-spawn-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, SporeSpawnColorFormat.FileName);
        File.WriteAllBytes(overrideFile, SporeSpawnColorFile.Write(visual));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        SporeSpawnColorCatalog colors = edited.Bosses.SporeSpawnColors ??
            throw new InvalidDataException("Edited enemy art has no Spore Spawn colors.");
        AssertTrue(native.ResolveSpore(5) != colors.ResolveSpore(5),
            "Spore Spawn initial spore edit changes its selected color");
        AssertEqual(native.ResolveSpore(4), colors.ResolveSpore(4),
            "Spore Spawn spore edit leaves adjacent color unchanged");
        for (int frame = 0; frame < SporeSpawnColorDefinitions.HealthFrameCount; frame++)
            CheckEditedFrame(native.ResolveHealth, colors.ResolveHealth, frame, "health");
        for (int frame = 0; frame < SporeSpawnColorDefinitions.DeathSpriteFrameCount; frame++)
            CheckEditedFrame(native.ResolveDeathSprite, colors.ResolveDeathSprite,
                frame, "death sprite");
        for (int frame = 0; frame < SporeSpawnColorDefinitions.DeathSceneFrameCount; frame++)
        {
            CheckEditedFrame(native.ResolveDeathLevel, colors.ResolveDeathLevel,
                frame, "death level");
            CheckEditedFrame(native.ResolveDeathBackground, colors.ResolveDeathBackground,
                frame, "death background");
        }
        AssertEqual(colors.ResolveDeathSprite(6, 5),
            (EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .Bosses.SporeSpawnColors ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrides) .SporeSpawnColors.")).ResolveDeathSprite(6, 5),
            "Spore Spawn override survives catalog reload");

        CartridgeReadGuard guarded = new CartridgeReadGuard(rom).Forbid(
            address => address is >= SporeSpawnSourceDefinitions.SporeColors and
                < (SporeSpawnSourceDefinitions.DeathBackgroundColors +
                    SporeSpawnColorDefinitions.DeathSceneFrameCount *
                    SporeSpawnSourceDefinitions.ImageBytes),
            address => $"Live Spore Spawn read migrated color ROM ${address:X6}.");
        var cgram = new SamusColourRig();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
        Type type = typeof(RoomEnemySystem);
        enemies.Presentation.BindColours(cgram.Gameplay);
        enemies.VerificationServices().IsAreaMiniBossDefeated = () => false;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.SporeSpawn, body.Definition);
        body.Bind(body.DefinitionId, default(RoomEnemyDefinition));
        body.Position = body.Position.WithWhole(new(128, 624));
        body.Health = 960;
        enemies.SporeSpawnBehavior.InitializeSporeSpawn(body);
        SporeSpawnEnemyState state = enemies.SporeSpawn ??
            throw new InvalidDataException("Live Spore Spawn initializer did not install state.");
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
        {
            int slot = SporeSpawnColorDestinations.SporeDestination + color;
            AssertEqual(colors.ResolveSpore(color), cgram.Colors[slot],
                $"live spore CGRAM color {color}");
            AssertEqual(colors.ResolveSpore(color), StagedAt(state, slot),
                $"live spore target color {color}");
        }
        AssertEqual(AnimationProgramCursor<SporeSpawnBodyProgram>.Start(SporeSpawnBodyProgram.InitialAlive),
            state.BodyProgram, "Spore Spawn starts its living program");

        for (int frame = 0; frame < SporeSpawnColorDefinitions.HealthFrameCount; frame++)
        {
            enemies.SporeSpawnBehavior.LoadSporeSpawnHealthPalette(frame);
            for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
                AssertEqual(colors.ResolveHealth(frame, color),
                    cgram.Colors[SporeSpawnColorDestinations.SpriteDestination + color],
                    $"live Spore Spawn health frame {frame} color {color}");
        }

        for (int frame = 0; frame < SporeSpawnColorDefinitions.DeathSceneFrameCount; frame++)
        {
            enemies.SporeSpawnBehavior.LoadSporeSpawnDeathPalette(frame, SporeSpawnPaletteCopy.TargetOnly);
            CheckDeath(frame, targetOnly: true);
            enemies.SporeSpawnBehavior.LoadSporeSpawnDeathPalette(frame, SporeSpawnPaletteCopy.CurrentAndTarget);
            CheckDeath(frame, targetOnly: false);
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "live Spore Spawn palette consumers avoid all migrated ROM colors");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Spore Spawn override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Spore Spawn color property fails loudly");
        visual.Health[0][5] = visual.Health[0][5] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => SporeSpawnColorFile.Write(visual),
            "Spore Spawn RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Spore Spawn colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Spore Spawn colors: 432 native RGB5 words, initial/health/seven death frames, current and target palette edits, ROM guard and strict failures pass.");

        void VerifyFrame(int source, Func<int, RgbColor> resolve, string name)
        {
            for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
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

        void CheckDeath(int frame, bool targetOnly)
        {
            CheckLayer(SporeSpawnDeathPaletteLayer.Sprite);
            CheckLayer(SporeSpawnDeathPaletteLayer.Level);
            CheckLayer(SporeSpawnDeathPaletteLayer.Background);

            void CheckLayer(SporeSpawnDeathPaletteLayer layer)
            {
                int destination = SporeSpawnColorDestinations.DeathDestination(layer);
                for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
                {
                    RgbColor actual = targetOnly
                        ? StagedAt(state, destination + color)
                        : cgram.Colors[destination + color];
                    AssertEqual(colors.ResolveDeath(layer, frame, color), actual,
                        $"live Spore Spawn {(targetOnly ? "target" : "current")} " +
                        $"{layer} frame {frame} color {color}");
                }
            }
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };

        static void CheckEditedFrame(Func<int, int, RgbColor> original,
            Func<int, int, RgbColor> changed, int frame, string name)
        {
            AssertTrue(original(frame, 5) != changed(frame, 5),
                $"{name} frame {frame} edit changes selected RGB5 word");
            AssertEqual(original(frame, 4), changed(frame, 4),
                $"{name} frame {frame} edit leaves adjacent color unchanged");
        }
    }

    /// <summary>The fade-target colour Spore Spawn last staged at the cartridge's colour <paramref name="position"/>.</summary>
    private static RgbColor StagedAt(SporeSpawnEnemyState state, int position) =>
        state.StagedTargetColours.Last(staged => ColourSetRunPositions.First(staged.Run) + staged.Colour == position).Value;
}
