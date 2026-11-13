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
using SuperMetroid.Core.Frontend.Map.Artwork;
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
    internal static void VerifyProjectileCompositions(SuperMetroidAddressSpace rom,
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
                native.AddProjectileSpritePart(new(0), 0, new(0), new Position16<SpritePixels>(0, 0));
                authored.AddProjectileSpritePart(new(0), 0, new(0), new Position16<SpritePixels>(0, 0));
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
        Console.WriteLine($"Projectile compositions: 417 extracted sprites/{draws} native OAM comparisons and invalid-field rejection pass without a draw-time ROM.");
    }

    /// <summary>Import-only bank-$93 record decoder for the projectile OAM oracle.</summary>
    internal static void DrawImportedProjectileSpritemap(ISnesAddressSpace source,
        OamBuffer oam, ushort pointer, Position16<SpritePixels> origin)
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
            var shots = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
            var bombs = new SamusBombProjectileSystem();
            var shot = shots.Slots[0];
            shot.Playback = ProjectileFixturePlaybacks.Active; shot.SpritemapPointer = id;
            shot.Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(type); shot.PicturedBeams = pictured; shot.Position = shot.Position.WithWhole(new(coordinate, coordinate));
            shot.Damage = 123; shot.Radius = shot.Radius.WithX(7).WithY(9);
            var bomb = bombs.Slots[0];
            bomb.Playback = ProjectileFixturePlaybacks.Active; bomb.SpritemapPointer = id;
            bomb.Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(type); bomb.PicturedBeams = pictured; bomb.Position = bomb.Position.WithWhole(new(coordinate, coordinate));
            bomb.BombTimer = WrappingCountdown.Start(1);
            foreach (ushort parity in new ushort[] { 0, 1, 2, 3 })
            {
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
                AssertEqual(123, shot.Damage, "Composition does not mutate damage");
                AssertEqual((ushort)7, shot.Radius.X, "Composition does not mutate X radius");
                AssertEqual((ushort)9, shot.Radius.Y, "Composition does not mutate Y radius");
                comparisons++;
            }
            if (type == 0x300)
            {
                bomb.BombTimer = WrappingCountdown.Start(0);
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
        MapPresentationInstalledRoomAssets roomAssets, AreaMapPresentation maps)
    {
        var random = new SharedRandom();
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(SuperMetroid.Core.Gameplay.GameplayAssists.None, InstalledFixtures.BareRuntimeArtwork, random: random);
        runtime.Artwork.MapPresentation = maps;
        roomAssets.Bind(runtime);
        runtime.Artwork.SpriteSheets = sheets;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), null, SuperMetroid.Core.Frontend.SuperMetroidGameHostServices.Default, random);
        game.AdoptRuntime(runtime);
        game.BindArtwork(InstalledFixtures.BareRuntimeArtwork with { SpriteSheets = sheets });
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
                var slot = runtime.BombProjectiles.Slots[0];
                slot.Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(family); slot.Playback = ProjectileFixturePlaybacks.Active; slot.SpritemapPointer = sprite;
                slot.Position = slot.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
            }
            else
            {
                var slot = runtime.Projectiles.Slots[0];
                slot.Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(family); slot.Playback = ProjectileFixturePlaybacks.Active; slot.SpritemapPointer = sprite;
                slot.Position = slot.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
            }
            SpriteSheet power = sheets.Get(SpriteSheetId.ProjectilesPower);
            AssertEqual(power.Frame(frame), Shown(runtime, kind).Image, $"Family ${family:X} draws its composition's frame from the bound power-beam sheet");
            game.BindArtwork(InstalledFixtures.BareRuntimeArtwork with { SpriteSheets = rebound });
            AssertTrue(ReferenceEquals(rebound.Get(SpriteSheetId.ProjectilesPower).Image, Shown(runtime, kind).Image.Sheet),
                $"Family ${family:X} draws from rebound sheets");
            game.BindArtwork(InstalledFixtures.BareRuntimeArtwork with { SpriteSheets = sheets });
        }
        Console.WriteLine("Projectile runtime binding: three actor draw passes, sheet frames and sheet rebinding pass.");

        static ImageSpriteDraw Shown(SuperMetroid.Core.Runtime.SuperMetroidRuntime target, SpriteSourceKind kind)
        {
            target.Sprites.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(target.Room.LoadedScope, SamusDrawOwner.Gameplay, SamusPaletteEffects.Hold);
            target.Sprites.FinalizeFrame();
            return target.Sprites.Draws.OfType<ImageSpriteDraw>().Single(draw => draw.Source.Kind == kind);
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
