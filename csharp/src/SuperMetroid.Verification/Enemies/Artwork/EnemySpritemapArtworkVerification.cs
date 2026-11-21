using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Input;
using SuperMetroid.Verification.Enemies.Puyo;
using SuperMetroid.Verification.Enemies.Choot;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Enemies.Alcoon.AlcoonInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Beetom.BeetomInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonVisualsVerification;
using static SuperMetroid.Verification.Enemies.Evir.EvirVisualsVerification;
using static SuperMetroid.Verification.Enemies.Crawlers.OrangeZoomerArtworkVerification;
using static SuperMetroid.Verification.Enemies.Hoppers.HopperAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Enemies.Metroids.MochtroidVisualsVerification;
using static SuperMetroid.Verification.Enemies.Crawlers.SharedCrawlerArtworkVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.WorkRobots.WorkRobotVisualsVerification;
using static SuperMetroid.Verification.Enemies.Yard.YardVisualsVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed enemy OAM compositions against pinned cartridge frames and drawing paths.</summary>
internal static class EnemySpritemapArtworkVerification
{
    /// <summary>Runs the Alcoon in a real room with the installed artwork.</summary>
    private static void VerifyInstalledAlcoonInstructionFrames(
        CartridgeImportAddressSpace rom, ImportEnemyArtwork stock)
    {
        Suite(nameof(VerifyAlcoonInstructionProgramDefinitions), () => VerifyAlcoonInstructionProgramDefinitions(rom, stock));
    }

    /// <summary>Runs the Beetom programs with the installed artwork.</summary>
    private static void VerifyInstalledBeetomInstructionFrames(
        CartridgeImportAddressSpace rom, ImportEnemyArtwork stock)
    {
        Suite(nameof(VerifyBeetomInstructionProgramDefinitions), () => VerifyBeetomInstructionProgramDefinitions(rom, stock));
    }

    /// <summary>Runs the hopper family's typed animations with the installed stock artwork.</summary>
    private static void VerifyInstalledHopperInstructionFrames(
        CartridgeImportAddressSpace rom, ImportEnemyArtwork stock)
    {
        Suite(nameof(VerifyHopperAnimationDefinitions), () => VerifyHopperAnimationDefinitions(rom, stock));
    }

    /// <summary>Runs Choot's idle, jumping and falling animations with the installed stock artwork.</summary>
    private static void VerifyInstalledChootInstructionFrames(
        ImportEnemyArtwork stock)
    {
        Suite(nameof(ChootBehaviorVerification.VerifyChootBehavior), () => ChootBehaviorVerification.VerifyChootBehavior(stock));
    }

    /// <summary>Checks Bull frame selectors and exercises normal and immune-shot programs without cartridge visual reads.</summary>
    private static void VerifyInstalledBullInstructionFrames(
        ImportEnemyArtwork stock)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: stock.Bosses);
        RoomEnemyPlace bull = enemies.Places.InUpdateOrder[0];
        bull.Bind(EnemyDefinitionId.Bull, bull.KeptDefinition);
        bull.Bind(bull.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        bull.SetSpawnWords(EnemyDefinitionId.Bull, words => words);
        new Action<RoomEnemyPlace>(fixturePlace => enemies.BullBehavior.InitializeBull(fixturePlace))(bull);
        object?[] arguments =
            [bull, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
        for (int frame = 0; frame < 41; frame++)
            EnemyInstructionRunner.Process(arguments);
        AssertEqual(BullAnimationsConstants.Spritemaps(BullSegment.Idle).First(), bull.SpritemapPointer,
            "installed Bull normal animation loops back to its first frame on time");
        BullEnemyState state = enemies.BullStates[0] ?? throw new InvalidDataException(
            "Bull initializer did not publish typed state.");
        BullBehavior.ResolveBullImmuneShot(bull, state, BullMovementDefinitions.ShotAngles.For(SamusProjectileDirection.Right));
        for (int frame = 0; frame < 61; frame++)
            EnemyInstructionRunner.Process(arguments);
        AssertEqual(BullAnimationsConstants.Spritemaps(BullSegment.Idle).First(), bull.SpritemapPointer,
            "installed Bull immune-shot program returns to the normal loop");
    }

    /// <summary>Runs Puyo's grounded and airborne programs with the installed stock artwork.</summary>
    private static void VerifyInstalledPuyoInstructionFrames(
        ImportEnemyArtwork stock) =>
        Suite(nameof(PuyoInstructionProgramDefinitionsVerification.VerifyPuyoInstructionProgramDefinitions),
            () => PuyoInstructionProgramDefinitionsVerification.VerifyPuyoInstructionProgramDefinitions(stock));

    /// <summary>Compares installed enemy compositions and rendering with the cartridge's OAM definitions.</summary>
    internal static void VerifyInstalledEnemySpritemaps(
        CartridgeImportAddressSpace rom,
        ImportEnemyArtwork stock)
    {
        AssertTrue(stock.Enemies.Spritemaps is not null,
            "installed enemy catalog contains named OAM compositions");
        HashSet<ushort> installedHunterPointers = EnemySpritemapDefinitions.Frames
            
            .Where(frame => frame.LegacyName.StartsWith("ki_hunter_a8_", StringComparison.Ordinal))
            .Select(frame => frame.Pointer)
            .ToHashSet();
        AssertTrue(KiHunterAnimationsConstants.Spritemaps(Enum.GetValues<KiHunterSegment>()).ToHashSet().SetEquals(
                installedHunterPointers),
            "the Ki-Hunter's programs show exactly its installed frames");
        AssertEqual(35, installedHunterPointers.Count, "KiHunter's programs show 35 distinct OAM frames");
        HashSet<ushort> installedMawPointers = EnemySpritemapDefinitions.Frames
            
            .Where(frame => frame.LegacyName.StartsWith("yapping_maw_a8_", StringComparison.Ordinal))
            .Select(frame => frame.Pointer)
            .ToHashSet();
        AssertTrue(YappingMawAnimationsConstants.Spritemaps(Enum.GetValues<YappingMawSegment>()).ToHashSet().SetEquals(installedMawPointers),
            "every installed Yapping Maw frame is shown by a program, and every shown pose is installed");
        AssertEqual(24, installedMawPointers.Count, "Yapping Maw's programs show 24 distinct frames");
        // The hit burst's four frames are Samus's weapon effect, installed with her projectile pictures (#1713).
        // Their record that draws nothing shows the empty frame every enemy family shares, which installs no picture (#2370).
        AssertEqual(122, EnemySpritemapDefinitions.Frames.Count(
                frame => frame.Bank == EnemySpritemapDefinitions.RoomEffectActorBank),
            "the bank-B4 sprite-object programs the game starts select 122 distinct enemy-sheet visual frames");
        HashSet<ushort> installedSpritePointers = EnemySpritemapDefinitions.Frames
            
            .Where(frame => frame.Bank == EnemySpritemapDefinitions.RoomEffectActorBank)
            .Select(frame => frame.Pointer)
            .ToHashSet();
        AssertTrue(RoomEffectActorVisualDefinitionsConstants.Pictures.Select(RoomEffectActorVisualDefinitionsConstants.Spritemap).ToHashSet()
                .SetEquals(SuperMetroid.Verification.Rooms.EffectActors.RoomEffectActorNativeLists.SpritemapPointers(rom)),
            "the room sprite-object pictures are exactly the spritemaps the native lists show");
        foreach (RoomEffectActorPicture picture in RoomEffectActorVisualDefinitionsConstants.Pictures)
        {
            bool installed = picture.EnemyFrame is { } enemyFrame && !CommonEnemyEmptyFrames.DrawsNothing(enemyFrame);
            AssertEqual(installed, installedSpritePointers.Contains(RoomEffectActorVisualDefinitionsConstants.Spritemap(picture)),
                $"room sprite-object picture {picture} has installed enemy art unless it is Samus's hit burst or the empty frame");
        }
        AssertTrue(FuneNamiheAnimationsConstants.Spritemaps(Enum.GetValues<FuneNamiheSegment>()).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.Name.StartsWith("fune_", StringComparison.Ordinal) ||
                    frame.Name.StartsWith("namihe_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Fune's and Namihe's programs show exactly their installed frames");
        AssertTrue(RioAnimationsConstants.Spritemaps(Enum.GetValues<RioSegment>()).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.LegacyName.StartsWith("rio_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Rio's programs show exactly its eight installed frames");
        AssertTrue(HoltzAnimationsConstants.Spritemaps(Enum.GetValues<HoltzSegment>()).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.LegacyName.StartsWith("lower_norfair_rio_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Lower Norfair Rio's programs show exactly its installed frames");
        AssertTrue(GerutaAnimationsConstants.Spritemaps(Enum.GetValues<GerutaSegment>()).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.LegacyName.StartsWith("norfair_rio_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Norfair Rio's programs show exactly its installed frames");
        Suite(nameof(VerifyInstalledPuyoInstructionFrames), () => VerifyInstalledPuyoInstructionFrames(stock));
        Suite(nameof(VerifyInstalledBullInstructionFrames), () => VerifyInstalledBullInstructionFrames(stock));
        Suite(nameof(VerifyInstalledAlcoonInstructionFrames), () => VerifyInstalledAlcoonInstructionFrames(rom, stock));
        Suite(nameof(VerifyInstalledMochtroidVisuals), () => VerifyInstalledMochtroidVisuals(rom, stock.Enemies));
        Suite(nameof(VerifyInstalledEvirVisuals), () => VerifyInstalledEvirVisuals(rom, stock));
        Suite(nameof(VerifyInstalledWorkRobotVisuals), () => VerifyInstalledWorkRobotVisuals(rom, stock.Enemies));
        Suite(nameof(VerifyInstalledYardVisuals), () => VerifyInstalledYardVisuals(rom, stock.Enemies));
        Suite(nameof(VerifyInstalledBotwoonVisuals), () => VerifyInstalledBotwoonVisuals(rom, stock.Enemies));
        Suite(nameof(VerifyInstalledBeetomInstructionFrames), () => VerifyInstalledBeetomInstructionFrames(rom, stock));
        Suite(nameof(VerifyInstalledHopperInstructionFrames), () => VerifyInstalledHopperInstructionFrames(rom, stock));
        Suite(nameof(VerifyInstalledChootInstructionFrames), () => VerifyInstalledChootInstructionFrames(stock));
        Suite(nameof(VerifyInstalledOrangeZoomerInstructionFrames), () => VerifyInstalledOrangeZoomerInstructionFrames(stock));
        Suite(nameof(VerifyInstalledSharedCrawlerFrames), () => VerifyInstalledSharedCrawlerFrames(stock.Enemies));
        AssertTrue(MorphBallEyeVisualDefinitions.Frames().Select(frame => frame.Pointer).ToHashSet().SetEquals(
                MorphBallEyeAnimationsConstants.Spritemaps(Enum.GetValues<MorphBallEyeSegment>())
                    .Concat(MorphBallEyeAnimations.AimFrames.InTurnOrder.Select(SuperMetroid.Core.Enemies.MorphBallEye.Artwork.MorphBallEyeFrameToolingDefinitions.Spritemap))),
            "the installed Morph Ball eye frames are exactly the poses its programs and its aim show");
        HashSet<ushort> shaktoolFrames = ShaktoolVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(15, shaktoolFrames.Count,
            "Shaktool's visual frame identities are distinct");
        AssertTrue(ShaktoolAnimationsConstants.Spritemaps(Enum.GetValues<ShaktoolSegment>())
                .Concat(ShaktoolHeadPictureDefinitions.AimedPictures.Values.Select(SuperMetroid.Core.Enemies.Shaktool.Artwork.ShaktoolFrameToolingDefinitions.Spritemap))
                .ToHashSet().SetEquals(shaktoolFrames),
            "the Shaktool's programs and its head's aimed pictures (#2284) show exactly its installed frames");
        HashSet<ushort> platformFramePointers = TripperKamerVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(12, platformFramePointers.Count,
            "Tripper/Kamer visual frame identities are distinct");
        HashSet<ushort> shownPlatformFrames = PlatformAnimationsConstants.Spritemaps(Enum.GetValues<PlatformSegment>()).ToHashSet();
        AssertEqual(14, shownPlatformFrames.Count, "platform programs show fourteen distinct animated compositions");
        AssertTrue(platformFramePointers.Except(shownPlatformFrames).ToHashSet()
                .SetEquals(new[]
                {
                    TripperKamerVisualDefinitions.FrozenMovingLeft,
                    TripperKamerVisualDefinitions.FrozenMovingRight,
                }),
            "only Tripper's two direct shot-AI frozen compositions are not shown by a platform program");
        HashSet<ushort> dragonFrames = DragonVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(12, dragonFrames.Count,
            "Dragon has twelve distinct installed body and wing frames");
        AssertTrue(dragonFrames.SetEquals(DragonAnimationsConstants.Spritemaps(Enum.GetValues<DragonSegment>())),
            "every installed Dragon body and wing frame is a pose its programs show");
        HashSet<ushort> multiviolaFrames = MultiviolaVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(MultiviolaVisualDefinitions.FrameCount, multiviolaFrames.Count,
            "Multiviola's fourteen-frame loop selects eight distinct OAM frames");
        AssertTrue(MultiviolaAnimationsConstants.Spritemaps(MultiviolaSegment.Flying).ToHashSet().SetEquals(multiviolaFrames),
            "every installed Multiviola composition is shown by its loop");
        HashSet<ushort> lavaJumperFrames = NorfairLavaJumperVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(NorfairLavaJumperVisualDefinitions.FrameCount,
            lavaJumperFrames.Count,
            "Norfair lava jumper has eleven distinct installed identities");
        AssertTrue(lavaJumperFrames.Contains(CommonEnemyEmptyFrameToolingDefinitions.EmptySpritemap),
            "Norfair lava jumper retains its native bank-local empty frame");
        AssertTrue(NorfairLavaJumperAnimationsConstants.Spritemaps(Enum.GetValues<NorfairLavaJumperSegment>()).ToHashSet().SetEquals(lavaJumperFrames),
            "every installed lava-jumper frame is shown by a typed program");
        HashSet<ushort> chozoFrames = ChozoStatueVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(ChozoStatueVisualDefinitions.FrameCount, chozoFrames.Count,
            "the two Chozo sequences select twenty-four distinct installed pictures");
        AssertTrue(ChozoStatueAnimationsConstants.Spritemaps(Enum.GetValues<ChozoStatueSegment>()).ToHashSet().SetEquals(chozoFrames),
            "every installed Chozo composition is shown by a typed program");
        HashSet<ushort> violaFrames = ViolaVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(ViolaVisualDefinitions.FrameCount, violaFrames.Count,
            "Viola's fourteen-frame loop selects eight distinct OAM frames");
        AssertTrue(CrawlerAnimationsConstants.Spritemaps(CrawlerSegment.ViolaIdle).ToHashSet().SetEquals(violaFrames),
            "every installed Viola composition is shown by its normal loop");
        HashSet<ushort> rinkaFrames = RinkaVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(RinkaVisualDefinitions.FrameCount, rinkaFrames.Count,
            "both Rinka loops select five distinct OAM frames");
        AssertTrue(RinkaAnimationsConstants.Spritemaps(Enum.GetValues<RinkaSegment>()).ToHashSet().SetEquals(rinkaFrames),
            "every installed Rinka composition is shown by a Rinka program");
        var differing = new List<string>();
        foreach (EnemyFrameName frame in EnemySpritemapDefinitions.Frames)
        {
            AssertTrue((stock.Enemies.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGet(frame.Bank, frame.Pointer, out var parts),
                $"installed enemy frame {frame.Name} exists");
            AssertTrue(stock.Enemies.Spritemaps.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                $"installed enemy frame {frame.Name} has a display binding");
        foreach ((ushort x, ushort y, ushort palette, ushort baseTile) in
                     new EnemySpritemapArtworkVerifyInstalledEnemySpritemapsCase4[]
                     {
                         new EnemySpritemapArtworkVerifyInstalledEnemySpritemapsCase4(0x0010, 0x0020, 0, 0),
                         new EnemySpritemapArtworkVerifyInstalledEnemySpritemapsCase4(0x01f8, 0x00fc, 0x0c00, 0x01ff),
                         new EnemySpritemapArtworkVerifyInstalledEnemySpritemapsCase4(0x0000, 0x0000, 0x0400, 0x0010),
                     })
            {
                var native = new OamBuffer();
                var installed = new OamBuffer();
                DrawImportedEnemySpritemap(rom, native, new NativeEnemySpritemapDraw { Bank = frame.Bank, Pointer = frame.Pointer, Origin = new(x, y), PaletteBits = palette, BaseTileIndex = baseTile });
                installed.AddEnemySpritemap(parts, new Position16<ScreenPixels>(x, y), palette, baseTile);
                AssertTrue(SuperMetroid.Verification.Harness.OamPriorityComparison.SameApartFromPriority(native, installed),
                    $"installed {frame.Name} OAM matches native at {x:X4},{y:X4} apart from the priority tier each part's depth replaces (#2550)");
            }
            // Bank-$B4 frames belong to the room sprite-object renderer, not an
            // enemy slot whose definition selects an $A0-$B3 spritemap bank.
            if (frame.Bank == EnemySpritemapDefinitions.RoomEffectActorBank)
                continue;
            // The head/neck use a private world-space hook, and the falling tubes
            // use Mother Brain's encounter route. Test that route separately below.
            if (frame.Bank == MotherBrainVisualToolingDefinitions.Bank)
                continue;
            if (frame.LegacyName.StartsWith("ridley_supplement_", StringComparison.Ordinal))
                continue;
            // A room draws a frame as the enemy whose behaviour shows it; a frame only an enemy drawing from the characters
            // it uploads shows, as Ridley carrying the Ceres Baby, draws through that enemy's own route instead.
            if (FrameOwner(RoomEnemyPlaceFrameAccess.FrameAt(frame.Bank, frame.Pointer)) is not { } owner)
                continue;
            // An enemy no retail room places, as the shootable horizontal shutter, has no production room to draw it in.
            if (!SpawnedObjectFixtures.Drawn(owner))
                continue;
            RoomEnemyPlace? drawn = null;
            RoomEnemySystem? drawnRoom = null;
            int drawnRow = 0;
            int drawnBase = 0;
            var room = DrawEnemy(stock, NewFrameReadGuard(rom, frame.Name), frame.Pointer, owner, inspect: (place, system) =>
                {
                    drawn = place;
                    drawnRoom = system;
                    drawnRow = place.PaletteRowIn(system);
                    drawnBase = place.NativeTileBaseIn(system);
                },
                spritemapBank: frame.Bank);
            var nativeRoom = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeRoom, new NativeEnemySpritemapDraw
            {
                Bank = frame.Bank, Pointer = frame.Pointer, Origin = new(0x0040, 0x0080),
                PaletteBits = NativeSpriteGraphicsWords.PaletteBits(drawn is null ? throw new InvalidOperationException("Verification requires the drawn slot.") : drawnRow),
                BaseTileIndex = checked((ushort)drawnBase),
            });
            // A picture the sheet keeps once for several identical frames stands for any of their OAM (#1513); each part's
            // depth replaces its priority tier (#2550).
            bool stands = SuperMetroid.Verification.Harness.OamPriorityComparison.ImagesEqualApartFromPriority(nativeRoom, room) || SpriteFrameBufferAccess.DrawnStandsFor(sprites =>
                {
                    RoomEnemySystem redrawn = drawnRoom ?? throw new InvalidOperationException("Verification requires the drawn room.");
                    redrawn.Drawing.Draw(redrawn.Drawing.Advance(new(0, 0)), sprites, new(0, 0), RoomEnemyFrameFixtures.StandaloneSamus());
                }, nativeRoom);
            if (!stands)
                differing.Add($"{frame.Name} (drawn by {owner}): native {OamRecords(nativeRoom)}, installed {OamRecords(room)}");
        }
        AssertTrue(differing.Count == 0,
            $"production room draws every installed frame without visual ROM reads; {differing.Count} differ (x, y, tile, attributes):\n{string.Join("\n", differing)}");

        Suite(nameof(VerifyInstalledMotherBrainDrawHook), () => VerifyInstalledMotherBrainDrawHook(rom, stock));

        // The used records of an OAM low table, for a failure message.
        static string OamRecords(OamBuffer oam) => string.Join(" ",
            Enumerable.Range(0, oam.NextByteOffset / 4).Select(record =>
                $"({oam.LowTable[record * 4]:X2},{oam.LowTable[(record * 4) + 1]:X2},{oam.LowTable[(record * 4) + 2]:X2},{oam.LowTable[(record * 4) + 3]:X2})"));

        // Ridley's scene adds his tail and wings to his body, so his frames are checked through his scene's own route.
        static EnemyDefinitionId? FrameOwner(EnemyFrameId? frame) => frame is not { } shown ? null
            : EnemyDefinitionFrameDefinitions.Frames.Where(entry => entry.Value.Contains(shown)
                    && entry.Key is not (EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.RidleyBreakupPiece))
                .Select(entry => (EnemyDefinitionId?)entry.Key).Order().FirstOrDefault();

        static OamBuffer DrawEnemy(ImportEnemyArtwork art,
            ISnesAddressSpace guard, ushort pointer, EnemyDefinitionId definition,
            Action<RoomEnemyPlace, RoomEnemySystem>? inspect = null, byte? spritemapBank = null)
        {
            var enemies = EnemySystemFixtures.PlacingAnyEnemy(EnemySessionFixtures.WithInstalledServices(art.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: art.Bosses);
            // A species that asks whether its area's boss is dead sees it alive.
            enemies.VerificationServices().IsAreaBossDefeated = () => false;
            // The enemy draws as a room drawing it loads it: at its tile base and in its palette there.
            enemies.DrawUnder(SpawnedObjectFixtures.SetDrawing(definition));
            enemies.Drawing.Enqueue(enemies.Places.InUpdateOrder[0], reached: true);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            // The enemy is placed as a retail room places it, so an actor its placement's settings choose, as an ordinary
            // Ceres door its facing (#1989), binds as it does there.
            if (SuperMetroid.AssetExtraction.World.RoomStates.RoomStateDefinitions.All.SelectMany(state => state.EnemyPopulation.Placements.ToArray())
                    .FirstOrDefault(placement => placement.DefinitionId == definition) is { } placed)
                place.PopulateFor(definition, placed.SpawnSettings);
            place.Bind(definition, place.KeptDefinition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            // A boss's part draws in a room whose boss has attached the body its body frames show on (#2573).
            BossBodyCanvasFixtures.AttachBodyOfPart(enemies, place);
            place.Shading = PictureShading.Standard;
            // The catalog-wide composition check includes families newer than the
            // fixture's actor-name mapping. Its frame identity owns the actual bank.
            byte frameBank = spritemapBank ?? (definition == EnemyDefinitionId.YappingMaw
                    ? YappingMawVisualDefinitions.Bank
                    : definition is EnemyDefinitionId.KihunterGreen or
                        EnemyDefinitionId.KihunterGreenWings or
                        EnemyDefinitionId.KihunterYellow or
                        EnemyDefinitionId.KihunterYellowWings or
                        EnemyDefinitionId.KihunterRed or
                        EnemyDefinitionId.KihunterRedWings
                    ? KiHunterVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.FakeKraid
                    ? EnemySpritemapDefinitions.FakeKraidBank
                    : definition is EnemyDefinitionId.KraidFirstNail or
                        EnemyDefinitionId.KraidSecondNail
                        ? EnemySpritemapDefinitions.KraidNailBank
                    : definition is EnemyDefinitionId.Zeb or
                    EnemyDefinitionId.Zebbo or
                    EnemyDefinitionId.Gamet or
                    EnemyDefinitionId.Geega
                    ? EnemySpritemapDefinitions.PipeBugBank
                    : definition is EnemyDefinitionId.CeresDoor or EnemyDefinitionId.CeresRidleyRoomDoor or EnemyDefinitionId.CeresElevatorDoor
                        or EnemyDefinitionId.CeresElevatorRoomWall or EnemyDefinitionId.CeresGetawayChamberWall
                        ? CeresDoorFrameDefinitionsConstants.Bank
                    : definition == EnemyDefinitionId.Boulder
                    ? EnemySpritemapDefinitions.BoulderBank
                    : definition == EnemyDefinitionId.Atomic
                        ? EnemySpritemapDefinitions.AtomicBank
                    : definition == EnemyDefinitionId.Magdollite
                        ? EnemySpritemapDefinitions.MagdolliteBank
                    : definition is EnemyDefinitionId.Fune or
                        EnemyDefinitionId.Namihe
                        ? EnemySpritemapDefinitions.FuneNamiheBank
                    : definition == EnemyDefinitionId.Bull
                        ? EnemySpritemapDefinitions.BullBank
                    : definition == EnemyDefinitionId.Alcoon
                        ? EnemySpritemapDefinitions.AlcoonBank
                    : definition == EnemyDefinitionId.Beetom
                        ? EnemySpritemapDefinitions.BeetomBank
                    : definition is EnemyDefinitionId.Sidehopper or
                        EnemyDefinitionId.Dessgeega or
                        EnemyDefinitionId.SidehopperLarge or
                        EnemyDefinitionId.SidehopperTourian or
                        EnemyDefinitionId.DessgeegaLarge
                        ? EnemySpritemapDefinitions.HopperBank
                    : definition == EnemyDefinitionId.Choot
                        ? EnemySpritemapDefinitions.ChootBank
                    : definition is EnemyDefinitionId.Mellow or
                        EnemyDefinitionId.Mella or EnemyDefinitionId.MaridiaFly
                        ? FlyVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.Kago
                        ? KagoVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.FaceBlock
                        ? BlueBrinstarFaceBlockVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.MorphBallEye
                        ? MorphBallEyeVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.Metroid
                        ? MetroidVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.Mochtroid
                        ? MochtroidVisualDefinitions.Bank
                    : definition is EnemyDefinitionId.Evir or
                        EnemyDefinitionId.EvirProjectile
                        ? EvirVisualDefinitions.Bank
                    : definition is EnemyDefinitionId.WorkRobot or
                        EnemyDefinitionId.UnpoweredWorkRobot
                        ? WorkRobotVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.Yard
                        ? YardVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.Botwoon
                        ? BotwoonVisualDefinitionsConstants.Bank
                    : definition == EnemyDefinitionId.Shaktool
                        ? ShaktoolVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.Chozo
                        ? ChozoStatueVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.Viola
                        ? ViolaVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.Rinka
                        ? RinkaVisualDefinitions.Bank
                    : definition is EnemyDefinitionId.Tripper or
                        EnemyDefinitionId.SuspensorPlatform
                        ? TripperKamerVisualDefinitions.Bank
                    : definition is EnemyDefinitionId.ShutterGrowing or
                        EnemyDefinitionId.ShutterShootable or
                        EnemyDefinitionId.ShutterDestroyable
                        ? ShutterVisualDefinitions.Bank
                    : definition is EnemyDefinitionId.OrangeZoomer or
                        EnemyDefinitionId.Sciser or
                        EnemyDefinitionId.Zeela or
                        EnemyDefinitionId.Sova or
                        EnemyDefinitionId.Zoomer or
                        EnemyDefinitionId.ArmoredZoomer
                        ? EnemySpritemapDefinitions.OrangeZoomerBank
                    : definition is EnemyDefinitionId.Sbug
                        ? EnemySpritemapDefinitions.SbugBank
                        : definition is EnemyDefinitionId.Elevator or
                          EnemyDefinitionId.Skultera or
                          EnemyDefinitionId.Waver or
                          EnemyDefinitionId.Fireflea or
                          EnemyDefinitionId.Zoa or
                          EnemyDefinitionId.Metaree or
                          EnemyDefinitionId.Skree
                            ? EnemySpritemapDefinitions.SkulteraBank
                        : EnemySpritemapDefinitions.BoyonBank);
            place.Frame = RoomEnemyPlaceFrameAccess.FrameAt(frameBank, pointer);
            if (place.Frame is { } shown)
                SpawnedObjectFixtures.ShowDrawnLooks(enemies, SuperMetroid.AssetExtraction.Enemies.FrameDrawings.Of(shown));
            place.AnimationClock.HoldFor(7);
            place.Position = place.Position.WithWhole(new(0x0040, 0x0080));
            var oam = new OamBuffer();
            enemies.Drawing.DrawAsOam(oam, new(0, 0));
            inspect?.Invoke(place, enemies);
            return oam;
        }
    }

    /// <summary>Checks Mother Brain's private draw hook against native head frames and above-screen culling.</summary>
    private static void VerifyInstalledMotherBrainDrawHook(
        CartridgeImportAddressSpace rom, ImportEnemyArtwork stock)
    {
        // Mother Brain's room's graphics set, which gives the tile base and colours it loads her head with.
        RoomEnemyGraphicsSetDefinition headSet = SpawnedObjectFixtures.SetDrawing(EnemyDefinitionId.MotherBrainHead);
        int tileBase = headSet.TileBaseOf(EnemyDefinitionId.MotherBrainHead);
        int indexesRow = headSet.DrawRow(EnemyDefinitionId.MotherBrainHead);
        // The head's draw hook shows the head's frames; her falling tubes draw from their own sheet through her encounter.
        foreach (NativeEnemySpritemapDefinition frame in MotherBrainVisualDefinitionsConstants.Frames().Take(MotherBrainVisualDefinitionsConstants.HeadFrameCount))
        {
            OamBuffer actual = DrawInstalledMotherBrainFrame(
                stock, frame.Pointer, new(0x0140, 0x00a0));
            var expected = new OamBuffer();
            DrawImportedEnemySpritemap(rom, expected, new NativeEnemySpritemapDraw { Bank = frame.Bank, Pointer = frame.Pointer, Origin = new(0x0040, 0x0090), PaletteBits = NativeSpriteGraphicsWords.PaletteBits(indexesRow), BaseTileIndex = tileBase });
            AssertTrue(actual.ImageEquals(expected) &&
                       actual.NextByteOffset == expected.NextByteOffset,
                $"Mother Brain private draw uses installed {frame.Name} with native OAM");
        }

        // A head whose origin lies a row above the view is still published, a row above the screen, for the display to
        // clip (#1587).
        var aboveScreen = new SpriteFrameBuffer();
        aboveScreen.BeginFrame();
        PublishInstalledMotherBrainFrame(stock, 0xa586, new(0x0140, 0x000f), aboveScreen);
        aboveScreen.FinalizeFrame();
        AssertEqual(1, aboveScreen.Draws.Length, "Mother Brain private draw publishes a head above the viewport");
        AssertEqual(-1, CoordinateAxis.AsSigned(aboveScreen.Draws[0].Origin.Y),
            "Mother Brain private draw publishes a head above the viewport at its row above the screen");
    }

    /// <summary>Draws one Mother Brain world frame through the installed private renderer under a cartridge-read guard.</summary>
    private static OamBuffer DrawInstalledMotherBrainFrame(
        ImportEnemyArtwork art, ushort pointer,
        Position16<RoomPixels> world)
    {
        var oam = new OamBuffer();
        oam.AppendDrawn(sprites => PublishInstalledMotherBrainFrame(art, pointer, world, sprites));
        return oam;
    }

    /// <summary>Publishes one Mother Brain world frame through the installed private renderer, under a camera at (256, 16).</summary>
    private static void PublishInstalledMotherBrainFrame(
        ImportEnemyArtwork art, ushort pointer,
        Position16<RoomPixels> world, SpriteFrameBuffer sprites)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(art.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: art.Bosses);
        // She draws from the sheet her room's set composes from this artwork, at the indexes it loads her with.
        SpawnedObjectFixtures.LoadSetDrawing(enemies, EnemyDefinitionId.MotherBrainHead);
        EnemyFrameId frame = RoomEnemyPlaceFrameAccess.FrameAt(MotherBrainVisualToolingDefinitions.Bank, pointer)
            ?? throw new InvalidOperationException("A Mother Brain draw shows a frame.");
        enemies.MotherBrainBehavior.Head.DrawMotherBrainWorldFrame(sprites,
            new SpriteSource(SpriteSourceKind.MotherBrainBrain), new MotherBrainWorldFrame(frame, world, PictureShading.Standard),
            new Position16<RoomPixels>(0x0100, 0x0010), corpseRows: null);
    }

    /// <summary>Rejects native visual-data reads for the enemy frame under inspection.</summary>
    private static CartridgeReadGuard NewFrameReadGuard(ISnesAddressSpace source,
        string? frameName = null) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa288da and < 0xa2890b or
                    >= 0xa2a0bb and < 0xa2a377 or
                    >= 0xa68a59 and < 0xa68b09 or
                    >= 0xa8e489 and < 0xa8e587 or
                    >= 0xa3881e and < 0xa388f0 or
                    >= 0xa38b65 and < 0xa38c0f or
                    >= 0xa3b55f and < 0xa3b5b3 or
                    >= 0xa3c842 and < 0xa3c8a6 or
                    >= 0xa3928a and < 0xa394aa or
                    >= 0xb389b7 and < 0xb389fd or
                    >= 0xb38a6d and < 0xb38ac1 or
                    >= 0xb38e96 and < 0xb38edc or
                    >= 0xb392ad and < 0xb39301 or
                    >= 0xa69c64 and < 0xa6a0e0 or
                    >= 0xa7a617 and < 0xa7a69f or
                    >= 0xa2a589 and < 0xa2a59e or
                    >= 0xa28aca and < 0xa28b60 or
                    >= 0xa2e3c5 and < 0xa2e457 or
                    >= 0xa2e527 and < 0xa2e56f or
                    >= 0xa38ea5 and < 0xa3900a or
                    >= 0xa8b448 and < 0xa8b65e or
                    >= 0xa6f921 and < 0xa6fb72 or
                    >= 0xa6a329 and < 0xa6a353 or
                    >= 0xa6bffd and < 0xa6c04e or
                    >= 0xa29df6 and < 0xa29e80 or
                    >= 0xa8db76 and < 0xa8dbb8 or
                    >= 0xa8dfa2 and < 0xa8e214 or
                    >= 0xa8bed3 and < 0xa8c143 or
                    >= 0xa3aee3 and < 0xa3b390 or
                    >= 0xa2e146 and < 0xa2e180 or
                    >= 0xa3e2e8 and < 0xa3e580 or
                    >= 0xa893f9 and < 0xa8959d or
                    >= 0xa897b4 and < 0xa899ac or
                    >= 0xaadf5c and < 0xaae03d or
                    >= 0xa39f29 and < 0xa3a051 or
                    >= 0xa2f468 and < 0xa2f498 or
                    >= 0xa3962f and < 0xa3965b,
                address => $"Installed enemy draw for {frameName ?? "an unnamed frame"} read native visual byte ${address:X6}.");
}
