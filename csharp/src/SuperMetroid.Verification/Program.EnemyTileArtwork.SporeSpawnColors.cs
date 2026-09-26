using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledSporeSpawnColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        SporeSpawnColorCatalog native = stock.SporeSpawnColors ??
            throw new InvalidDataException("Installed enemy art has no Spore Spawn colors.");
        VerifyFrame(SporeSpawnColorRomData.SporeSource, native.ResolveSpore,
            "spore initialization");
        VerifyFrames(SporeSpawnColorRomData.HealthSource,
            SporeSpawnColorRomData.HealthFrameCount, native.ResolveHealth, "health");
        VerifyFrames(SporeSpawnColorRomData.DeathSpriteSource,
            SporeSpawnColorRomData.DeathSpriteFrameCount, native.ResolveDeathSprite,
            "death sprite");
        VerifyFrames(SporeSpawnColorRomData.DeathLevelSource,
            SporeSpawnColorRomData.DeathSceneFrameCount, native.ResolveDeathLevel,
            "death level");
        VerifyFrames(SporeSpawnColorRomData.DeathBackgroundSource,
            SporeSpawnColorRomData.DeathSceneFrameCount, native.ResolveDeathBackground,
            "death background");

        string file = Path.Combine(stockDirectory, SporeSpawnColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        SporeSpawnColorDocument visual =
            JsonSerializer.Deserialize<SporeSpawnColorDocument>(stockJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
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
        File.WriteAllBytes(overrideFile, SporeSpawnColorCatalog.Write(visual));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        SporeSpawnColorCatalog colors = edited.SporeSpawnColors ??
            throw new InvalidDataException("Edited enemy art has no Spore Spawn colors.");
        AssertTrue(native.ResolveSpore(5) != colors.ResolveSpore(5),
            "Spore Spawn initial spore edit changes its selected color");
        AssertEqual(native.ResolveSpore(4), colors.ResolveSpore(4),
            "Spore Spawn spore edit leaves adjacent color unchanged");
        for (int frame = 0; frame < SporeSpawnColorRomData.HealthFrameCount; frame++)
            CheckEditedFrame(native.ResolveHealth, colors.ResolveHealth, frame, "health");
        for (int frame = 0; frame < SporeSpawnColorRomData.DeathSpriteFrameCount; frame++)
            CheckEditedFrame(native.ResolveDeathSprite, colors.ResolveDeathSprite,
                frame, "death sprite");
        for (int frame = 0; frame < SporeSpawnColorRomData.DeathSceneFrameCount; frame++)
        {
            CheckEditedFrame(native.ResolveDeathLevel, colors.ResolveDeathLevel,
                frame, "death level");
            CheckEditedFrame(native.ResolveDeathBackground, colors.ResolveDeathBackground,
                frame, "death background");
        }
        AssertEqual(colors.ResolveDeathSprite(6, 5),
            EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .SporeSpawnColors!.ResolveDeathSprite(6, 5),
            "Spore Spawn override survives catalog reload");

        var guarded = new SporeSpawnPaletteReadGuard(rom);
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem { TileArtwork = edited };
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guarded);
        type.GetField("_cgram", flags)!.SetValue(enemies, cgram);
        type.GetField("_isAreaMiniBossDefeated", flags)!.SetValue(
            enemies, (Func<bool>)(() => false));
        RoomEnemySlot body = enemies.Slots[0];
        body.EnemyDefinitionPointer = RoomEnemySystem.SporeSpawnDefinition;
        body.Definition = default(RoomEnemyDefinition) with { Bank = 0xa5 };
        body.XPosition = 128;
        body.YPosition = 624;
        body.Health = 960;
        type.GetMethod("InitializeSporeSpawn", flags)!.Invoke(enemies, [body]);
        SporeSpawnEnemyState state = enemies.SporeSpawn ??
            throw new InvalidDataException("Live Spore Spawn initializer did not install state.");
        for (int color = 0; color < SporeSpawnColorRomData.ColorsPerFrame; color++)
        {
            int slot = SporeSpawnColorRomData.SporeDestination + color;
            AssertEqual(colors.ResolveSpore(color), cgram.Colors[slot],
                $"live spore CGRAM color {color}");
            AssertEqual(colors.ResolveSpore(color), state.TargetPalette.Span[slot],
                $"live spore target color {color}");
        }
        AssertEqual(SporeSpawnInstructionProgramDefinitions.InitialAlive,
            body.CurrentInstruction, "Spore Spawn retains native living instruction");

        MethodInfo loadHealth = type.GetMethod("LoadSporeSpawnHealthPalette", flags)!;
        for (int frame = 0; frame < SporeSpawnColorRomData.HealthFrameCount; frame++)
        {
            loadHealth.Invoke(enemies, [(ushort)(frame * SporeSpawnColorRomData.FrameByteCount)]);
            for (int color = 0; color < SporeSpawnColorRomData.ColorsPerFrame; color++)
                AssertEqual(colors.ResolveHealth(frame, color),
                    cgram.Colors[SporeSpawnColorRomData.SpriteDestination + color],
                    $"live Spore Spawn health frame {frame} color {color}");
        }

        MethodInfo loadDeath = type.GetMethod("LoadSporeSpawnDeathPalette", flags)!;
        for (int frame = 0; frame < SporeSpawnColorRomData.DeathSceneFrameCount; frame++)
        {
            ushort offset = (ushort)(frame * SporeSpawnColorRomData.FrameByteCount);
            loadDeath.Invoke(enemies, [offset, true]);
            CheckDeath(frame, targetOnly: true);
            loadDeath.Invoke(enemies, [offset, false]);
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
        AssertThrows<InvalidDataException>(() => SporeSpawnColorCatalog.Write(visual),
            "Spore Spawn RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Spore Spawn colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Spore Spawn colors: 432 native RGB5 words, initial/health/seven death frames, current and target palette edits, ROM guard and strict failures pass.");

        void VerifyFrame(int source, Func<int, ushort> resolve, string name)
        {
            for (int color = 0; color < SporeSpawnColorRomData.ColorsPerFrame; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed {name} color {color} matches cartridge");
        }

        void VerifyFrames(int source, int count, Func<int, int, ushort> resolve, string name)
        {
            for (int frame = 0; frame < count; frame++)
                VerifyFrame(source + frame * SporeSpawnColorRomData.FrameByteCount,
                    color => resolve(frame, color), $"{name} frame {frame}");
        }

        void CheckDeath(int frame, bool targetOnly)
        {
            CheckLayer(SporeSpawnDeathPaletteLayer.Sprite);
            CheckLayer(SporeSpawnDeathPaletteLayer.Level);
            CheckLayer(SporeSpawnDeathPaletteLayer.Background);

            void CheckLayer(SporeSpawnDeathPaletteLayer layer)
            {
                int destination = SporeSpawnColorRomData.DeathDestination(layer);
                for (int color = 0; color < SporeSpawnColorRomData.ColorsPerFrame; color++)
                {
                    ushort actual = targetOnly
                        ? state.TargetPalette.Span[destination + color]
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

        static void CheckEditedFrame(Func<int, int, ushort> original,
            Func<int, int, ushort> changed, int frame, string name)
        {
            AssertTrue(original(frame, 5) != changed(frame, 5),
                $"{name} frame {frame} edit changes selected RGB5 word");
            AssertEqual(original(frame, 4), changed(frame, 4),
                $"{name} frame {frame} edit leaves adjacent color unchanged");
        }
    }

    private sealed class SporeSpawnPaletteReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= SporeSpawnColorRomData.SporeSource &&
                address < SporeSpawnColorRomData.DeathBackgroundSource +
                    SporeSpawnColorRomData.DeathSceneFrameCount *
                    SporeSpawnColorRomData.FrameByteCount)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Live Spore Spawn read migrated color ROM ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
