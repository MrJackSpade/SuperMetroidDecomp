using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.BeamTileArtworkVerification;
using static SuperMetroid.Verification.Samus.Palettes.HyperBeamFxColorArtworkVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroProjectileArtworkVerification;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Runtime;

/// <summary>Checks imported projectile OAM against native records, production draw owners, and installed edits.</summary>
internal static class ProjectileCompositionsVerification
{
    /// <summary>Compares extracted bank-$93 sprites across coordinate wraps and verifies edits without draw-time ROM access.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyProjectileCompositions(SuperMetroidAddressSpace rom,
        bool compositionOnly = false, string sourceRom = "Super Metroid.smc")
    {
        byte[] json = ProjectileSpriteExtractor.Extract(rom);
        if (!compositionOnly)
        {
            VerifyBeamTileArtwork(rom);
            VerifyHyperBeamFxColorArtwork(rom);
        }
        var content = ProjectileSpriteCatalog.Load(new MemoryStream(json));
        Suite(nameof(VerifyProjectileCompositionOwners), () => VerifyProjectileCompositionOwners(content));
        int draws = 0;
        foreach (ushort id in ProjectileSpriteDefinitions.NativePointers)
        foreach (ushort origin in new ushort[] { 0, 1, 127, 255, 256, 0x7fff, 0xffff })
        foreach (int preceding in new[] { 0, 127 })
        {
            var native = new OamBuffer(); var authored = new OamBuffer();
            for (int i = 0; i < preceding; i++)
            {
                native.AddProjectileSpritePart(new(0), 0, new(0), new Position16<OamPixels>(0, 0));
                authored.AddProjectileSpritePart(new(0), 0, new(0), new Position16<OamPixels>(0, 0));
            }
            DrawImportedProjectileSpritemap(rom, native, id, new(origin, origin));
            content.Draw(id, authored, new(origin, origin));
            AssertTrue(native.LowTable.SequenceEqual(authored.LowTable), "Extracted composition retains all low OAM bytes");
            AssertTrue(native.HighTable.SequenceEqual(authored.HighTable), "Extracted composition retains all high OAM bits");
            AssertEqual(native.NextByteOffset, authored.NextByteOffset, "Extracted composition retains native capacity wrapping");
            draws++;
        }
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<ProjectileSpriteDocument>(json, options);
        ushort editedId = ProjectileSpriteDefinitions.NativePointers.First(id => document.Frames[ProjectileSpriteDefinitions.Name(id)].Length > 0);
        string name = ProjectileSpriteDefinitions.Name(editedId);
        var original = document.Frames[name][0];
        document.Frames[name][0] = original with { OffsetX = original.OffsetX == 255 ? 254 : original.OffsetX + 1 };
        byte[] editedJson = JsonSerializer.SerializeToUtf8Bytes(document, options);
        var edited = ProjectileSpriteCatalog.Load(new MemoryStream(editedJson));
        Suite(nameof(VerifyProjectileFiles), () => VerifyProjectileFiles(rom, editedJson));
        using var temporaryDirectory = new TestScratchDirectory("map-catalog");
        GameInstallation installation = GameAssetInstaller.Install(
            Path.GetFullPath(sourceRom), temporaryDirectory.Root);
        var roomAssets = new MapPresentationInstalledRoomAssets(installation);
        Suite(nameof(VerifyRuntimeProjectileCompositions), () => VerifyRuntimeProjectileCompositions(rom, content, edited, editedId,
            roomAssets, installation.LoadMaps()));
        if (!compositionOnly)
            VerifyIntroProjectileArtwork(rom, content, edited, editedId);
        var baselineOam = new OamBuffer(); var editedOam = new OamBuffer();
        content.Draw(editedId, baselineOam, new(100, 100)); edited.Draw(editedId, editedOam, new(100, 100));
        AssertEqual(unchecked((byte)(baselineOam.LowTable[0] + (original.OffsetX == 255 ? -1 : 1))), editedOam.LowTable[0], "Edited offset reaches emitted OAM");
        AssertTrue(!baselineOam.LowTable.SequenceEqual(editedOam.LowTable), "Visual edit is observable");
        document.Frames[name][0] = original;
        editedOam = new OamBuffer(); edited.Draw(editedId, editedOam, new(100, 100));
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
        AssertThrows<InvalidDataException>(() => content.Draw(0, new OamBuffer(), new(0, 0)), "Unknown sprite is loud");
        Console.WriteLine($"Projectile compositions: 417 extracted sprites/{draws} native OAM comparisons, observable edits, immutable load and invalid-field rejection pass without a draw-time ROM.");
    }

    /// <summary>Import-only bank-$93 record decoder for the projectile OAM oracle.</summary>
    [CpuBusBoundary("exact bus test: reference bank-$93 projectile spritemap decoder over a retail or synthetic cartridge")]
    internal static void DrawImportedProjectileSpritemap(ISnesAddressSpace source,
        OamBuffer oam, ushort pointer, Position16<OamPixels> origin)
    {
        int record = 0x930000 | pointer;
        ushort partCount = CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(record);
        int partAddress = SnesAddressMath.AddWithinBank(record, 2);
        for (int part = 0; part < partCount; part++)
        {
            oam.AddProjectileSpritePart(new SnesSpritemapXWord(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(partAddress)),
                source.ReadCartridgeByte(SnesAddressMath.AddWithinBank(partAddress, 2)),
                new SnesObjAttributeWord(CartridgeImportSource.Require(source).ReadCartridgeWordFixedBank(SnesAddressMath.AddWithinBank(partAddress, 3))),
                origin);
            partAddress = SnesAddressMath.AddWithinBank(partAddress, 5);
        }
    }

    /// <summary>Checks shot, explosion, and bomb draw owners preserve OAM, flicker, and projectile state without ROM reads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void VerifyProjectileCompositionOwners(ProjectileSpriteCatalog content)
    {
        var forbidden = NewProjectileCompositionForbiddenBus();
        int comparisons = 0;
        foreach (ushort id in ProjectileSpriteDefinitions.NativePointers)
        foreach (ushort coordinate in new ushort[] { 0, 100, 223, 255, 256, 303, 304, 0xffcf, 0xffd0 })
        foreach (ushort type in new ushort[] { 0, 0x10, 0x100, 0x200, 0x300, 0x500, 0x700, 0x800 })
        {
            var shots = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            var shot = shots.Slots[0];
            shot.Cursor = ProjectileFixtureCursors.Active; shot.SpritemapPointer = id;
            shot.Type = type; shot.Position = shot.Position.WithWhole(new(coordinate, coordinate));
            shot.Damage = 123; shot.Radius = shot.Radius.WithX(7).WithY(9);
            var bomb = bombs.Slots[0];
            bomb.Cursor = ProjectileFixtureCursors.Active; bomb.SpritemapPointer = id;
            bomb.Type = type; bomb.Position = bomb.Position.WithWhole(new(coordinate, coordinate));
            bomb.BombTimer = NativeCountdown.Start(1);
            foreach (ushort parity in new ushort[] { 0, 1, 2, 3 })
            {
                ProjectileDrawArtwork artwork = InstalledFixtures.ProjectileArtwork(content);
                var native = new SpriteFrameBuffer(); var extracted = new SpriteFrameBuffer();
                native.BeginFrame(); extracted.BeginFrame();
                shots.DrawLiveProjectiles(native, Position16<RoomPixels>.Zero, parity, artwork);
                shots.DrawExplosions(native, Position16<RoomPixels>.Zero, artwork);
                bombs.Draw(native, Position16<RoomPixels>.Zero, artwork);
                shots.DrawLiveProjectiles(extracted, Position16<RoomPixels>.Zero, parity, artwork);
                shots.DrawExplosions(extracted, Position16<RoomPixels>.Zero, artwork);
                bombs.Draw(extracted, Position16<RoomPixels>.Zero, artwork);
                AssertTrue(native.FinalizeAsOam().ImageEquals(extracted.FinalizeAsOam()),
                    "Production projectile composition preserves admission, flicker and OAM");
                AssertEqual(native.Draws.Length, extracted.Draws.Length, "Composition owner publishes the same draws");
                AssertEqual((ushort)123, shot.Damage, "Composition does not mutate damage");
                AssertEqual((ushort)7, shot.Radius.X, "Composition does not mutate X radius");
                AssertEqual((ushort)9, shot.Radius.Y, "Composition does not mutate Y radius");
                comparisons++;
            }
            if (type == 0x300)
            {
                bomb.BombTimer = NativeCountdown.Start(0);
                var hidden = new SpriteFrameBuffer();
                hidden.BeginFrame();
                bombs.Draw(hidden, Position16<RoomPixels>.Zero, InstalledFixtures.ProjectileArtwork(content));
                hidden.FinalizeFrame();
                AssertEqual(0, hidden.Draws.Length, "Detonated power bomb remains hidden with extracted art");
            }
        }
        Console.WriteLine($"Projectile composition owners: {comparisons} stock OAM comparisons with every ROM access forbidden.");
    }

    /// <summary>Rejects every cartridge read and bus write during installed composition drawing.</summary>
    internal static CartridgeReadGuard NewProjectileCompositionForbiddenBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Composition draw read ROM {address:X6}.")
            .RejectWrites(_ => "Composition draw mutated the bus.");

    /// <summary>Checks frontend gameplay drawing binds selected projectile art for each production owner family.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void VerifyRuntimeProjectileCompositions(ISnesAddressSpace bus,
        ProjectileSpriteCatalog stock, ProjectileSpriteCatalog edited, ushort sprite,
        MapPresentationInstalledRoomAssets roomAssets, AreaMapPresentationCatalog maps)
    {
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(bus, new SuperMetroid.Core.Gameplay.HostGameplayOverrides(), initialPaletteArt: roomAssets.InitialPalettes);
        runtime.Artwork.MapPresentation = maps;
        roomAssets.Bind(runtime);
        runtime.Artwork.ProjectileCompositions = stock;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(bus, new SuperMetroid.Core.Saves.SaveSlotStore());
        game.Session.runtime = runtime;
        byte[] Draw(SuperMetroid.Core.Runtime.SuperMetroidRuntime target)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(false, false);
            target.Sprites.FinalizeFrame();
            OamBuffer image = target.Sprites.FinalizedAsOam();
            return image.LowTable.ConcatToArray(image.HighTable);
        }
        // Seed only a visible timed composition. The shared production actor draw, not
        // a duplicate test emitter, must bind all three owner passes to host content.
        foreach (ushort family in new ushort[] { 0x10, 0x700, 0x500 })
        {
            runtime.Projectiles.Reset(); runtime.BombProjectiles.Reset();
            if (family == 0x500)
            {
                var slot = runtime.BombProjectiles.Slots[0];
                slot.Type = family; slot.Cursor = ProjectileFixtureCursors.Active; slot.SpritemapPointer = sprite;
                slot.Position = slot.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
            }
            else
            {
                var slot = runtime.Projectiles.Slots[0];
                slot.Type = family; slot.Cursor = ProjectileFixtureCursors.Active; slot.SpritemapPointer = sprite;
                slot.Position = slot.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
            }
            // Actor drawing also updates mutable visual state. Compare identical
            // captured states rather than two sequential draws of one runtime.
            var stockCopy = CloneGame();
            var editedCopy = CloneGame();
            byte[] baseline = Draw(runtime);
            stockCopy.BindArtwork(stockCopy.Artwork with
            {
                ProjectileCompositions = stock,
            });
            AssertTrue(baseline.SequenceEqual(Draw(stockCopy.Session.GameplayRuntime)),
                "Restored stock projectile actor matches the same initial state");
            editedCopy.BindArtwork(editedCopy.Artwork with
            {
                ProjectileCompositions = edited,
            });
            AssertTrue(!baseline.SequenceEqual(Draw(editedCopy.Session.GameplayRuntime)),
                "Edited composition reaches runtime actor drawing");
        }
        game.BindArtwork(game.Artwork with
        {
            ProjectileCompositions = null,
        });
        using var without = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(without, game);
        game.BindArtwork(game.Artwork with
        {
            ProjectileCompositions = stock,
        });
        using var with = new MemoryStream();
        SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(with, game);
        AssertTrue(without.WrittenBytes().SequenceEqual(with.WrittenBytes()), "Projectile content excluded from frontend and runtime state graph");
        with.Position = 0;
        var restored = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Frontend.SuperMetroidGame>(with);
        var restoredRuntime = restored.Session.GameplayRuntime;
        AssertTrue(restoredRuntime.Artwork.ProjectileCompositions is null, "Restored runtime requires external composition rebind");
        roomAssets.Bind(restoredRuntime);
        restoredRuntime.Artwork.MapPresentation = maps;
        restoredRuntime.Player.IntroducedSamus.Presentation.TileTransfers.BindArtwork(roomAssets.SamusBody);
        restoredRuntime.Player.IntroducedSamus.ArmCannon.Artwork = roomAssets.SamusBody.ArmCannon;
        restored.BindArtwork(restored.Artwork with
        {
            ProjectileCompositions = edited,
        });
        game.BindArtwork(game.Artwork with
        {
            ProjectileCompositions = edited,
        });
        AssertTrue(Draw(runtime).SequenceEqual(Draw(restoredRuntime)), "Restored graph draws current edited content after rebind");
        Console.WriteLine("Projectile runtime binding: three actor draw passes, stock parity, visual edits and nonserialized restore/rebind pass.");

        SuperMetroid.Core.Frontend.SuperMetroidGame CloneGame()
        {
            using var snapshot = new MemoryStream();
            SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer.Serialize(snapshot, game);
            snapshot.Position = 0;
            var clone = SuperMetroid.Diagnostics.SaveStates.DebuggerObjectGraphSerializer
                .Deserialize<SuperMetroid.Core.Frontend.SuperMetroidGame>(snapshot);
            var cloneRuntime = clone.Session.GameplayRuntime;
            roomAssets.Bind(cloneRuntime);
            cloneRuntime.Artwork.MapPresentation = maps;
            cloneRuntime.Player.IntroducedSamus.Presentation.TileTransfers.BindArtwork(roomAssets.SamusBody);
            cloneRuntime.Player.IntroducedSamus.ArmCannon.Artwork = roomAssets.SamusBody.ArmCannon;
            return clone;
        }
    }

    /// <summary>Checks stock digests, persistent overrides, invalid content, and stock repair in projectile installation.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    private static void VerifyProjectileFiles(ISnesAddressSpace bus, byte[] editedJson)
    {
        using var rootScratch = new TestScratchDirectory("projectile-install");
        string root = rootScratch.Root;
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
        string beamName = BeamTileAtlasDefinitions.FileName(0);
        string beamStock = Path.Combine(installation.ProjectileDirectory, beamName);
        string beamOverride = Path.Combine(installation.ProjectileOverrideDirectory, beamName);
        IndexedPngImage beamImage;
        using (var input = File.OpenRead(beamStock)) beamImage = IndexedPng.Read(input, new(64, 8));
        beamImage.Pixels[0] ^= 1;
        using (var output = File.Create(beamOverride))
            IndexedPng.Write(output, new(64, 8), beamImage.Pixels, beamImage.Palette);
        var beamEdited = installation.LoadProjectiles();
        AssertTrue(beamEdited.SelectedSha256 != edited.SelectedSha256, "PNG-only change updates aggregate content identity");
        AssertEqual((byte)(stock.BeamTiles.Resolve(BeamTileCatalog.AssetFor(0)).Span[0] ^ 0x80),
            beamEdited.BeamTiles.Resolve(BeamTileCatalog.AssetFor(0)).Span[0], "Installed beam PNG edit reaches compiled tile byte");
        byte[] savedPng = File.ReadAllBytes(beamOverride);
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        AssertTrue(savedPng.SequenceEqual(File.ReadAllBytes(beamOverride)), "Re-extraction preserves PNG override bytes");
        AssertEqual(beamEdited.SelectedSha256, installation.LoadProjectiles().SelectedSha256, "Re-extraction preserves PNG selection identity");
        File.WriteAllText(beamOverride, "broken png");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Corrupt beam PNG override rejected");
        File.WriteAllBytes(beamOverride, savedPng);
        File.WriteAllText(beamStock, "broken stock png");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "PNG override cannot conceal stock corruption");
        File.Move(beamStock, beamStock + ".invalid");
        AssertThrows<FileNotFoundException>(() => installation.LoadProjectiles(), "PNG override cannot conceal missing stock");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        string paletteStock = Path.Combine(installation.ProjectileDirectory, BeamPaletteDefinitions.FileName);
        string paletteOverride = Path.Combine(installation.ProjectileOverrideDirectory, BeamPaletteDefinitions.FileName);
        var paletteDocument = RequiredJson.Parse(File.ReadAllText(paletteStock));
        var red = paletteDocument.Required("palettes").Required(BeamPaletteDefinitions.Key(0)).Required(0);
        red["red"] = red.Required("red").GetValue<int>() ^ 1;
        File.WriteAllText(paletteOverride, paletteDocument.ToJsonString());
        var paletteEdited = installation.LoadProjectiles();
        AssertTrue(paletteEdited.SelectedSha256 != beamEdited.SelectedSha256, "Palette-only override changes content identity");
        File.WriteAllText(paletteStock, "broken stock palette");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Palette override cannot conceal stock corruption");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        AssertEqual(paletteEdited.SelectedSha256, installation.LoadProjectiles().SelectedSha256, "Stock repair preserves selected palette");
        File.WriteAllText(paletteOverride, "broken palette override");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Malformed palette override does not fall back");
        File.WriteAllText(paletteOverride, paletteDocument.ToJsonString());
        string hyperStock = Path.Combine(installation.ProjectileDirectory, HyperBeamFxColorFormat.FileName);
        string hyperOverride = Path.Combine(installation.ProjectileOverrideDirectory, HyperBeamFxColorFormat.FileName);
        var hyperDocument = RequiredJson.Parse(File.ReadAllText(hyperStock));
        var hyperRed = hyperDocument.Required("frames").Required(3).Required(2).Required("red");
        hyperDocument.Required("frames").Required(3).Required(2)["red"] = hyperRed.GetValue<int>() ^ 1;
        File.WriteAllText(hyperOverride, hyperDocument.ToJsonString());
        var hyperEdited = installation.LoadProjectiles();
        AssertTrue(hyperEdited.SelectedSha256 != paletteEdited.SelectedSha256,
            "Hyper Beam FX override changes selected identity");
        var hyperNative = new SnesCgram();
        var hyperSelected = new SnesCgram();
        (stock.BeamTiles.HyperBeamFxColors ?? throw new InvalidOperationException("Verification requires stock.BeamTiles.HyperBeamFxColors.")).Apply(hyperNative, 3, 225);
        (hyperEdited.BeamTiles.HyperBeamFxColors ?? throw new InvalidOperationException("Verification requires hyperEdited.BeamTiles.HyperBeamFxColors.")).Apply(hyperSelected, 3, 225);
        AssertEqual((ushort)(hyperNative.Colors[227].ToWord() ^ 1), hyperSelected.Colors[227],
            "Installed Hyper Beam FX override changes its selected color");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        AssertEqual(hyperEdited.SelectedSha256, installation.LoadProjectiles().SelectedSha256,
            "Re-extraction retains Hyper Beam FX override");
        File.WriteAllText(hyperStock, "broken stock Hyper Beam FX colors");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(),
            "Hyper Beam FX override cannot conceal stock corruption");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        File.WriteAllText(hyperOverride, "broken Hyper Beam FX override");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(),
            "Malformed Hyper Beam FX override does not fall back");
        File.WriteAllText(hyperOverride, hyperDocument.ToJsonString());
        string trailStock = Path.Combine(installation.ProjectileDirectory, ProjectileTrailVisualDefinitions.FileName);
        string trailOverride = Path.Combine(installation.ProjectileOverrideDirectory, ProjectileTrailVisualDefinitions.FileName);
        var trailDocument = RequiredJson.Parse(File.ReadAllText(trailStock));
        trailDocument.Required("frames").Required(ProjectileTrailVisualDefinitions.Name(ProjectileTrailVisualDefinitions.Frames[0]))["flipX"] = true;
        File.WriteAllText(trailOverride, trailDocument.ToJsonString());
        var trailEdited = installation.LoadProjectiles();
        AssertTrue(trailEdited.SelectedSha256 != paletteEdited.SelectedSha256, "Trail-only edit changes content identity");
        File.WriteAllText(trailStock, "broken stock trail");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Trail override cannot conceal stock corruption");
        File.Move(trailStock, trailStock + ".invalid");
        AssertThrows<FileNotFoundException>(() => installation.LoadProjectiles(), "Trail override cannot conceal missing stock");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        AssertEqual(trailEdited.SelectedSha256, installation.LoadProjectiles().SelectedSha256, "Stock repair preserves trail override");
        File.WriteAllText(trailOverride, "broken override");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Invalid trail override does not fall back");
        File.WriteAllText(trailOverride, trailDocument.ToJsonString());
        string trailPngStock = Path.Combine(installation.ProjectileDirectory, ProjectileTrailAtlasDefinitions.FileName);
        string trailPngOverride = Path.Combine(installation.ProjectileOverrideDirectory, ProjectileTrailAtlasDefinitions.FileName);
        var trailImage = IndexedPng.Read(new MemoryStream(File.ReadAllBytes(trailPngStock)), new(ProjectileTrailAtlasDefinitions.Width, ProjectileTrailAtlasDefinitions.Height));
        trailImage.Pixels[0] ^= 1;
        using var trailPng = new MemoryStream();
        IndexedPng.Write(trailPng, new(trailImage.Size.X, trailImage.Size.Y), trailImage.Pixels, trailImage.Palette);
        File.WriteAllBytes(trailPngOverride, trailPng.ToArray());
        var pngEdited = installation.LoadProjectiles();
        AssertTrue(pngEdited.SelectedSha256 != trailEdited.SelectedSha256, "Trail PNG-only edit changes identity");
        AssertTrue(!(pngEdited.Trails.Tiles ?? throw new InvalidOperationException("Verification requires pngEdited.Trails.Tiles.")).IceAndWave.Span.SequenceEqual((trailEdited.Trails.Tiles ?? throw new InvalidOperationException("Verification requires trailEdited.Trails.Tiles.")).IceAndWave.Span), "Installed trail PNG edit reaches selected catalog");
        File.WriteAllText(trailPngStock, "broken PNG");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Trail PNG override cannot hide broken stock");
        File.Move(trailPngStock, trailPngStock + ".invalid");
        AssertThrows<FileNotFoundException>(() => installation.LoadProjectiles(), "Trail PNG override cannot hide missing stock");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        AssertEqual(pngEdited.SelectedSha256, installation.LoadProjectiles().SelectedSha256, "Repair preserves trail PNG override");
        File.WriteAllText(trailPngOverride, "invalid override");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Malformed trail PNG override never falls back");
        File.WriteAllBytes(trailPngOverride, trailPng.ToArray());
        string flareStock = Path.Combine(installation.ProjectileDirectory, ChargeFlarePlacementDefinitions.FileName);
        string flareOverride = Path.Combine(installation.ProjectileOverrideDirectory, ChargeFlarePlacementDefinitions.FileName);
        var flareDocument = RequiredJson.Parse(File.ReadAllText(flareStock));
        flareDocument.Required("offsets").Required(ChargeFlarePlacementDefinitions.Key(false, 0))["x"] = 17;
        File.WriteAllText(flareOverride, flareDocument.ToJsonString());
        var flareEdited = installation.LoadProjectiles();
        AssertTrue(flareEdited.SelectedSha256 != pngEdited.SelectedSha256, "Flare-only edit changes content identity");
        AssertEqual(17, flareEdited.FlarePlacement.Resolve(false, 0).X, "Installer selects edited flare offset");
        File.WriteAllText(flareStock, "broken stock");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Flare override cannot hide stock corruption");
        File.Move(flareStock, flareStock + ".invalid");
        AssertThrows<FileNotFoundException>(() => installation.LoadProjectiles(), "Flare override cannot hide missing stock");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        AssertEqual(flareEdited.SelectedSha256, installation.LoadProjectiles().SelectedSha256, "Repair preserves flare override");
        File.WriteAllText(flareOverride, "broken override");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Invalid flare override never falls back");
        File.WriteAllText(flareOverride, flareDocument.ToJsonString());
        string compositionStock = Path.Combine(installation.ProjectileDirectory, ChargeFlareSpriteDefinitions.FileName);
        string compositionOverride = Path.Combine(installation.ProjectileOverrideDirectory, ChargeFlareSpriteDefinitions.FileName);
        var compositionDocument = RequiredJson.Parse(File.ReadAllText(compositionStock));
        string firstFlare = ProjectileSpriteDefinitions.Name(ChargeFlareSpriteDefinitions.Selectors[0]);
        compositionDocument.Required("frames").Required(firstFlare).Required(0)["offsetX"] = 17;
        File.WriteAllText(compositionOverride, compositionDocument.ToJsonString());
        var compositionEdited = installation.LoadProjectiles();
        AssertTrue(compositionEdited.SelectedSha256 != flareEdited.SelectedSha256, "Composition-only flare edit changes content identity");
        var flareOam = new OamBuffer();
        compositionEdited.FlareCompositions.Draw(0, flareOam, new(100, 100));
        AssertEqual(117, flareOam.GetEntry(0).X, "Installed flare composition emits selected part offset");
        File.WriteAllText(compositionStock, "broken stock");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Flare composition override cannot conceal corrupt stock");
        File.Move(compositionStock, compositionStock + ".invalid");
        AssertThrows<FileNotFoundException>(() => installation.LoadProjectiles(), "Flare composition override cannot conceal missing stock");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        AssertEqual(compositionEdited.SelectedSha256, installation.LoadProjectiles().SelectedSha256, "Stock repair preserves flare composition override");
        File.WriteAllText(compositionOverride, "broken override");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Invalid flare composition override never falls back");
        File.WriteAllText(compositionOverride, compositionDocument.ToJsonString());
        string grappleStock = Path.Combine(installation.ProjectileDirectory, GrappleTileDefinitions.FileName);
        string grappleOverride = Path.Combine(installation.ProjectileOverrideDirectory, GrappleTileDefinitions.FileName);
        var grappleImage = IndexedPng.Read(new MemoryStream(File.ReadAllBytes(grappleStock)), new(GrappleTileDefinitions.Width, GrappleTileDefinitions.Height));
        grappleImage.Pixels[0] ^= 1;
        using var grapplePng = new MemoryStream();
        IndexedPng.Write(grapplePng, new(grappleImage.Size.X, grappleImage.Size.Y), grappleImage.Pixels, grappleImage.Palette);
        File.WriteAllBytes(grappleOverride, grapplePng.ToArray());
        var grappleEdited = installation.LoadProjectiles();
        AssertTrue(grappleEdited.SelectedSha256 != compositionEdited.SelectedSha256, "Grapple PNG-only edit changes content identity");
        AssertEqual((byte)(compositionEdited.GrappleTiles.Resolve(VramAssetId.GrapplePointFirstTiles).Span[0] ^ 128),
            grappleEdited.GrappleTiles.Resolve(VramAssetId.GrapplePointFirstTiles).Span[0], "Installed Grapple PNG selects edited endpoint pixel");
        File.WriteAllText(grappleStock, "broken PNG");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Grapple override cannot conceal corrupt stock");
        File.Move(grappleStock, grappleStock + ".invalid");
        AssertThrows<FileNotFoundException>(() => installation.LoadProjectiles(), "Grapple override cannot conceal missing stock");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        AssertEqual(grappleEdited.SelectedSha256, installation.LoadProjectiles().SelectedSha256, "Stock repair preserves Grapple PNG override");
        File.WriteAllText(grappleOverride, "broken override");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Invalid Grapple PNG override never falls back");
        File.WriteAllBytes(grappleOverride, grapplePng.ToArray());
        string grappleStyleStock = Path.Combine(installation.ProjectileDirectory, GrappleSpriteDefinitions.FileName);
        string grappleStyleOverride = Path.Combine(installation.ProjectileOverrideDirectory, GrappleSpriteDefinitions.FileName);
        var grappleStyle = RequiredJson.Parse(File.ReadAllText(grappleStyleStock));
        grappleStyle.Required("endpoint")["palette"] = 4;
        File.WriteAllText(grappleStyleOverride, grappleStyle.ToJsonString());
        var styleEdited = installation.LoadProjectiles();
        AssertTrue(styleEdited.SelectedSha256 != grappleEdited.SelectedSha256, "Grapple style-only edit changes content identity");
        AssertEqual((ushort)((grappleEdited.GrappleTiles.Sprites ?? throw new InvalidOperationException("Verification requires grappleEdited.GrappleTiles.Sprites.")).Endpoint ^ 512), (styleEdited.GrappleTiles.Sprites ?? throw new InvalidOperationException("Verification requires styleEdited.GrappleTiles.Sprites.")).Endpoint, "Installed Grapple style selects edited palette");
        File.WriteAllText(grappleStyleStock, "broken stock");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Grapple style override cannot hide corrupt stock");
        File.Move(grappleStyleStock, grappleStyleStock + ".invalid");
        AssertThrows<FileNotFoundException>(() => installation.LoadProjectiles(), "Grapple style override cannot hide missing stock");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        AssertEqual(styleEdited.SelectedSha256, installation.LoadProjectiles().SelectedSha256, "Repair preserves Grapple style override");
        File.WriteAllText(grappleStyleOverride, "broken override");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Invalid Grapple style never falls back");
        File.WriteAllText(grappleStyleOverride, grappleStyle.ToJsonString());
        string grappleFlareStock = Path.Combine(installation.ProjectileDirectory, GrappleFlarePlacementDefinitions.FileName);
        string grappleFlareOverride = Path.Combine(installation.ProjectileOverrideDirectory, GrappleFlarePlacementDefinitions.FileName);
        var grappleFlare = RequiredJson.Parse(File.ReadAllText(grappleFlareStock));
        grappleFlare.Required("offsets").Required(ChargeFlarePlacementDefinitions.Key(false, 0))["x"] = 17;
        File.WriteAllText(grappleFlareOverride, grappleFlare.ToJsonString());
        var grappleFlareEdited = installation.LoadProjectiles();
        AssertTrue(grappleFlareEdited.SelectedSha256 != styleEdited.SelectedSha256, "Grapple flare-only edit changes content identity");
        AssertEqual((short)17, (grappleFlareEdited.GrappleTiles.FlarePlacement ?? throw new InvalidOperationException("Verification requires grappleFlareEdited.GrappleTiles.FlarePlacement.")).Resolve(false, 0).X, "Installer selects Grapple flare offset");
        File.WriteAllText(grappleFlareStock, "broken stock");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Grapple flare override cannot hide corrupt stock");
        File.Move(grappleFlareStock, grappleFlareStock + ".invalid");
        AssertThrows<FileNotFoundException>(() => installation.LoadProjectiles(), "Grapple flare override cannot hide missing stock");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        AssertEqual(grappleFlareEdited.SelectedSha256, installation.LoadProjectiles().SelectedSha256, "Repair preserves Grapple flare override");
        File.WriteAllText(grappleFlareOverride, "broken override");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Invalid Grapple flare never falls back");
        File.WriteAllText(grappleFlareOverride, grappleFlare.ToJsonString());
        string swingStock = Path.Combine(installation.ProjectileDirectory, GrappleSwingFrameDefinitions.FileName);
        string swingOverride = Path.Combine(installation.ProjectileOverrideDirectory, GrappleSwingFrameDefinitions.FileName);
        var swing = RequiredJson.Parse(File.ReadAllText(swingStock));
        swing.Required("frames")[0] = 17;
        File.WriteAllText(swingOverride, swing.ToJsonString());
        var swingEdited = installation.LoadProjectiles();
        AssertTrue(swingEdited.SelectedSha256 != grappleFlareEdited.SelectedSha256, "Swing-only edit changes content identity");
        AssertEqual((byte)17, (swingEdited.GrappleTiles.SwingFrames ?? throw new InvalidOperationException("Verification requires swingEdited.GrappleTiles.SwingFrames.")).Resolve(0), "Installer selects edited swing frame");
        File.WriteAllText(swingStock, "broken stock");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Swing override cannot hide corrupt stock");
        File.Move(swingStock, swingStock + ".invalid");
        AssertThrows<FileNotFoundException>(() => installation.LoadProjectiles(), "Swing override cannot hide missing stock");
        ProjectilePresentationFiles.Extract(bus, installation.ProjectileDirectory);
        AssertEqual(swingEdited.SelectedSha256, installation.LoadProjectiles().SelectedSha256, "Repair preserves swing override");
        File.WriteAllText(swingOverride, "broken override");
        AssertThrows<InvalidDataException>(() => installation.LoadProjectiles(), "Invalid swing override never falls back");
        Console.WriteLine("Projectile installation: stock/override identity, persistent edits, missing/corrupt content and manifest validation pass.");
    }

    /// <summary>Runs native projectile composition verification against the pinned cartridge.</summary>
    [VerificationCommand("--projectile-compositions-fixture")]
    internal static int ProjectileCompositionsFixtureCommand()
    {
        Suite(nameof(VerifyProjectileCompositions), () => VerifyProjectileCompositions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
