using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
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
using SuperMetroid.Core.World;

using SuperMetroid.AssetExtraction.Areas.Ceres;
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
        EnemyTileSourceArtwork artwork = InstalledFixtures.EnemyTiles;
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        SpawnedObjectFixtures.LoadSetDrawing(enemies, EnemyDefinitionId.CorpseTorizo);
        SuperMetroid.Core.Enemies.Corpses.DeadTorizoBehavior.InitializeDeadTorizo(enemies.SlotTable.Slots[0]);
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        enemies.DeadTorizoBehavior.DrawDeadTorizoSandHeap(sprites, DeadTorizoCamera);
        sprites.FinalizeFrame();
        // The hook draws one picture per layer of its frame, each for what the rotting rows it reads show.
        SuperMetroid.Core.Enemies.Artwork.SpawnedObjectSheet hookPictures =InstalledFixtures.EnemySheets.Spawned(SpawnedObjectFixtures.SetDrawing(EnemyDefinitionId.CorpseTorizo), SpawnedObjectOwner.Shared);
        AssertTrue(sprites.Draws.Length > 0, "Dead Torizo's hook draws");
        foreach (SpriteDraw corpse in sprites.Draws)
        {
            AssertEqual(new SpriteSource(SpriteSourceKind.DeadTorizoSandHeap), corpse.Source, "Dead Torizo's hook names its source");
            AssertTrue(corpse is ImageSpriteDraw hook && hookPictures.Sheets.Any(sheet => sheet.Image == hook.Image.Sheet),
                "Dead Torizo's hook draws from its set's character-zero pictures");
            AssertEqual(new Position16<ScreenPixels>(unchecked((ushort)(296 - DeadTorizoCamera.X)), unchecked((ushort)(187 - DeadTorizoCamera.Y))), corpse.Origin,
                "Dead Torizo's hook frame sits at its fixed room point");
        }

        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        var uploads = new SpriteArtworkPlacements();
        var queue = new VramWriteQueue();
        var owned = new InstalledArtworkUploadSource(InstalledArtworkId.StandardObjectTiles);
        var content = RuntimeArtworkContent.Of(runtime);
        // An owned source's transfer copies the whole source, so the upload is the installed characters entire; the staged
        // upload after it lands over its bytes from 0x800 on.
        ReadOnlyMemory<byte> installed = owned.Resolve(content);
        ReadOnlyMemory<byte> picture = installed[..0x800];
        queue.EnqueueSource(owned, checked((ushort)installed.Length), VramDmaDestinationConstants.At(GameplayVramLayoutConstants.ObjectCharacters));
        var staging = new VramStagingBuffer(64);
        staging.ByteView(0, 64)[3] = 0x5a;
        queue.EnqueueStaging(64, 0x7e9000, staging, 0, VramDmaDestinationConstants.At(GameplayVramLayoutConstants.ObjectCharacters, 0x400));
        queue.DrainTo(new SnesVram(), content, uploads, BgArtworkFixtures.For(new SnesVram()));
        staging.ByteView(0, 64)[3] = 0;
        AssertEqual(2, uploads.Uploads.Count, "Both uploads reach the OBJ page");
        AssertEqual<VerificationArtworkSource>(owned, uploads.Uploads[0].Source, "The installed upload is bound by its identity");
        AssertTrue(uploads.Uploads[1].Source is StagedObjCharacters, "A staged corpse upload keeps the bytes it read");
        SpriteArtwork page = new ObjCharacterTablePage().Page(uploads, content);
        AssertTrue(page.Characters.Slice(0, picture.Length).SequenceEqual(picture.Span), "The page shows the installed upload");
        AssertEqual((byte)0x5a, page.Characters[0x800 + 3], "The page shows the staged bytes as uploaded, not as later changed");

        Console.WriteLine("Boss sprite entries stage 1: Dead Torizo's hook frame publishes over the OBJ page, and installed and staged corpse uploads bind as owned sources.");
    }

    /// <summary>Stage 2: Draygon's look once her opening readies the Evirs copies Evir's installed tiles (#1538).</summary>
    internal static void VerifyBossSpriteEntriesStage2()
    {
        ImportEnemyArtwork artwork = InstalledFixtures.EnemyArtwork;
        var resolver = new EnemyTileTransfers(artwork);
        ReadOnlyMemory<byte> evirTiles = artwork.Enemies.TilesOf(EnemyDefinitionId.Evir);
        AssertEqual(SuperMetroid.AssetExtraction.Enemies.EnemyTileSetDefinitions.ByteCounts[EnemyDefinitionId.Evir], evirTiles.Length,
            "Evir's installed sheet is its whole tile set");
        EncounterArtworkUpload upload = EncounterLookUploads.Uploads(ComposedEncounter.DraygonEvirs,
            EncounterLookUploads.IndexOf(ComposedEncounter.DraygonEvirs, EncounterLookUploads.DraygonEvirsLook), resolver).Single();
        AssertTrue(upload.Characters.Span.SequenceEqual(evirTiles.Span), "Draygon's Evirs look copies Evir's installed sheet");
        Console.WriteLine("Boss sprite entries stage 2: Draygon's Evirs look copies Evir's installed tiles.");
    }

    /// <summary>
    /// Stage 3: Ridley's tail and wings publish named entries over the OBJ page in the cartridge's order, and Ceres
    /// escape character uploads name their installed page.
    /// </summary>
    internal static void VerifyBossSpriteEntriesStage3()
    {
        ImportEnemyArtwork artwork = InstalledFixtures.EnemyArtwork;
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artwork.Bosses };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.RidleyCeres, slot.Definition);
        slot.Position = slot.Position.WithWhole(new(200, 100));
        // Ceres Ridley's scene draws from his room's sheets.
        SpawnedObjectFixtures.LoadSetDrawing(enemies, EnemyDefinitionId.RidleyCeres);
        enemies.RidleyEncounter.Ridley = new SuperMetroid.Core.Bosses.Ridley.RidleyEnemyState
        {
            MovementAnimationEnabled = true,
            FacingDirection = SuperMetroid.Core.Bosses.Ridley.RidleyFacing.Left,
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
        // A frame drawn over uploaded characters publishes one draw per layer picture, all under its piece's source (#1406),
        // so the order is each piece once.
        SpriteSource[] published = [.. sprites.Draws.Select(draw => draw.Source)
            .Where((source, index) => index == 0 || sprites.Draws[index - 1].Source != source)];
        AssertTrue(published.SequenceEqual(expected),
            "Ridley publishes the tail tip, then segments five to the base, then the wings; published "
            + string.Join(", ", published.Select(source => $"{source.Kind} {source.Instance}")));
        AssertEqual(new Position16<ScreenPixels>(140, 138), sprites.Draws[0].Origin, "The tail tip sits at its segment's screen point");

        int characters = CeresWarningTextPageConstants.ForTransfer(CeresEscapeNativeSources.PageSource(0) + 512, 512)
            ?? throw new InvalidOperationException("The warning page holds the transfer.");
        AssertEqual(1, characters, "A warning-text transfer names the warning page it copies");
        AssertTrue(CeresWarningTextPageConstants.ForTransfer(CeresEscapeNativeSources.PageSource(0) + 64, 128) is null,
            "A transfer that copies no warning page whole names no page");
        AssertTrue(CeresWarningTextPageConstants.ForTransfer(0x7e9000, 64) is null, "A transfer outside both pages names no page");
        AssertTrue(CeresWarningTextPages.ByteOffset(characters) == 512 && CeresWarningTextPages.ByteCount(characters) == 512,
            "The warning page shows the installed warning characters the transfer copies");
        Console.WriteLine("Boss sprite entries stage 3: Ridley's tail and wings publish named entries in order; Ceres warning transfers name their pages.");
    }

    /// <summary>Runs the #1344 confirmations.</summary>
    [VerificationCommand("--boss-sprite-entries")]
    internal static int BossSpriteEntriesCommand()
    {
        Suite(nameof(VerifyBossSpriteEntriesStage1), VerifyBossSpriteEntriesStage1);
        Suite(nameof(VerifyBossSpriteEntriesStage2), VerifyBossSpriteEntriesStage2);
        Suite(nameof(VerifyBossSpriteEntriesStage3), VerifyBossSpriteEntriesStage3);
        return 0;
    }
}
