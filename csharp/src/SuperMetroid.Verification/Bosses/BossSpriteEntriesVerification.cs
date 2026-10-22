using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses;

using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Escape;

/// <summary>
/// Checks #1344: boss and scripted-actor sprite parts publish whole-frame entries over the gameplay OBJ page, and their
/// OBJ artwork reaches that page as named owned sources rather than VRAM writes or bus addresses.
/// </summary>
internal static class BossSpriteEntriesVerification
{
    /// <summary>A camera that leaves Dead Torizo's hook frame on screen.</summary>
    private static readonly Position16<RoomPixels> DeadTorizoCamera = new(200, 40);

    /// <summary>Stage 1: Dead Torizo's hook frame, the Tourian statue pictures and staged corpse uploads.</summary>
    internal static void VerifyBossSpriteEntriesStage1()
    {
        EnemyTileArtworkCatalog artwork = InstalledFixtures.EnemyTiles;
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork };
        SpawnedObjectFixtures.LoadSetDrawing(enemies, EnemyDefinitionId.CorpseTorizo);
        enemies.DeadTorizoBehavior.InitializeDeadTorizo(enemies.SlotTable.Slots[0]);
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        enemies.DeadTorizoBehavior.DrawDeadTorizoHook(sprites, DeadTorizoCamera);
        sprites.FinalizeFrame();
        // The hook draws one picture per layer of its frame, each for what the rotting rows it reads show.
        SpriteImage hookSheet = InstalledFixtures.EnemySheets.Spawned(SpawnedObjectFixtures.SetDrawing(EnemyDefinitionId.CorpseTorizo), 0).Sheet.Image;
        AssertTrue(sprites.Draws.Length > 0, "Dead Torizo's hook draws");
        foreach (SpriteDraw corpse in sprites.Draws)
        {
            AssertEqual(new SpriteSource(SpriteSourceKind.DeadTorizoHook), corpse.Source, "Dead Torizo's hook names its source");
            AssertTrue(corpse is ImageSpriteDraw hook && hook.Image.Sheet == hookSheet,
                "Dead Torizo's hook draws from its set's character-zero sheet");
            AssertEqual(new Position16<ScreenPixels>(unchecked((ushort)(296 - DeadTorizoCamera.X)), unchecked((ushort)(187 - DeadTorizoCamera.Y))), corpse.Origin,
                "Dead Torizo's hook frame sits at its fixed room point");
        }

        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        var uploads = new ObjCharacterUploads();
        var queue = new VramWriteQueue();
        var statue = new TourianStatuePictureCharacters(AnimatedTileObject.TourianStatuePhantoon, TourianStatueTilePicture.Released);
        ReadOnlyMemory<byte> picture = statue.Resolve(runtime.Artwork);
        queue.EnqueueSource(statue, checked((ushort)picture.Length), VramDmaDestination.At(GameplayVramLayout.ObjectCharacters));
        var staging = new VramStagingBuffer(64);
        staging.ByteView(0, 64)[3] = 0x5a;
        queue.EnqueueStaging(64, 0x7e9000, staging, 0, VramDmaDestination.At(GameplayVramLayout.ObjectCharacters, 0x400));
        queue.DrainTo(new SnesVram(), runtime.Artwork, uploads, BgArtworkFixtures.For(new SnesVram()));
        staging.ByteView(0, 64)[3] = 0;
        AssertEqual(2, uploads.Uploads.Count, "Both uploads reach the OBJ page");
        AssertEqual<ObjCharacterSource>(statue, uploads.Uploads[0].Source, "The statue picture is bound by its identity");
        AssertTrue(uploads.Uploads[1].Source is StagedObjCharacters, "A staged corpse upload keeps the bytes it read");
        SpriteArtwork page = new ObjCharacterTablePage().Page(uploads, runtime.Artwork);
        AssertTrue(page.Characters.Slice(0, picture.Length).SequenceEqual(picture.Span), "The page shows the statue picture");
        AssertEqual((byte)0x5a, page.Characters[0x800 + 3], "The page shows the staged bytes as uploaded, not as later changed");

        Console.WriteLine("Boss sprite entries stage 1: Dead Torizo's hook frame publishes over the OBJ page, and statue pictures and staged corpse uploads bind as owned sources.");
    }

    /// <summary>Stage 2: Draygon's opening Evir sheet and Torizo's instruction-time copies name their installed tiles.</summary>
    internal static void VerifyBossSpriteEntriesStage2()
    {
        InstalledEnemyArtwork artwork = InstalledFixtures.EnemyArtwork;
        var resolver = new EnemyTileTransfers(artwork);
        RoomEnemyDefinition evir = RoomEnemyDefinitionCatalog.Get(EnemyDefinitionId.Evir);
        AssertTrue(artwork.Enemies.TryResolve(evir.TileDataAddress, evir.TileDataSize & short.MaxValue, out ReadOnlyMemory<byte> evirTiles) &&
            new EnemyTileCharacters(EnemyDefinitionId.Evir).Resolve(resolver).Span.SequenceEqual(evirTiles.Span),
            "Evir's tiles resolve to its installed sheet");
        var uploads = new ObjCharacterUploads();
        uploads.RecordUpload(BossVram.DraygonEvirTilesDma, evirTiles.Length, new EnemyTileCharacters(EnemyDefinitionId.Evir));
        SpriteArtwork page = new ObjCharacterTablePage().Page(uploads, resolver);
        int evirByte = (BossVram.DraygonEvirTilesDma.Word - GameplayVramLayout.ObjectCharacters.Offset) * 2;
        AssertTrue(page.Characters.Slice(evirByte, evirTiles.Length).SequenceEqual(evirTiles.Span),
            "The page shows Evir's sheet where Draygon's opening upload lands");
        Console.WriteLine("Boss sprite entries stage 2: Draygon's Evir sheet binds Evir's installed tiles.");
    }

    /// <summary>
    /// Stage 3: Ridley's tail and wings publish named entries over the OBJ page in the cartridge's order, and Ceres
    /// escape character uploads name their installed page.
    /// </summary>
    internal static void VerifyBossSpriteEntriesStage3()
    {
        InstalledEnemyArtwork artwork = InstalledFixtures.EnemyArtwork;
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.RidleyCeres, slot.Definition);
        slot.Position = slot.Position.WithWhole(new(200, 100));
        // Ceres Ridley's scene draws from his room's sheets.
        SpawnedObjectFixtures.LoadSetDrawing(enemies, EnemyDefinitionId.RidleyCeres);
        enemies.RidleyEncounter.Ridley = new SuperMetroid.Core.Bosses.Ridley.RidleyEnemyState
        {
            MovementAnimationEnabled = true,
            FacingDirection = SuperMetroid.Core.Bosses.Ridley.RidleyFacing.Left,
            SpritemapPaletteIndex = SnesObjPalettes.Palette(1),
            TailSegments = [.. Enumerable.Range(0, 7).Select(index => new SuperMetroid.Core.Bosses.Ridley.RidleyTailSegment
            {
                Position = new Position16<RoomPixels>(unchecked((ushort)(80 + index * 10)), unchecked((ushort)(120 + index * 3))),
            })],
        };
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        enemies.CeresRidleyBehavior.BabyScene.DrawRidleySupplementalSprites(sprites, slot, Position16<RoomPixels>.Zero);
        sprites.FinalizeFrame();
        SpriteSource[] expected = [.. new[] { 6, 5, 4, 3, 2, 1, 0 }.Select(segment => new SpriteSource(SpriteSourceKind.RidleyTail, segment)),
            new SpriteSource(SpriteSourceKind.RidleyWings)];
        AssertTrue(sprites.Draws.Select(draw => draw.Source).SequenceEqual(expected),
            "Ridley publishes the tail tip, then segments five to the base, then the wings");
        AssertEqual(new Position16<ScreenPixels>(140, 138), sprites.Draws[0].Origin, "The tail tip sits at its segment's screen point");

        TileSheetDefinition warning = CeresEscapeTileArtworkDefinitions.WarningText;
        CeresEscapeTileCharacters characters = CeresEscapeTileCharacters.ForTransfer(warning.SourceAddress + 64, 128)
            ?? throw new InvalidOperationException("The warning page holds the transfer.");
        AssertEqual(new CeresEscapeTileCharacters(0, 64, 128), characters, "A warning-text transfer names its page range");
        AssertTrue(CeresEscapeTileCharacters.ForTransfer(0x7e9000, 64) is null, "A transfer outside both pages names no page");
        AssertTrue((artwork.Areas.CeresEscapeTiles ?? throw new InvalidOperationException("The installation has no Ceres escape pages."))
                .TryResolve(warning.SourceAddress + 64, 128, out ReadOnlyMemory<byte> installed) &&
            characters.Resolve(new EnemyTileTransfers(artwork)).Span.SequenceEqual(installed.Span),
            "The page range resolves to the installed warning characters");
        Console.WriteLine("Boss sprite entries stage 3: Ridley's tail and wings publish named entries in order; Ceres escape pages bind by range.");
    }

    /// <summary>Stage 4: Mother Brain's tile transfers show installed pages, and her WRAM corpse buffer is never transferred.</summary>
    internal static void VerifyBossSpriteEntriesStage4()
    {
        InstalledEnemyArtwork artwork = InstalledFixtures.EnemyArtwork;
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        ObjCharacterUploads uploads = enemies.Room.Session.ObjCharacters;
        int corpseBytes = MotherBrainCorpseArtworkDefinitions.VramPageByteCount;
        enemies.MotherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(new MotherBrainSpriteTileTransferRequest(0, corpseBytes,
            MotherBrainCorpseArtworkDefinitions.VramPageSource(1), MotherBrainCorpseArtworkDefinitions.VramPageDestination(1)));
        AssertEqual<ObjCharacterSource>(new MotherBrainCorpseCharacters(1), uploads.Uploads.Single().Source,
            "A corpse page transfer binds its installed page");
        AssertEqual((MotherBrainCorpseArtworkDefinitions.VramPageDestination(1) - GameplayVramLayout.ObjectCharacters.Offset) * 2,
            uploads.Uploads.Single().FirstByte, "The corpse page lands at its native OBJ position");
        var resolver = new EnemyTileTransfers(artwork);
        AssertTrue((artwork.Bosses.MotherBrainCorpse ?? throw new InvalidOperationException("The installation has no Mother Brain corpse tiles."))
                .Transfer.Span.Slice(checked((int)MotherBrainCorpseArtworkDefinitions.VramPageSource(1)) - MotherBrainCorpseArtworkDefinitions.SourceAddress, corpseBytes)
                .SequenceEqual(new MotherBrainCorpseCharacters(1).Resolve(resolver).Span),
            "The corpse page resolves to the installed corpse tiles");

        // The rotting corpse's rows show the extraction's rot pictures by step (#1392).
        AssertThrows<InvalidDataException>(() => enemies.MotherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(new MotherBrainSpriteTileTransferRequest(0, 64,
            MotherBrainCorpseArtworkDefinitions.StagingAddress, MotherBrainCorpseArtworkDefinitions.VramPageDestination(2))),
            "The WRAM corpse buffer is never transferred as characters");
        Console.WriteLine("Boss sprite entries stage 4: Mother Brain's transfers bind installed pages and refuse her corpse buffer.");
    }

    /// <summary>Runs the #1344 confirmations.</summary>
    [VerificationCommand("--boss-sprite-entries")]
    internal static int BossSpriteEntriesCommand()
    {
        Suite(nameof(VerifyBossSpriteEntriesStage1), VerifyBossSpriteEntriesStage1);
        Suite(nameof(VerifyBossSpriteEntriesStage2), VerifyBossSpriteEntriesStage2);
        Suite(nameof(VerifyBossSpriteEntriesStage3), VerifyBossSpriteEntriesStage3);
        Suite(nameof(VerifyBossSpriteEntriesStage4), VerifyBossSpriteEntriesStage4);
        return 0;
    }
}
