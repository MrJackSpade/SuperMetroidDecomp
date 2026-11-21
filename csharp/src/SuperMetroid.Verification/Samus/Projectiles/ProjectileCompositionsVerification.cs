using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
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
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Gameplay;

/// <summary>Checks imported projectile OAM against native records and production draw owners.</summary>
internal static class ProjectileCompositionsVerification
{
    /// <summary>Compares extracted bank-$93 sprites across coordinate wraps without draw-time ROM access, and rejects malformed compositions.</summary>
    internal static void VerifyProjectileCompositions(CartridgeImportAddressSpace rom,
        bool compositionOnly = false)
    {
        byte[] json = ProjectileSpriteExtractor.Extract(rom);
        if (!compositionOnly)
        {
            VerifyBeamTileArtwork(rom);
            VerifyHyperBeamFxColorArtwork(rom);
        }
        var content = InstalledProjectileSprites.Load(new MemoryStream(json));
        Suite(nameof(VerifyProjectileCompositionOwners), () => VerifyProjectileCompositionOwners());
        int draws = 0;
        foreach (ushort id in ProjectileSpriteDefinitions.NativePointers)
        foreach (ushort origin in new ushort[] { 0, 1, 127, 255, 256, 0x7fff, 0xffff })
        foreach (int preceding in new[] { 0, 127 })
        {
            var native = new OamBuffer(); var authored = new OamBuffer();
            for (int i = 0; i < preceding; i++)
            {
                native.AddProjectileSpritePart(new(0), 0, new(0), new Position16<ScreenPixels>(0, 0));
                authored.AddProjectileSpritePart(new(0), 0, new(0), new Position16<ScreenPixels>(0, 0));
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
        VerifyCompositionsWrittenOnce(rom, document, options);
        ushort drawnId = ProjectileSpriteDefinitions.NativePointers.First(id => document.Frames[ProjectileSpriteDefinitions.Name(id)].Length > 0);
        string name = ProjectileSpriteDefinitions.Name(drawnId);
        var original = document.Frames[name][0];
        var roomAssets = MapPresentationInstalledRoomAssets.Shared;
        Suite(nameof(VerifyRuntimeProjectileSheets), () => VerifyRuntimeProjectileSheets(drawnId, InstalledFixtures.SpriteSheets,
            roomAssets, InstalledFixtures.Maps));
        if (!compositionOnly)
            VerifyIntroProjectileArtwork(drawnId);

        void Reject(ProjectileSpriteDocument bad) => AssertThrows<InvalidDataException>(() => InstalledProjectileSprites.Load(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(bad, options))), "Malformed composition rejected");
        Reject(document with { Version = ProjectileSpriteDefinitions.Version + 1 });
        foreach (var badPart in new[] { original with { Palette = null }, original with { TileColumn = 16 }, original with { TileRow = 32 }, original with { OffsetX = 256 }, original with { OffsetY = -129 }, original with { Size = 32 } })
        {
            document.Frames[name][0] = badPart; Reject(document);
        }
        document.Frames.Remove(name); Reject(document);
        foreach (string field in new[] { "\"version\":1,", "\"damage\":10," })
        {
            byte[] malformed = System.Text.Encoding.UTF8.GetBytes(System.Text.Encoding.UTF8.GetString(json).Insert(1, field));
            AssertThrows<InvalidDataException>(() => InstalledProjectileSprites.Load(new MemoryStream(malformed)), "Duplicate or unknown mechanics fields rejected");
        }
        AssertThrows<InvalidDataException>(() => content.Draw(0, new OamBuffer(), new(0, 0)), "Unknown sprite is loud");
        Console.WriteLine($"Projectile compositions: {ProjectileSpriteDefinitions.NativePointers.Count} stored records as {ProjectileSpriteDefinitions.All.Count} compositions/{draws} native OAM comparisons and invalid-field rejection pass without a draw-time ROM.");
    }

    /// <summary>
    /// Checks that each composition is written once (#1635, #2295): no two installed compositions hold the same parts, a record
    /// storing a composition again, byte for byte, holds that composition, and the extractor refuses a cartridge whose records
    /// would write one twice or whose copy differs from the composition it is held as.
    /// </summary>
    private static void VerifyCompositionsWrittenOnce(CartridgeImportAddressSpace rom, ProjectileSpriteDocument document,
        JsonSerializerOptions options)
    {
        AssertEqual(ProjectileSpriteDefinitions.All.Count, document.Frames.Count, "one installed composition per distinct composition");
        AssertEqual(document.Frames.Count, document.Frames.Values.Select(parts => JsonSerializer.Serialize(parts, options)).Distinct().Count(),
            "no two installed compositions hold the same parts");
        AssertEqual(ProjectileSpriteNative.Projectile(0xed9e), ProjectileSpriteNative.Projectile(0xedca),
            "charged Ice's two stored crosses are one composition");
        AssertEqual(ProjectileSpriteNative.Projectile(0xedb4), ProjectileSpriteNative.Projectile(0xede0),
            "charged Ice's two stored stars are one composition");

        // Power's rightward record, which nothing copies, rewritten as its leftward one: two compositions would store the same
        // bytes.
        byte[] doubled = rom.Rom.ToArray();
        int left = CartridgeImportAddressSpace.ToRomOffset(0x930000 | ProjectileSpriteNative.Address(new(ProjectileSpriteGroup.Power, "Left")));
        int right = CartridgeImportAddressSpace.ToRomOffset(0x930000 | ProjectileSpriteNative.Address(new(ProjectileSpriteGroup.Power, "Right")));
        doubled.AsSpan(left, 7).CopyTo(doubled.AsSpan(right, 7));
        InvalidDataException twice = AssertThrows<InvalidDataException>(() => ProjectileSpriteExtractor.Extract(new CartridgeImportAddressSpace(doubled)),
            "a composition stored twice under two names is refused before it is written twice");
        AssertTrue(twice.Message.Contains("Power.Left", StringComparison.Ordinal) && twice.Message.Contains("Power.Right", StringComparison.Ordinal),
            $"the refusal names both compositions: {twice.Message}");

        // The Wave beam's centre, held as the leftward Power beam's character, flipped: it no longer stores that composition.
        byte[] diverged = rom.Rom.ToArray();
        diverged[CartridgeImportAddressSpace.ToRomOffset(0x93ae65) + 6] ^= 0x40;
        InvalidDataException copy = AssertThrows<InvalidDataException>(() => ProjectileSpriteExtractor.Extract(new CartridgeImportAddressSpace(diverged)),
            "a record held as another composition's copy must store it byte for byte");
        AssertTrue(copy.Message.Contains("$93:AE65", StringComparison.Ordinal) && copy.Message.Contains("Power.Left", StringComparison.Ordinal),
            $"the refusal names the record and the composition it is held as: {copy.Message}");
    }

    /// <summary>Import-only bank-$93 record decoder for the projectile OAM oracle.</summary>
    internal static void DrawImportedProjectileSpritemap(ISnesAddressSpace source,
        OamBuffer oam, ushort pointer, Position16<ScreenPixels> origin)
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

    /// <summary>Checks shot, explosion, and bomb draw owners preserve OAM and projectile state without ROM reads.</summary>
    private static void VerifyProjectileCompositionOwners()
    {
        int comparisons = 0;
        // Each projectile sheet holds only the compositions its beam combination's projectiles show (#1513): a composition
        // draws from the sheet of a combination showing it, and one no combination shows is on no gameplay sheet and drawn
        // by no projectile.
        ProjectileDrawArtwork artwork = InstalledFixtures.ProjectileArtwork();
        foreach (ushort id in ProjectileSpriteDefinitions.NativePointers.Where(id => CombinationShowing(id) is not null))
        foreach (ushort coordinate in new ushort[] { 0, 100, 223, 255, 256, 303, 304, 0xffcf, 0xffd0 })
        foreach (ushort type in new ushort[] { 0, 0x10, 0x100, 0x200, 0x300, 0x500, 0x700, 0x800 })
        {
            SamusBeamCombination pictured = CombinationShowing(id) ?? throw new InvalidOperationException($"No beam combination's sheet holds composition ${id:X4}.");
            var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
            var bombs = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
            var shot = shots.Shots[0];
            shot.HeldShot().Animation = ProjectileFixturePlaybacks.Active(); shot.SpritemapPointer = id;
            shot.Occupy(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(type), 123); shot.HeldShot().PicturedBeams = pictured; shot.HeldShot().Position = shot.Position.WithWhole(new(coordinate, coordinate));
            shot.HeldShot().Radius = shot.Radius.WithX(7).WithY(9);
            var bomb = bombs.Bombs[0];
            bomb.HeldBomb().Animation = ProjectileFixturePlaybacks.Active(); bomb.SpritemapPointer = id;
            bomb.OccupyAsPlaced(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(type)); bomb.HeldBomb().PicturedBeams = pictured; bomb.HeldBomb().Position = bomb.Position.WithWhole(new(coordinate, coordinate));
            bomb.HeldBomb().Fuse = UpdateCountdown.For(1);
            {
                var native = new SpriteFrameBuffer(); var extracted = new SpriteFrameBuffer();
                native.BeginFrame(); extracted.BeginFrame();
                shots.DrawLiveProjectiles(native, Position16<RoomPixels>.Zero, artwork);
                shots.DrawExplosions(native, Position16<RoomPixels>.Zero, artwork);
                bombs.Draw(native, Position16<RoomPixels>.Zero, artwork);
                shots.DrawLiveProjectiles(extracted, Position16<RoomPixels>.Zero, artwork);
                shots.DrawExplosions(extracted, Position16<RoomPixels>.Zero, artwork);
                bombs.Draw(extracted, Position16<RoomPixels>.Zero, artwork);
                AssertTrue(native.FinalizeAsOam().ImageEquals(extracted.FinalizeAsOam()),
                    "Production projectile composition preserves admission and OAM");
                AssertEqual(native.Draws.Length, extracted.Draws.Length, "Composition owner publishes the same draws");
                AssertEqual(123, shot.HeldShot().Damage, "Composition does not mutate damage");
                AssertEqual((ushort)7, shot.Radius.X, "Composition does not mutate X radius");
                AssertEqual((ushort)9, shot.Radius.Y, "Composition does not mutate Y radius");
                comparisons++;
            }
            if (type == 0x300)
            {
                bomb.HeldBomb().Fuse = UpdateCountdown.For(0);
                var hidden = new SpriteFrameBuffer();
                hidden.BeginFrame();
                bombs.Draw(hidden, Position16<RoomPixels>.Zero, artwork);
                hidden.FinalizeFrame();
                AssertEqual(0, hidden.Draws.Length, "Detonated power bomb remains hidden with extracted art");
            }
        }
        Console.WriteLine($"Projectile composition owners: {comparisons} stock OAM comparisons with every ROM access forbidden.");

        // The first beam combination whose installed sheet holds the composition at bank-$93 address pointer, pictured or
        // as showing nothing, which the drawn shot and bomb are pictured with; none when no combination's sheet holds it.
        static SamusBeamCombination? CombinationShowing(ushort pointer)
        {
            string frame = ProjectileSheetFrames.ProjectileFrame(ProjectileSpriteNative.Projectile(pointer));
            foreach ((SamusBeamCombination combination, SpriteSheetId sheetId) in ProjectileSheetDefinitions.Sheets)
            {
                SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(sheetId);
                if (sheet.frames.ContainsKey(frame) || sheet.blanks.Contains(frame))
                    return combination;
            }
            return null;
        }
    }

    /// <summary>Rejects every cartridge read and bus write during installed composition drawing.</summary>
    internal static CartridgeReadGuard NewProjectileCompositionForbiddenBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Composition draw read ROM {address:X6}.")
            .RejectWrites(_ => "Composition draw mutated the bus.");

    /// <summary>
    /// Checks the three projectile actor passes draw the composition's frame from the bound projectile sheet, that a
    /// rebound sheet set reaches them, and that saved graphs exclude the sheets and draw alike once rebound.
    /// </summary>
    private static void VerifyRuntimeProjectileSheets(ushort sprite, InstalledSpriteSheets sheets,
        MapPresentationInstalledRoomAssets roomAssets, InstalledScreenContent maps)
    {
        var random = new SharedRandom();
        var game = new global::SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), null, SuperMetroidGameHostServices.Default, random);
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(SuperMetroid.Core.Gameplay.GameplayAssists.None, InstalledFixtures.GameArtwork.Gameplay, random: random, audio: game.AudioForVerification);
        runtime.Artwork.ScreenContent = maps;
        roomAssets.Bind(runtime);
        runtime.Artwork.SpriteSheets = sheets;
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        game.AdoptRuntime(runtime);
        game.BindArtwork(InstalledFixtures.GameArtwork with { SpriteSheets = sheets });
        string frame = ProjectileSheetFrames.ProjectileFrame(ProjectileSpriteNative.Projectile(sprite));
        // A sheet set loaded apart from the bound one, which each pass must reach once rebound.
        InstalledSpriteSheets rebound = InstalledFixtures.SpriteSheetsReloading(SpriteSheetId.ProjectilesPower);
        // Seed only a visible timed composition. The shared production actor draw, not
        // a duplicate test emitter, must bind all three owner passes to host content.
        foreach (ushort family in new ushort[] { 0x10, 0x700, 0x500 })
        {
            runtime.Projectiles.Reset(); runtime.BombProjectiles.Reset();
            var kind = family == 0x500 ? SpriteSourceKind.SamusBomb : SpriteSourceKind.SamusProjectile;
            if (family == 0x500)
            {
                var slot = runtime.BombProjectiles.Bombs[0];
                slot.OccupyAsPlaced(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(family)); slot.HeldBomb().Animation = ProjectileFixturePlaybacks.Active(); slot.SpritemapPointer = sprite;
                slot.HeldBomb().Position = slot.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
            }
            else
            {
                var slot = runtime.Projectiles.Shots[0];
                slot.OccupyAsFired(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(family)); slot.HeldShot().Animation = ProjectileFixturePlaybacks.Active(); slot.SpritemapPointer = sprite;
                slot.HeldShot().Position = slot.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
            }
            SpriteSheet power = sheets.Get(SpriteSheetId.ProjectilesPower);
            AssertEqual(power.Frame(frame), Shown(runtime, kind).Image, $"Family ${family:X} draws its composition's frame from the bound power-beam sheet");
            game.BindArtwork(InstalledFixtures.GameArtwork with { SpriteSheets = rebound });
            AssertTrue(ReferenceEquals(rebound.Get(SpriteSheetId.ProjectilesPower).Image, Shown(runtime, kind).Image.Sheet),
                $"Family ${family:X} draws from rebound sheets");
            game.BindArtwork(InstalledFixtures.GameArtwork with { SpriteSheets = sheets });
        }
        Console.WriteLine("Projectile runtime binding: three actor draw passes, sheet frames and sheet rebinding pass.");

        static SamusColouredDraw Shown(SuperMetroid.Core.Runtime.SuperMetroidRuntime target, SpriteSourceKind kind)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(target.Room.LoadedScope, target.Player.IntroducedSamus, SamusDrawOwner.Gameplay, SamusColourEffects.Hold, deathStep: null);
            target.Sprites.FinalizeFrame();
            return target.Sprites.Draws.OfType<SamusColouredDraw>().Single(draw => draw.Source.Kind == kind);
        }
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
