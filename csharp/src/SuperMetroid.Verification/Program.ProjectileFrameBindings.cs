using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyProjectileFrameBindings(ISnesAddressSpace rom)
    {
        byte[] stockJson = ProjectileFrameBindingExtractor.Extract(rom);
        var stock = ProjectileFrameBindingCatalog.Load(new MemoryStream(stockJson, writable: false));
        AssertEqual(ProjectileFrameBindingFormat.FrameCount,
            SamusProjectileRadiusDefinitions.TimedRecordPointers.Count,
            "physical timed records and visual frame bindings have identical coverage");
        var blocked = new HashSet<int>();
        foreach (ushort pointer in SamusProjectileRadiusDefinitions.TimedRecordPointers)
        {
            int address = SamusProjectileRomData.Banks.Projectile | unchecked((ushort)(pointer + 2));
            blocked.Add(address);
            blocked.Add(SamusProjectileRomData.Banks.Projectile | unchecked((ushort)(pointer + 3)));
            ushort native = RomDataReader.ReadWordFixedBank(rom, address);
            AssertEqual(native, stock.Resolve(pointer),
                $"timed projectile $93:{pointer:X4} keeps its native sprite reference");
        }
        var guard = new ProjectileFrameBindingReadGuard(rom, blocked);
        var runBomb = typeof(SamusBombProjectileSystem).GetMethod("RunProjectileInstructionHandler",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (ushort pointer in SamusProjectileRadiusDefinitions.TimedRecordPointers)
        {
            var nativeShot = new SamusProjectileSlot(0)
                { InstructionPointer = pointer, InstructionTimer = 1, Damage = 30 };
            var installedShot = new SamusProjectileSlot(0)
                { InstructionPointer = pointer, InstructionTimer = 1, Damage = 30 };
            var nativeBomb = new SamusBombProjectileSlot(0)
                { InstructionPointer = pointer, InstructionTimer = 1, Type = 0x0500, Damage = 30 };
            var installedBomb = new SamusBombProjectileSlot(0)
                { InstructionPointer = pointer, InstructionTimer = 1, Type = 0x0500, Damage = 30 };
            var shots = new SamusProjectileSystem { FrameBindings = stock };
            var bombs = new SamusBombProjectileSystem { FrameBindings = stock };
            var nativeShots = new SamusProjectileSystem();
            var nativeBombs = new SamusBombProjectileSystem();
            AssertEqual(nativeShots.RunProjectileInstructionHandler(rom, nativeShot),
                shots.RunProjectileInstructionHandler(guard, installedShot),
                "installed projectile frame keeps native timed/deletion result");
            AssertEqual((bool)runBomb.Invoke(nativeBombs, [rom, nativeBomb])!,
                (bool)runBomb.Invoke(bombs, [guard, installedBomb])!,
                "installed bomb frame keeps native timed/deletion result");
            AssertEqual(nativeShot.SpritemapPointer, installedShot.SpritemapPointer,
                "installed projectile frame selects the native sprite");
            AssertEqual(nativeBomb.SpritemapPointer, installedBomb.SpritemapPointer,
                "installed bomb frame selects the native sprite");
            AssertEqual(nativeShot.InstructionTimer, installedShot.InstructionTimer,
                "projectile duration is independent of sprite binding");
            AssertEqual(nativeShot.InstructionPointer, installedShot.InstructionPointer,
                "projectile flow is independent of sprite binding");
            AssertEqual(nativeShot.XRadius, installedShot.XRadius,
                "projectile X radius is independent of sprite binding");
            AssertEqual(nativeShot.YRadius, installedShot.YRadius,
                "projectile Y radius is independent of sprite binding");
            AssertEqual(nativeBomb.InstructionTimer, installedBomb.InstructionTimer,
                "bomb duration is independent of sprite binding");
            AssertEqual(nativeBomb.XRadius, installedBomb.XRadius,
                "bomb X radius is independent of sprite binding");
            AssertEqual(nativeBomb.YRadius, installedBomb.YRadius,
                "bomb Y radius is independent of sprite binding");
        }

        var document = JsonSerializer.Deserialize<ProjectileFrameBindingDocument>(stockJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        ushort editedPointer = SamusProjectileRadiusDefinitions.TimedRecordPointers[0];
        string frameName = ProjectileFrameBindingFormat.FrameName(editedPointer);
        ushort originalSprite = stock.Resolve(editedPointer);
        ProjectileSpriteCatalog compositions = ProjectileSpriteCatalog.Load(
            new MemoryStream(ProjectileSpriteExtractor.Extract(rom)));
        var stockOam = new OamBuffer();
        compositions.Draw(originalSprite, stockOam, 100, 100);
        ushort replacement = ProjectileSpriteDefinitions.NativePointers.ToArray().First(sprite =>
        {
            var comparison = new OamBuffer();
            compositions.Draw(sprite, comparison, 100, 100);
            return !comparison.LowTable.SequenceEqual(stockOam.LowTable) ||
                !comparison.HighTable.SequenceEqual(stockOam.HighTable);
        });
        document.Frames[frameName] = ProjectileSpriteDefinitions.Name(replacement);
        byte[] editedJson = ProjectileFrameBindingCatalog.Write(document);
        var edited = ProjectileFrameBindingCatalog.Load(new MemoryStream(editedJson));
        var editedShot = new SamusProjectileSlot(0)
            { InstructionPointer = editedPointer, InstructionTimer = 1, Damage = 30 };
        new SamusProjectileSystem { FrameBindings = edited }
            .RunProjectileInstructionHandler(guard, editedShot);
        AssertEqual(replacement, editedShot.SpritemapPointer,
            "edited frame binding reaches the actual projectile instruction handler");
        AssertEqual(SamusProjectileInstructionDefinitions.ReadWord(
                SamusProjectileRomData.Banks.Projectile | editedPointer),
            editedShot.InstructionTimer, "visual edit does not change frame duration");
        var editedOam = new OamBuffer();
        compositions.Draw(editedShot.SpritemapPointer, editedOam, 100, 100);
        AssertTrue(!editedOam.LowTable.SequenceEqual(stockOam.LowTable) ||
            !editedOam.HighTable.SequenceEqual(stockOam.HighTable),
            "edited visual binding changes emitted OAM, not merely a cached pointer");

        string root = Path.Combine(Path.GetFullPath("csharp/test-temp"),
            "projectile-frame-bindings-" + Guid.NewGuid().ToString("N"));
        try
        {
            ProjectilePresentationFiles.Extract(rom, root);
            InstalledProjectilePresentation baseline = ProjectilePresentationFiles.Load(root, null);
            AssertEqual(originalSprite, baseline.FrameBindings.Resolve(editedPointer),
                "installed stock frame binding selects native sprite");
            string overrides = Path.Combine(root, "overrides");
            Directory.CreateDirectory(overrides);
            File.WriteAllBytes(Path.Combine(overrides, ProjectileFrameBindingFormat.FileName), editedJson);
            InstalledProjectilePresentation selected = ProjectilePresentationFiles.Load(root, overrides);
            AssertEqual(replacement, selected.FrameBindings.Resolve(editedPointer),
                "installed visual override selects alternate sprite");
            AssertTrue(baseline.SelectedSha256 != selected.SelectedSha256,
                "visual frame override changes diagnostic content identity");
            AssertEqual(replacement, ProjectilePresentationFiles.Load(root, overrides)
                .FrameBindings.Resolve(editedPointer), "visual override persists across catalog reload");
            File.WriteAllBytes(Path.Combine(overrides, ProjectileFrameBindingFormat.FileName), new byte[] { 0 });
            AssertThrows<InvalidDataException>(() => ProjectilePresentationFiles.Load(root, overrides),
                "invalid frame-binding override fails loudly");
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
        document.Frames.Remove(frameName);
        AssertThrows<InvalidDataException>(() => ProjectileFrameBindingCatalog.Write(document),
            "missing timed frame fails before gameplay");
        document.Frames[frameName] = "sprite_0000";
        AssertThrows<InvalidDataException>(() => ProjectileFrameBindingCatalog.Write(document),
            "uncatalogued sprite selection fails before gameplay");
        Console.WriteLine("Projectile frame bindings: 805 timed records, both handlers, observable OAM edit, identity, and malformed resources pass.");
    }

    private sealed class ProjectileFrameBindingReadGuard(ISnesAddressSpace source, HashSet<int> blocked)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) => blocked.Contains(address)
            ? throw new InvalidDataException($"Installed projectile frame read ROM sprite reference ${address:X6}.")
            : source.ReadByte(address);
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
