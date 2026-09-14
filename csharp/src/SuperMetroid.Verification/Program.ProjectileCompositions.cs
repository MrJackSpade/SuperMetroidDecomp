using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyProjectileCompositions(SuperMetroidAddressSpace rom)
    {
        byte[] json = ProjectileSpriteExtractor.Extract(rom);
        var content = ProjectileSpriteCatalog.Load(new MemoryStream(json));
        VerifyProjectileCompositionOwners(rom, content);
        int draws = 0;
        foreach (ushort id in ProjectileSpriteDefinitions.NativePointers)
        foreach (ushort origin in new ushort[] { 0, 1, 127, 255, 256, 0x7fff, 0xffff })
        foreach (int preceding in new[] { 0, 127 })
        {
            var native = new OamBuffer(); var authored = new OamBuffer();
            for (int i = 0; i < preceding; i++)
            {
                native.AddProjectileSpritePart(new(0), 0, new(0), 0, 0);
                authored.AddProjectileSpritePart(new(0), 0, new(0), 0, 0);
            }
            native.AddProjectileSpritemap(rom, id, origin, origin);
            content.Draw(id, authored, origin, origin);
            AssertTrue(native.LowTable.SequenceEqual(authored.LowTable), "Extracted composition retains all low OAM bytes");
            AssertTrue(native.HighTable.SequenceEqual(authored.HighTable), "Extracted composition retains all high OAM bits");
            AssertEqual(native.NextByteOffset, authored.NextByteOffset, "Extracted composition retains native capacity wrapping");
            draws++;
        }
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<ProjectileSpriteDocument>(json, options)!;
        ushort editedId = ProjectileSpriteDefinitions.NativePointers.ToArray().First(id => document.Frames[ProjectileSpriteDefinitions.Name(id)].Length > 0);
        string name = ProjectileSpriteDefinitions.Name(editedId);
        var original = document.Frames[name][0];
        document.Frames[name][0] = original with { OffsetX = original.OffsetX == 255 ? 254 : original.OffsetX + 1 };
        byte[] editedJson = JsonSerializer.SerializeToUtf8Bytes(document, options);
        var edited = ProjectileSpriteCatalog.Load(new MemoryStream(editedJson));
        VerifyProjectileFiles(rom, editedJson);
        VerifyRuntimeProjectileCompositions(rom, content, edited, editedId);
        var baselineOam = new OamBuffer(); var editedOam = new OamBuffer();
        content.Draw(editedId, baselineOam, 100, 100); edited.Draw(editedId, editedOam, 100, 100);
        AssertEqual(unchecked((byte)(baselineOam.LowTable[0] + (original.OffsetX == 255 ? -1 : 1))), editedOam.LowTable[0], "Edited offset reaches emitted OAM");
        AssertTrue(!baselineOam.LowTable.SequenceEqual(editedOam.LowTable), "Visual edit is observable");
        document.Frames[name][0] = original;
        editedOam = new OamBuffer(); edited.Draw(editedId, editedOam, 100, 100);
        AssertTrue(!baselineOam.LowTable.SequenceEqual(editedOam.LowTable), "Loaded composition owns immutable compiled parts");

        void Reject(ProjectileSpriteDocument bad) => AssertThrows<InvalidDataException>(() => ProjectileSpriteCatalog.Load(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(bad, options))), "Malformed composition rejected");
        Reject(document with { Version = 2 });
        foreach (var badPart in new[] { original with { Palette = null }, original with { TileColumn = 16 }, original with { TileRow = 32 }, original with { OffsetX = 256 }, original with { OffsetY = -129 }, original with { Size = 32 } })
        {
            document.Frames[name][0] = badPart; Reject(document);
        }
        document.Frames.Remove(name); Reject(document);
        foreach (string field in new[] { "\"version\":1,", "\"damage\":10," })
        {
            byte[] malformed = System.Text.Encoding.UTF8.GetBytes(System.Text.Encoding.UTF8.GetString(json).Insert(1, field));
            AssertThrows<InvalidDataException>(() => ProjectileSpriteCatalog.Load(new MemoryStream(malformed)), "Duplicate or unknown mechanics fields rejected");
        }
        AssertThrows<InvalidDataException>(() => content.Draw(0, new OamBuffer(), 0, 0), "Unknown sprite is loud");
        Console.WriteLine($"Projectile compositions: 417 extracted sprites/{draws} native OAM comparisons, observable edits, immutable load and invalid-field rejection pass without a draw-time ROM.");
    }

    private static void VerifyProjectileCompositionOwners(ISnesAddressSpace rom, ProjectileSpriteCatalog content)
    {
        var forbidden = new ProjectileCompositionForbiddenBus();
        int comparisons = 0;
        foreach (ushort id in ProjectileSpriteDefinitions.NativePointers)
        foreach (ushort coordinate in new ushort[] { 0, 100, 223, 255, 256, 303, 304, 0xffcf, 0xffd0 })
        foreach (ushort type in new ushort[] { 0, 0x10, 0x100, 0x200, 0x300, 0x500, 0x700, 0x800 })
        {
            var shots = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            var shot = shots.Slots[0];
            shot.InstructionPointer = 1; shot.SpritemapPointer = id;
            shot.Type = type; shot.XPosition = coordinate; shot.YPosition = coordinate;
            shot.Damage = 123; shot.XRadius = 7; shot.YRadius = 9;
            var bomb = bombs.Slots[0];
            bomb.InstructionPointer = 1; bomb.SpritemapPointer = id;
            bomb.Type = type; bomb.XPosition = coordinate; bomb.YPosition = coordinate;
            bomb.BombTimer = 1;
            foreach (ushort parity in new ushort[] { 0, 1, 2, 3 })
            {
                var native = new OamBuffer(); var extracted = new OamBuffer();
                shots.DrawLiveProjectiles(rom, native, 0, 0, parity);
                shots.DrawExplosions(rom, native, 0, 0);
                bombs.Draw(rom, native, 0, 0);
                shots.DrawLiveProjectiles(forbidden, extracted, 0, 0, parity, content);
                shots.DrawExplosions(forbidden, extracted, 0, 0, content);
                bombs.Draw(forbidden, extracted, 0, 0, content);
                AssertTrue(native.LowTable.SequenceEqual(extracted.LowTable) && native.HighTable.SequenceEqual(extracted.HighTable),
                    "Production projectile composition preserves admission, flicker and OAM");
                AssertEqual(native.NextByteOffset, extracted.NextByteOffset, "Composition owner preserves OAM cursor");
                AssertEqual((ushort)123, shot.Damage, "Composition does not mutate damage");
                AssertEqual((ushort)7, shot.XRadius, "Composition does not mutate X radius");
                AssertEqual((ushort)9, shot.YRadius, "Composition does not mutate Y radius");
                comparisons++;
            }
            if (type == 0x300)
            {
                bomb.BombTimer = 0;
                var hidden = new OamBuffer();
                bombs.Draw(forbidden, hidden, 0, 0, content);
                AssertEqual(0, hidden.NextByteOffset, "Detonated power bomb remains hidden with extracted art");
            }
        }
        Console.WriteLine($"Projectile composition owners: {comparisons} stock OAM comparisons with every ROM access forbidden.");
    }

    private sealed class ProjectileCompositionForbiddenBus : ISnesAddressSpace
    {
        public byte ReadByte(int address) => throw new InvalidOperationException($"Composition draw read ROM {address:X6}.");
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Composition draw mutated the bus.");
    }

    private static void VerifyRuntimeProjectileCompositions(ISnesAddressSpace bus,
        ProjectileSpriteCatalog stock, ProjectileSpriteCatalog edited, ushort sprite)
    {
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(bus);
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var runtimeField = typeof(SuperMetroid.Core.Frontend.SuperMetroidGame).GetField("runtime", flags)!;
        runtimeField.SetValue(game, runtime);
        var draw = runtime.GetType().GetMethod("DrawGameplayActors", flags)!;
        byte[] Draw(SuperMetroid.Core.Runtime.SuperMetroidRuntime target)
        {
            target.Oam.BeginFrame();
            draw.Invoke(target, new object?[] { false, null, null, false });
            return target.Oam.LowTable.ToArray().Concat(target.Oam.HighTable.ToArray()).ToArray();
        }
        // Seed only a visible timed composition. The shared production actor draw, not
        // a duplicate test emitter, must bind all three owner passes to host content.
        foreach (ushort family in new ushort[] { 0x10, 0x700, 0x500 })
        {
            runtime.Projectiles.Reset(); runtime.BombProjectiles.Reset();
            if (family == 0x500)
            {
                var slot = runtime.BombProjectiles.Slots[0];
                slot.Type = family; slot.InstructionPointer = 1; slot.SpritemapPointer = sprite;
                slot.XPosition = (ushort)(runtime.Camera!.XPosition + 100);
                slot.YPosition = (ushort)(runtime.Camera.YPosition + 100);
            }
            else
            {
                var slot = runtime.Projectiles.Slots[0];
                slot.Type = family; slot.InstructionPointer = 1; slot.SpritemapPointer = sprite;
                slot.XPosition = (ushort)(runtime.Camera!.XPosition + 100);
                slot.YPosition = (ushort)(runtime.Camera.YPosition + 100);
            }
            game.BindProjectileCompositions(null);
            byte[] native = Draw(runtime);
            game.BindProjectileCompositions(stock);
            AssertTrue(native.SequenceEqual(Draw(runtime)), "Runtime stock composition OAM matches native");
            game.BindProjectileCompositions(edited);
            AssertTrue(!native.SequenceEqual(Draw(runtime)), "Edited composition reaches runtime actor drawing");
        }
        game.BindProjectileCompositions(null);
        using var without = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(without, game);
        game.BindProjectileCompositions(stock);
        using var with = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(with, game);
        AssertTrue(without.ToArray().SequenceEqual(with.ToArray()), "Projectile content excluded from frontend and runtime state graph");
        with.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Frontend.SuperMetroidGame>(with);
        var restoredRuntime = (SuperMetroid.Core.Runtime.SuperMetroidRuntime)runtimeField.GetValue(restored)!;
        AssertTrue(restoredRuntime.ProjectileCompositions is null, "Restored runtime requires external composition rebind");
        restored.BindProjectileCompositions(edited);
        game.BindProjectileCompositions(edited);
        AssertTrue(Draw(runtime).SequenceEqual(Draw(restoredRuntime)), "Restored graph draws current edited content after rebind");
        Console.WriteLine("Projectile runtime binding: three actor draw passes, stock parity, visual edits and nonserialized restore/rebind pass.");
    }

    private static void VerifyProjectileFiles(ISnesAddressSpace bus, byte[] editedJson)
    {
        string root = Path.GetFullPath(Path.Combine("csharp/test-temp", "projectile-install-" + Guid.NewGuid().ToString("N")));
        var installation = new GameInstallation(root);
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        var stock = installation.LoadProjectiles();
        AssertEqual(stock.StockSha256, stock.SelectedSha256, "Unmodified projectile identity");
        Directory.CreateDirectory(installation.ProjectileOverrideDirectory);
        string replacement = Path.Combine(installation.ProjectileOverrideDirectory, ProjectileSpriteDefinitions.FileName);
        File.WriteAllBytes(replacement, editedJson);
        var edited = installation.LoadProjectiles();
        AssertTrue(edited.SelectedSha256 != stock.SelectedSha256, "Selected projectile identity reflects override");
        AssertEqual(stock.StockSha256, edited.StockSha256, "Override preserves stock provenance");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        AssertTrue(File.ReadAllBytes(replacement).SequenceEqual(editedJson), "Re-extraction preserves external projectile override");
        AssertEqual(edited.SelectedSha256, installation.LoadProjectiles().SelectedSha256, "Re-extraction preserves selected content");
        File.WriteAllText(replacement, "broken");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Corrupt override never falls back");
        File.WriteAllBytes(replacement, editedJson);
        string stockPath = Path.Combine(installation.ProjectileDirectory, ProjectileSpriteDefinitions.FileName);
        File.WriteAllText(stockPath, "broken");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Override cannot hide corrupt stock");
        File.Move(stockPath, stockPath + ".invalid");
        AssertThrows<FileNotFoundException>(() => installation.LoadProjectiles(), "Override cannot hide missing stock");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        string manifestPath = Path.Combine(installation.ProjectileDirectory, ProjectilePresentationFiles.ManifestFileName);
        string manifest = File.ReadAllText(manifestPath);
        File.WriteAllText(manifestPath, manifest.Replace(SupportedCartridge.Sha256, new string('0', 64)));
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Wrong cartridge provenance rejected");
        File.WriteAllText(manifestPath, manifest.Insert(1, "\"version\":1,"));
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Duplicate manifest identity rejected");
        File.WriteAllText(manifestPath, manifest);
        Console.WriteLine("Projectile installation: stock/override identity, persistent edits, missing/corrupt content and manifest validation pass.");
    }
}
