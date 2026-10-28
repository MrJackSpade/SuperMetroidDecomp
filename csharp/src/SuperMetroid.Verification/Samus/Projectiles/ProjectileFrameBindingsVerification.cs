using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Programs;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks editable projectile sprite bindings independently of timed mechanics.</summary>
internal static class ProjectileFrameBindingsVerification
{
    /// <summary>Checks 805 native sprite references, both projectile handlers, and a visible override without ROM fallback.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyProjectileFrameBindings(ISnesAddressSpace rom)
    {
        byte[] stockJson = ProjectileFrameBindingExtractor.Extract(rom);
        var stock = ProjectileFrameBindingCatalog.Load(new MemoryStream(stockJson, writable: false));
        AssertEqual(ProjectileFrameBindingFormat.FrameCount,
            SamusProjectileRecordDefinitions.TimedRecordAddresses.Count,
            "physical timed records and visual frame bindings have identical coverage");
        var blocked = new HashSet<int>();
        foreach (ushort pointer in SamusProjectileRecordDefinitions.TimedRecordAddresses)
        {
            int address = SamusSourceDefinitions.ProjectileDataBank.Value | unchecked((ushort)(pointer + 2));
            blocked.Add(address);
            blocked.Add(SamusSourceDefinitions.ProjectileDataBank.Value | unchecked((ushort)(pointer + 3)));
            ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), address);
            AssertEqual(native, stock.Resolve(pointer),
                $"timed projectile $93:{pointer:X4} keeps its native sprite reference");
        }
        var guard = NewProjectileFrameBindingReadGuard(rom, blocked);
        foreach (ushort pointer in SamusProjectileRecordDefinitions.TimedRecordAddresses)
        {
            int record = SamusSourceDefinitions.ProjectileDataBank.Value | pointer;
            ushort Word(int offset) => LittleEndianBytes.Word(CpuBusRegionRead.Read(rom, record + offset), CpuBusRegionRead.Read(rom, record + offset + 1));
            var nativeShot = new SamusProjectileSlot(0)
            {
                Cursor = SamusProjectileProgramAddresses.CursorAt(unchecked((ushort)(pointer + 8))),
                InstructionTimer = WrappingCountdown.FromRaw(Word(0)), SpritemapPointer = Word(2),
                Radius = new(CpuBusRegionRead.Read(rom, record + 4), CpuBusRegionRead.Read(rom, record + 5)),
                AnimationFrame = Word(6), Damage = 30,
            };
            var nativeBomb = new SamusBombProjectileSlot(0)
            {
                Cursor = nativeShot.Cursor,
                InstructionTimer = nativeShot.InstructionTimer,
                SpritemapPointer = nativeShot.SpritemapPointer,
                Radius = new(nativeShot.Radius.X, nativeShot.Radius.Y),
                Type = 0x0500, Damage = 30,
            };
            var installedShot = new SamusProjectileSlot(0)
                { Cursor = SamusProjectileProgramAddresses.CursorAt(pointer), InstructionTimer = WrappingCountdown.Start(1), Damage = 30 };
            var installedBomb = new SamusBombProjectileSlot(0)
                { Cursor = SamusProjectileProgramAddresses.CursorAt(pointer), InstructionTimer = WrappingCountdown.Start(1), Type = 0x0500, Damage = 30 };
            var shots = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None) { Animation = { FrameBindings = stock } };
            var bombs = new SamusBombProjectileSystem { FrameBindings = stock };
            AssertTrue(nativeShot.InstructionTimer.RawValue is > 0 and < 0x8000,
                "native inventory contains timed records");
            AssertEqual(false, shots.Animation.RunProjectileInstructionHandler(installedShot),
                "installed projectile timed record survives");
            AssertEqual(false, bombs.RunProjectileInstructionHandler(installedBomb),
                "installed bomb timed record survives");
            AssertEqual(nativeShot.SpritemapPointer, installedShot.SpritemapPointer,
                "installed projectile frame selects the native sprite");
            AssertEqual(nativeBomb.SpritemapPointer, installedBomb.SpritemapPointer,
                "installed bomb frame selects the native sprite");
            AssertEqual(nativeShot.InstructionTimer.RawValue, installedShot.InstructionTimer.RawValue,
                "projectile duration is independent of sprite binding");
            AssertEqual(nativeShot.InstructionPointer, installedShot.InstructionPointer,
                "projectile flow is independent of sprite binding");
            AssertEqual(nativeShot.Radius.X, installedShot.Radius.X,
                "projectile X radius is independent of sprite binding");
            AssertEqual(nativeShot.Radius.Y, installedShot.Radius.Y,
                "projectile Y radius is independent of sprite binding");
            AssertEqual(nativeBomb.InstructionTimer.RawValue, installedBomb.InstructionTimer.RawValue,
                "bomb duration is independent of sprite binding");
            AssertEqual(nativeBomb.Radius.X, installedBomb.Radius.X,
                "bomb X radius is independent of sprite binding");
            AssertEqual(nativeBomb.Radius.Y, installedBomb.Radius.Y,
                "bomb Y radius is independent of sprite binding");
        }

        var document = RequiredJson.Deserialize<ProjectileFrameBindingDocument>(stockJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        ushort editedPointer = SamusProjectileRecordDefinitions.TimedRecordAddresses[0];
        string frameName = ProjectileFrameBindingFormat.FrameNameAt(editedPointer);
        ushort originalSprite = stock.Resolve(editedPointer);
        ProjectileSpriteCatalog compositions = ProjectileSpriteCatalog.Load(
            new MemoryStream(ProjectileSpriteExtractor.Extract(rom)));
        var stockOam = new OamBuffer();
        compositions.Draw(originalSprite, stockOam, new(100, 100));
        ushort replacement = ProjectileSpriteDefinitions.NativePointers.First(sprite =>
        {
            var comparison = new OamBuffer();
            compositions.Draw(sprite, comparison, new(100, 100));
            return !comparison.LowTable.SequenceEqual(stockOam.LowTable) ||
                !comparison.HighTable.SequenceEqual(stockOam.HighTable);
        });
        document.Frames[frameName] = ProjectileSpriteDefinitions.Name(replacement);
        byte[] editedJson = ProjectileFrameBindingCatalog.Write(document);
        var edited = ProjectileFrameBindingCatalog.Load(new MemoryStream(editedJson));
        var editedShot = new SamusProjectileSlot(0)
            { Cursor = SamusProjectileProgramAddresses.CursorAt(editedPointer), InstructionTimer = WrappingCountdown.Start(1), Damage = 30 };
        new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None) { Animation = { FrameBindings = edited } }
            .Animation.RunProjectileInstructionHandler(editedShot);
        AssertEqual(replacement, editedShot.SpritemapPointer,
            "edited frame binding reaches the actual projectile instruction handler");
        var editedRecord = SamusProjectileProgramAddresses.CursorAt(editedPointer) ?? throw new InvalidDataException("A record address is never zero.");
        AssertEqual(AssertIs<AnimationProgramStep<SamusProjectileProgram, SamusProjectileFrame, NoAnimationCue>.ShowStep>(SamusProjectileAnimations.Programs.Steps(editedRecord.Program)[editedRecord.Position], "edited record step").Duration,
            (int)editedShot.InstructionTimer.RawValue, "visual edit does not change frame duration");
        var editedOam = new OamBuffer();
        compositions.Draw(checked((ushort)editedShot.SpritemapPointer), editedOam, new(100, 100));
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

    /// <summary>Rejects reads of migrated sprite-reference operands in timed projectile records.</summary>
    private static CartridgeReadGuard NewProjectileFrameBindingReadGuard(ISnesAddressSpace source, HashSet<int> blocked) =>
        new CartridgeReadGuard(source).Forbid(blocked.Contains,
            address => new InvalidDataException($"Installed projectile frame read ROM sprite reference ${address:X6}."));

    /// <summary>Runs the projectile frame binding and override checks.</summary>
    [VerificationCommand("--projectile-frame-bindings")]
    internal static int ProjectileFrameBindingsCommand()
    {
        Suite(nameof(VerifyProjectileFrameBindings), () => VerifyProjectileFrameBindings(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
