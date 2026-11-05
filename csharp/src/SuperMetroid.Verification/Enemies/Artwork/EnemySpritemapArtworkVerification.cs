using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Verification.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Verification.Enemies.Choot;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.Core.Enemies.Ripper.Artwork;
using SuperMetroid.Tooling;
using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Enemies.Alcoon.AlcoonInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Beetom.BeetomInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonVisualsVerification;
using static SuperMetroid.Verification.Enemies.Corpses.DeadTorizoArtworkVerification;
using static SuperMetroid.Verification.Enemies.Evir.EvirVisualsVerification;
using static SuperMetroid.Verification.Enemies.Crawlers.HZoomerArtworkVerification;
using static SuperMetroid.Verification.Enemies.Hoppers.HopperAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Enemies.Metroids.MochtroidVisualsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.RidleySupplementalArtworkVerification;
using static SuperMetroid.Verification.Enemies.Crawlers.SharedCrawlerArtworkVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.WorkRobots.WorkRobotVisualsVerification;
using static SuperMetroid.Verification.Enemies.Yard.YardVisualsVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Shutters;
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
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed enemy OAM compositions against pinned cartridge frames and drawing paths.</summary>
internal static class EnemySpritemapArtworkVerification
{
    /// <summary>Runs the Alcoon in a real room with the installed artwork.</summary>
    private static void VerifyInstalledAlcoonInstructionFrames(
        SuperMetroidAddressSpace rom, InstalledEnemyArtwork stock)
    {
        Suite(nameof(VerifyAlcoonInstructionProgramDefinitions), () => VerifyAlcoonInstructionProgramDefinitions(rom, stock));
    }

    /// <summary>Runs the Beetom programs with the installed artwork.</summary>
    private static void VerifyInstalledBeetomInstructionFrames(
        SuperMetroidAddressSpace rom, InstalledEnemyArtwork stock)
    {
        Suite(nameof(VerifyBeetomInstructionProgramDefinitions), () => VerifyBeetomInstructionProgramDefinitions(rom, stock));
    }

    /// <summary>Runs the hopper family's typed animations with the installed stock artwork.</summary>
    private static void VerifyInstalledHopperInstructionFrames(
        SuperMetroidAddressSpace rom, InstalledEnemyArtwork stock)
    {
        Suite(nameof(VerifyHopperAnimationDefinitions), () => VerifyHopperAnimationDefinitions(rom, stock));
    }

    /// <summary>Runs Choot's idle, jumping and falling animations with the installed stock artwork.</summary>
    private static void VerifyInstalledChootInstructionFrames(
        InstalledEnemyArtwork stock)
    {
        Suite(nameof(ChootBehaviorVerification.VerifyChootBehavior), () => ChootBehaviorVerification.VerifyChootBehavior(stock));
    }

    /// <summary>Checks Bull frame selectors and exercises normal and immune-shot programs without cartridge visual reads.</summary>
    private static void VerifyInstalledBullInstructionFrames(
        InstalledEnemyArtwork stock)
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses };
        RoomEnemySlot bull = enemies.SlotTable.Slots[0];
        bull.Bind(EnemyDefinitionId.Bull, bull.Definition);
        bull.Bind(bull.DefinitionId, default(RoomEnemyDefinition));
        bull.SetSpawnWords(EnemyDefinitionId.Bull, words => words);
        new Action<RoomEnemySlot>(enemies.BullBehavior.InitializeBull)(bull);
        object?[] arguments =
            [bull, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
        for (int frame = 0; frame < 41; frame++)
            EnemyInstructionRunner.Process(arguments);
        AssertEqual(BullAnimationsConstants.Spritemaps(BullProgram.Normal).First(), bull.SpritemapPointer,
            "installed Bull normal animation loops back to its first frame on time");
        BullEnemyState state = enemies.BullStates[0] ?? throw new InvalidDataException(
            "Bull initializer did not publish typed state.");
        BullBehavior.ResolveBullImmuneShot(bull, state, SamusProjectileDirection.Right);
        for (int frame = 0; frame < 61; frame++)
            EnemyInstructionRunner.Process(arguments);
        AssertEqual(BullAnimationsConstants.Spritemaps(BullProgram.Normal).First(), bull.SpritemapPointer,
            "installed Bull immune-shot program returns to the normal loop");
        AssertEqual((ushort)0, bull.Timer.RawValue,
            "installed Bull immune-shot loop exhausts its native repeat timer");
    }

    /// <summary>Runs Puyo's grounded and airborne programs with the installed stock artwork.</summary>
    private static void VerifyInstalledPuyoInstructionFrames(
        InstalledEnemyArtwork stock) =>
        Suite(nameof(PuyoInstructionProgramDefinitionsVerification.VerifyPuyoInstructionProgramDefinitions),
            () => PuyoInstructionProgramDefinitionsVerification.VerifyPuyoInstructionProgramDefinitions(stock));

    /// <summary>Compares installed enemy compositions and rendering with the cartridge's OAM definitions.</summary>
    internal static void VerifyInstalledEnemySpritemaps(
        SuperMetroidAddressSpace rom,
        InstalledEnemyArtwork stock)
    {
        AssertTrue(stock.Enemies.Spritemaps is not null,
            "installed enemy catalog contains named OAM compositions");
        AssertEqual(EnemySpritemapDefinitions.PreCeresDoorFrameCount,
            EnemySpritemapDefinitions.PreDisplayBindingsFrameCount,
            "Ceres-door expansion retains the previous display-binding identities");
        AssertEqual(EnemySpritemapDefinitions.PreCeresDoorFrameCount + 15,
            EnemySpritemapDefinitions.PreCeresBabyFrameCount,
            "Ceres door adds its thirteen selected poses, initial pose, and private overlay");
        AssertEqual(EnemySpritemapDefinitions.PreCeresBabyFrameCount + 3,
            EnemySpritemapDefinitions.PreRioFrameCount,
            "Ceres Baby adds its three authored OAM poses");
        AssertEqual(EnemySpritemapDefinitions.PreRioFrameCount + 8,
            EnemySpritemapDefinitions.PreLowerNorfairRioFrameCount,
            "Rio adds its eight distinct OAM compositions");
        AssertEqual(EnemySpritemapDefinitions.PreLowerNorfairRioFrameCount + 18,
            EnemySpritemapDefinitions.PreNorfairRioFrameCount,
            "Lower Norfair Rio adds its eighteen parent and flame compositions");
        AssertEqual(EnemySpritemapDefinitions.PreNorfairRioFrameCount + 20,
            EnemySpritemapDefinitions.PrePuyoFrameCount,
            "Norfair Rio adds its twenty parent and flame compositions");
        AssertEqual(EnemySpritemapDefinitions.PrePuyoFrameCount + 8,
            EnemySpritemapDefinitions.PreBullFrameCount,
            "Puyo adds its eight distinct ground and airborne compositions");
        AssertEqual(EnemySpritemapDefinitions.PreBullFrameCount + 3,
            EnemySpritemapDefinitions.PreAlcoonFrameCount,
            "Bull adds its three distinct normal and immune-shot compositions");
        AssertEqual(EnemySpritemapDefinitions.PreAlcoonFrameCount + 18,
            EnemySpritemapDefinitions.PreBeetomFrameCount,
            "Alcoon adds eighteen left/right walking, firing, and airborne compositions");
        AssertEqual(EnemySpritemapDefinitions.PreBeetomFrameCount + 22,
            EnemySpritemapDefinitions.PreHopperFrameCount,
            "Beetom adds twenty-two distinct crawl, hop, and drain compositions");
        AssertEqual(EnemySpritemapDefinitions.PreHopperFrameCount + 24,
            EnemySpritemapDefinitions.PreChootFrameCount,
            "Hoppers add twenty-four distinct floor/ceiling compositions");
        AssertEqual(EnemySpritemapDefinitions.PreChootFrameCount + 4,
            EnemySpritemapDefinitions.PreHZoomerFrameCount,
            "Choot adds four idle, jumping, and falling compositions");
        AssertEqual(EnemySpritemapDefinitions.PreHZoomerFrameCount + 20,
            EnemySpritemapDefinitions.PreSbugFrameCount,
            "HZoomer adds five frames in each of four surface orientations");
        AssertEqual(EnemySpritemapDefinitions.PreSbugFrameCount + 24,
            EnemySpritemapDefinitions.PreFuneNamiheFrameCount,
            "Sbug adds three distinct frames in each of eight facing directions");
        AssertEqual(EnemySpritemapDefinitions.PreFuneNamiheFrameCount + 22,
            EnemySpritemapDefinitions.PreKamerFrameCount,
            "Fune/Namihe add all left/right idle and active OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreKamerFrameCount + 4,
            EnemySpritemapDefinitions.PreElevatorFrameCount,
            "Kamer platform adds its four native OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreElevatorFrameCount + 2,
            EnemySpritemapDefinitions.PreDraygonIntroFrameCount,
            "elevator adds its two native OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreDraygonIntroFrameCount + 4,
            EnemySpritemapDefinitions.PreDraygonBreathFrameCount,
            "Draygon intro adds its four native Evir sprite-object frames");
        AssertEqual(EnemySpritemapDefinitions.PreDraygonBreathFrameCount + 9,
            EnemySpritemapDefinitions.PreRoomSpriteObjectFrameCount,
            "Draygon breath bubble adds its nine native sprite-object frames");
        AssertEqual(EnemySpritemapDefinitions.PreRoomSpriteObjectFrameCount + 263,
            EnemySpritemapDefinitions.PreYappingMawFrameCount,
            "remaining room sprite-object programs add 263 distinct visual frames");
        AssertEqual(EnemySpritemapDefinitions.PreYappingMawFrameCount + 24,
            EnemySpritemapDefinitions.PreKiHunterFrameCount,
            "Yapping Maw adds 24 distinct OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreKiHunterFrameCount + 41,
            EnemySpritemapDefinitions.PreMotherBrainFrameCount,
            "KiHunter adds 41 distinct body and wing OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreMotherBrainFrameCount + 18,
            EnemySpritemapDefinitions.PreDeadTorizoFrameCount,
            "Mother Brain adds eighteen head, neck, and falling-tube OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreDeadTorizoFrameCount + 1,
            EnemySpritemapDefinitions.PreRidleySupplementFrameCount,
            "Dead Torizo adds its private corpse OAM frame");
        AssertEqual(EnemySpritemapDefinitions.PreRidleySupplementFrameCount + 31,
            EnemySpritemapDefinitions.PreSciserFrameCount,
            "Ridley adds sixteen tips, twelve wings, and three tail segment OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreSciserFrameCount + 12,
            EnemySpritemapDefinitions.PreFlyFrameCount,
            "Sciser adds twelve distinct surface-animation OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreFlyFrameCount + 4,
            EnemySpritemapDefinitions.PreKagoFrameCount,
            "Mellow, Mella and Memu share four flight OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreKagoFrameCount + 3,
            EnemySpritemapDefinitions.PreFaceBlockFrameCount,
            "Kago adds three OAM frames shared by its slow and fast loops");
        AssertEqual(EnemySpritemapDefinitions.PreFaceBlockFrameCount + 5,
            EnemySpritemapDefinitions.PreMorphBallEyeFrameCount,
            "face block adds five distinct neutral and directional OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreMorphBallEyeFrameCount + 22,
            EnemySpritemapDefinitions.PreShutterFrameCount,
            "Morph Ball eye and mount add twenty-two distinct OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreShutterFrameCount + 5,
            EnemySpritemapDefinitions.PreMetroidFrameCount,
            "growing, vertical and horizontal shutters share five distinct OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreMetroidFrameCount + 4,
            EnemySpritemapDefinitions.PreShaktoolFrameCount,
            "both ordinary-Metroid loops share four distinct body OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreShaktoolFrameCount + 15,
            EnemySpritemapDefinitions.PreTripperKamerFrameCount,
            "Shaktool's saw, arm, and head programs select fifteen OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreTripperKamerFrameCount + 18,
            EnemySpritemapDefinitions.PreDragonFrameCount,
            "Tripper/Kamer add sixteen animated and two frozen OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreDragonFrameCount + 12,
            EnemySpritemapDefinitions.PreMultiviolaFrameCount,
            "Dragon adds twelve distinct body and wing OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreMultiviolaFrameCount +
                MultiviolaVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreNorfairLavaJumperFrameCount,
            "Multiviola adds eight distinct spinning OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreNorfairLavaJumperFrameCount +
                NorfairLavaJumperVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreChozoStatueFrameCount,
            "Norfair lava jumper adds ten visible and one empty OAM frame");
        AssertEqual(EnemySpritemapDefinitions.PreChozoStatueFrameCount +
                ChozoStatueVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreViolaFrameCount,
            "Lower Norfair and Wrecked Ship Chozo share twenty-six OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreViolaFrameCount +
                ViolaVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreRinkaFrameCount,
            "Viola adds eight distinct OAM frames");
        AssertEqual(EnemySpritemapDefinitions.PreRinkaFrameCount +
                RinkaVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreDeadTorizoStationaryFrameCount,
            "Rinka adds five OAM frames shared by ordinary and boss loops");
        AssertEqual(EnemySpritemapDefinitions.PreDeadTorizoStationaryFrameCount + 1,
            EnemySpritemapDefinitions.PreDeadTourianCorpseFrameCount,
            "Dead Torizo adds its stationary corpse composition after version 53");
        AssertEqual(EnemySpritemapDefinitions.PreDeadTourianCorpseFrameCount +
                DeadTourianCorpseVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreMochtroidFrameCount,
            "dead Zoomer, Ripper, Skree, and Sidehopper add thirteen compositions");
        AssertEqual(EnemySpritemapDefinitions.PreMochtroidFrameCount +
                MochtroidVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreEvirFrameCount,
            "Mochtroid adds six free-flight and attached OAM compositions");
        AssertEqual(EnemySpritemapDefinitions.PreEvirFrameCount +
                EvirVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreWorkRobotFrameCount,
            "Evir adds 24 body, arms, and projectile compositions");
        AssertEqual(EnemySpritemapDefinitions.PreWorkRobotFrameCount +
                WorkRobotVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreYardFrameCount,
            "Work Robot adds 27 powered and unpowered compositions");
        AssertEqual(EnemySpritemapDefinitions.PreYardFrameCount +
                YardVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreBotwoonFrameCount,
            "Yard adds 104 crawling, turn, hiding, and airborne compositions");
        AssertEqual(EnemySpritemapDefinitions.PreBotwoonFrameCount +
                BotwoonVisualDefinitionsConstants.Frames().Length,
            EnemySpritemapDefinitions.PreGunshipFrameCount,
            "Botwoon adds sixteen distinct visible head compositions");
        AssertEqual(EnemySpritemapDefinitions.PreGunshipFrameCount +
                GunshipVisualDefinitions.FrameCount,
            EnemySpritemapDefinitions.PreMamaTurtleFrameCount,
            "Gunship adds two hull and eleven entrance-pad compositions");
        AssertEqual(EnemySpritemapDefinitions.PreMamaTurtleFrameCount +
                MamaTurtleVisualDefinitions.FrameCount + ZeroVisualDefinitions.FrameCount +
                FriendlyAnimalVisualDefinitions.EtecoonFrameCount +
                FriendlyAnimalVisualDefinitions.DachoraFrameCount +
                FriendlyAnimalVisualDefinitions.EscapeEtecoonFrameCount +
                FriendlyAnimalVisualDefinitions.EscapeDachoraFrameCount +
                HibashiVisualDefinitions.FrameCount + ZebetiteVisualToolingDefinitions.FrameCount +
                WreckedShipGhostVisualDefinitions.FrameCount + PowampVisualDefinitions.FrameCount +
                SparkVisualDefinitions.FrameCount + ShitroidVisualDefinitions.FrameCount +
                KraidLintVisualDefinitions.FrameCount + NuclearWaffleVisualDefinitions.FrameCount +
                2 + // Single-frame Kzan and Polyp.
                RidleyBreakupVisualDefinitions.Legs.Length + RidleyBreakupVisualDefinitions.Torso.Length +
                RidleyBreakupVisualDefinitions.Head.Length + RidleyBreakupVisualDefinitions.Claw.Length,
            EnemySpritemapDefinitions.Frames.Length,
            "post-Gunship installed composition groups account for the complete current catalog");
        HashSet<ushort> installedHunterPointers = EnemySpritemapDefinitions.Frames
            
            .Where(frame => frame.LegacyName.StartsWith("ki_hunter_a8_", StringComparison.Ordinal))
            .Select(frame => frame.Pointer)
            .ToHashSet();
        AssertTrue(KiHunterAnimationsConstants.Spritemaps(Enum.GetValues<KiHunterProgram>()).ToHashSet().SetEquals(
                installedHunterPointers),
            "the Ki-Hunter's programs show exactly its installed frames");
        AssertEqual(41, installedHunterPointers.Count, "KiHunter's programs show 41 distinct OAM frames");
        HashSet<ushort> installedMawPointers = EnemySpritemapDefinitions.Frames
            
            .Where(frame => frame.LegacyName.StartsWith("yapping_maw_a8_", StringComparison.Ordinal))
            .Select(frame => frame.Pointer)
            .ToHashSet();
        AssertTrue(YappingMawAnimationsConstants.Spritemaps(Enum.GetValues<YappingMawProgram>()).ToHashSet().SetEquals(installedMawPointers),
            "every installed Yapping Maw frame is shown by a program, and every shown pose is installed");
        AssertEqual(24, installedMawPointers.Count, "Yapping Maw's programs show 24 distinct frames");
        AssertEqual(276, EnemySpritemapDefinitions.Frames.Count(
                frame => frame.Bank == EnemySpritemapDefinitions.RoomSpriteObjectBank),
            "all bank-B4 sprite-object programs select 276 distinct visual frames");
        HashSet<ushort> installedSpritePointers = EnemySpritemapDefinitions.Frames
            
            .Where(frame => frame.Bank == EnemySpritemapDefinitions.RoomSpriteObjectBank)
            .Select(frame => frame.Pointer)
            .ToHashSet();
        AssertTrue(Enum.GetValues<RoomSpriteObjectFrame>().Select(RoomSpriteObjectVisualDefinitionsConstants.Spritemap).ToHashSet()
                .SetEquals(SuperMetroid.Verification.Rooms.SpriteObjects.RoomSpriteObjectNativeLists.SpritemapPointers(rom)),
            "the room sprite-object frames are exactly the spritemaps the native lists show");
        foreach (RoomSpriteObjectFrame frame in Enum.GetValues<RoomSpriteObjectFrame>())
            AssertTrue(installedSpritePointers.Contains(RoomSpriteObjectVisualDefinitionsConstants.Spritemap(frame)),
                $"room sprite-object frame {frame} has installed art");
        AssertTrue(FuneNamiheAnimationsConstants.Spritemaps(Enum.GetValues<FuneNamiheProgram>()).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.Name.StartsWith("fune_", StringComparison.Ordinal) ||
                    frame.Name.StartsWith("namihe_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Fune's and Namihe's programs show exactly their installed frames");
        AssertTrue(RioAnimationsConstants.Spritemaps(Enum.GetValues<RioProgram>()).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.LegacyName.StartsWith("rio_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Rio's programs show exactly its eight installed frames");
        AssertTrue(LowerNorfairRioAnimationsConstants.Spritemaps(Enum.GetValues<LowerNorfairRioProgram>()).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.LegacyName.StartsWith("lower_norfair_rio_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Lower Norfair Rio's programs show exactly its installed frames");
        AssertTrue(NorfairRioAnimationsConstants.Spritemaps(Enum.GetValues<NorfairRioProgram>()).ToHashSet().SetEquals(
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
        Suite(nameof(VerifyInstalledHZoomerInstructionFrames), () => VerifyInstalledHZoomerInstructionFrames(stock));
        Suite(nameof(VerifyInstalledSharedCrawlerFrames), () => VerifyInstalledSharedCrawlerFrames(stock.Enemies));
        AssertTrue(MorphBallEyeVisualDefinitions.Frames().Select(frame => frame.Pointer).ToHashSet().SetEquals(
                MorphBallEyeAnimationsConstants.Spritemaps(Enum.GetValues<MorphBallEyeProgram>())),
            "the installed Morph Ball eye frames are exactly the poses its programs show");
        HashSet<ushort> shaktoolFrames = ShaktoolVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(15, shaktoolFrames.Count,
            "Shaktool's visual frame identities are distinct");
        AssertTrue(ShaktoolAnimationsConstants.Spritemaps(Enum.GetValues<ShaktoolProgram>()).ToHashSet().SetEquals(shaktoolFrames),
            "the Shaktool's programs show exactly its installed frames");
        HashSet<ushort> platformFramePointers = TripperKamerVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(18, platformFramePointers.Count,
            "Tripper/Kamer visual frame identities are distinct");
        HashSet<ushort> shownPlatformFrames = PlatformAnimationsConstants.Spritemaps(Enum.GetValues<PlatformProgram>()).ToHashSet();
        AssertEqual(16, shownPlatformFrames.Count, "platform programs show sixteen distinct animated compositions");
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
        AssertTrue(dragonFrames.SetEquals(DragonAnimationsConstants.Spritemaps(Enum.GetValues<DragonProgram>())),
            "every installed Dragon body and wing frame is a pose its programs show");
        HashSet<ushort> multiviolaFrames = MultiviolaVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(MultiviolaVisualDefinitions.FrameCount, multiviolaFrames.Count,
            "Multiviola's fourteen-frame loop selects eight distinct OAM frames");
        AssertTrue(MultiviolaAnimationsConstants.Spritemaps(MultiviolaProgram.Flying).ToHashSet().SetEquals(multiviolaFrames),
            "every installed Multiviola composition is shown by its loop");
        HashSet<ushort> lavaJumperFrames = NorfairLavaJumperVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(NorfairLavaJumperVisualDefinitions.FrameCount,
            lavaJumperFrames.Count,
            "Norfair lava jumper has eleven distinct installed identities");
        AssertTrue(lavaJumperFrames.Contains(CommonEnemyEmptyExtendedFrameToolingDefinitions.EmptySpritemap),
            "Norfair lava jumper retains its native bank-local empty frame");
        AssertTrue(NorfairLavaJumperAnimationsConstants.Spritemaps(Enum.GetValues<NorfairLavaJumperProgram>()).ToHashSet().SetEquals(lavaJumperFrames),
            "every installed lava-jumper frame is shown by a typed program");
        HashSet<ushort> chozoFrames = ChozoStatueVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(ChozoStatueVisualDefinitions.FrameCount, chozoFrames.Count,
            "the two Chozo sequences select twenty-six distinct installed frames");
        AssertTrue(ChozoStatueAnimationsConstants.Spritemaps(Enum.GetValues<ChozoStatueProgram>()).ToHashSet().SetEquals(chozoFrames),
            "every installed Chozo composition is shown by a typed program");
        HashSet<ushort> violaFrames = ViolaVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(ViolaVisualDefinitions.FrameCount, violaFrames.Count,
            "Viola's fourteen-frame loop selects eight distinct OAM frames");
        AssertTrue(CrawlerAnimationsConstants.Spritemaps(CrawlerProgram.ViolaNormal).ToHashSet().SetEquals(violaFrames),
            "every installed Viola composition is shown by its normal loop");
        HashSet<ushort> rinkaFrames = RinkaVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(RinkaVisualDefinitions.FrameCount, rinkaFrames.Count,
            "both Rinka loops select five distinct OAM frames");
        AssertTrue(RinkaAnimationsConstants.Spritemaps(Enum.GetValues<RinkaProgram>()).ToHashSet().SetEquals(rinkaFrames),
            "every installed Rinka composition is shown by a Rinka program");
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
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
                installed.AddEnemySpritemap(parts, new Position16<SpritePixels>(x, y), palette, baseTile);
                AssertTrue(native.ImageEquals(installed) &&
                           native.NextByteOffset == installed.NextByteOffset,
                    $"installed {frame.Name} OAM matches native at {x:X4},{y:X4}");
            }
            // Bank-$B4 frames belong to the room sprite-object renderer, not an
            // enemy slot whose definition selects an $A0-$B3 spritemap bank.
            if (frame.Bank == EnemySpritemapDefinitions.RoomSpriteObjectBank)
                continue;
            // The head/neck use a private world-space hook, and the falling tubes
            // use Mother Brain's encounter route. Test that route separately below.
            if (frame.Bank == MotherBrainVisualToolingDefinitions.Bank)
                continue;
            if (frame.LegacyName.StartsWith("ridley_supplement_", StringComparison.Ordinal))
                continue;
            // A room draws a frame as the enemy whose behaviour shows it; a frame only an enemy drawing from the characters
            // it uploads shows, as Ridley carrying the Ceres Baby, draws through that enemy's own route instead.
            if (FrameOwner(RoomEnemySlotFrameAccess.FrameAt(frame.Bank, frame.Pointer)) is not { } owner)
                continue;
            RoomEnemySlot? drawn = null;
            RoomEnemySystem? drawnRoom = null;
            int drawnRow = 0;
            int drawnBase = 0;
            var room = DrawEnemy(stock, NewFrameReadGuard(rom, frame.Name), frame.Pointer, owner, inspect: (slot, system) =>
                {
                    drawn = slot;
                    drawnRoom = system;
                    drawnRow = slot.PaletteRowIn(system);
                    drawnBase = slot.NativeTileBaseIn(system);
                },
                spritemapBank: frame.Bank);
            var nativeRoom = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeRoom, new NativeEnemySpritemapDraw
            {
                Bank = frame.Bank, Pointer = frame.Pointer, Origin = new(0x0040, 0x0080),
                PaletteBits = NativeSpriteGraphicsWords.PaletteBits(drawn is null ? throw new InvalidOperationException("Verification requires the drawn slot.") : drawnRow),
                BaseTileIndex = checked((ushort)drawnBase),
            });
            // A picture the sheet keeps once for several identical frames stands for any of their OAM (#1513).
            AssertTrue(room.ImageEquals(nativeRoom) || SpriteFrameBufferAccess.DrawnStandsFor(sprites =>
                {
                    RoomEnemySystem redrawn = drawnRoom ?? throw new InvalidOperationException("Verification requires the drawn room.");
                    redrawn.Drawing.AdvanceLayers(new(0, 0), 0, 0);
                    redrawn.Drawing.DrawLayers(sprites, new(0, 0), 0, 0);
                }, nativeRoom),
                $"production room draws installed {frame.Name} without visual ROM reads");
        }

        Suite(nameof(VerifyInstalledMotherBrainDrawHook), () => VerifyInstalledMotherBrainDrawHook(rom, stock));
        Suite(nameof(VerifyInstalledRidleySupplementalArtwork), () => VerifyInstalledRidleySupplementalArtwork(rom, stock));

        // Ridley's scene adds his tail and wings to his body, so his frames are checked through his scene's own route.
        static EnemyDefinitionId? FrameOwner(EnemyFrameId? frame) => frame is not { } shown ? null
            : EnemyDefinitionFrameDefinitions.Frames.Where(entry => entry.Value.Contains(shown)
                    && entry.Key is not (EnemyDefinitionId.Ridley or EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.RidleyExplosion))
                .Select(entry => (EnemyDefinitionId?)entry.Key).Order().FirstOrDefault();

        static OamBuffer DrawEnemy(InstalledEnemyArtwork art,
            ISnesAddressSpace guard, ushort pointer, EnemyDefinitionId definition,
            Action<RoomEnemySlot, RoomEnemySystem>? inspect = null, byte? spritemapBank = null)
        {
            var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art.Enemies, BossArtwork = art.Bosses };
            // The enemy draws as a room drawing it loads it: at its tile base and in its palette there.
            enemies.GraphicsSet.LoadGraphicsSet(SpawnedObjectFixtures.SetDrawing(definition));
            enemies.Drawing.Enqueue(0, enemies.SlotTable.Slots[0]);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(definition, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.Shading = PictureShading.Standard;
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
                    : definition == EnemyDefinitionId.MiniKraid
                    ? EnemySpritemapDefinitions.FakeKraidBank
                    : definition is EnemyDefinitionId.KraidNail or
                        EnemyDefinitionId.KraidNailBad
                        ? EnemySpritemapDefinitions.KraidNailBank
                    : definition is EnemyDefinitionId.Zeb or
                    EnemyDefinitionId.Zebbo or
                    EnemyDefinitionId.Gamet or
                    EnemyDefinitionId.Geega
                    ? EnemySpritemapDefinitions.PipeBugBank
                    : definition == EnemyDefinitionId.CeresDoor
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
                        EnemyDefinitionId.Mella or EnemyDefinitionId.Menu
                        ? FlyVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.Kago
                        ? KagoVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.FaceBlock
                        ? BlueBrinstarFaceBlockVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.Eye
                        ? MorphBallEyeVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.Metroid
                        ? MetroidVisualDefinitions.Bank
                    : definition == EnemyDefinitionId.Mochtroid
                        ? MochtroidVisualDefinitions.Bank
                    : definition is EnemyDefinitionId.Evir or
                        EnemyDefinitionId.EvirProjectile
                        ? EvirVisualDefinitions.Bank
                    : definition is EnemyDefinitionId.Robot or
                        EnemyDefinitionId.RobotNoPower
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
                        EnemyDefinitionId.Kamer2
                        ? TripperKamerVisualDefinitions.Bank
                    : definition is EnemyDefinitionId.ShutterGrowing or
                        EnemyDefinitionId.ShutterShootable or
                        EnemyDefinitionId.ShutterDestroyable or
                        EnemyDefinitionId.ShutterHorizShootable
                        ? ShutterVisualDefinitions.Bank
                    : definition is EnemyDefinitionId.HZoomer or
                        EnemyDefinitionId.Sciser or
                        EnemyDefinitionId.Zeela or
                        EnemyDefinitionId.Sova or
                        EnemyDefinitionId.Zoomer or
                        EnemyDefinitionId.MZoomer
                        ? EnemySpritemapDefinitions.HZoomerBank
                    : definition is EnemyDefinitionId.Sbug or
                        EnemyDefinitionId.Sbug2
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
            slot.Frame = RoomEnemySlotFrameAccess.FrameAt(frameBank, pointer);
            if (slot.Frame is { } shown)
                SpawnedObjectFixtures.ShowDrawnSteps(enemies, SuperMetroid.AssetExtraction.Enemies.FrameDrawings.Of(shown));
            slot.FrameHold = WrappingCountdown.Start(7);
            slot.Timer = WrappingCountdown.Start(9);
            slot.Position = slot.Position.WithWhole(new(0x0040, 0x0080));
            var oam = new OamBuffer();
            enemies.Drawing.DrawLayersAsOam(oam, new(0, 0), 0, 0);
            inspect?.Invoke(slot, enemies);
            return oam;
        }
    }

    /// <summary>Checks Mother Brain's private draw hook against native head frames and above-screen culling.</summary>
    private static void VerifyInstalledMotherBrainDrawHook(
        SuperMetroidAddressSpace rom, InstalledEnemyArtwork stock)
    {
        EnemyGraphicsSet headSet = MotherBrainHeadSet();
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

        OamBuffer aboveScreen = DrawInstalledMotherBrainFrame(
            stock, 0xa586, new(0x0140, 0x000f));
        AssertEqual(0, aboveScreen.NextByteOffset,
            "Mother Brain private draw still culls a head above the viewport");
    }

    /// <summary>Mother Brain's room's graphics set, which gives the tile base and colours it loads her head with.</summary>
    private static EnemyGraphicsSet MotherBrainHeadSet()
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = InstalledFixtures.EnemyTiles };
        SpawnedObjectFixtures.LoadSetDrawing(enemies, EnemyDefinitionId.MotherBrainHead);
        return enemies.GraphicsSet;
    }

    /// <summary>Draws one Mother Brain world frame through the installed private renderer under a cartridge-read guard.</summary>
    private static OamBuffer DrawInstalledMotherBrainFrame(
        InstalledEnemyArtwork art, ushort pointer,
        Position16<RoomPixels> world)
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art.Enemies, BossArtwork = art.Bosses };
        // She draws from the sheet her room's set composes from this artwork, at the indexes it loads her with.
        SpawnedObjectFixtures.LoadSetDrawing(enemies, EnemyDefinitionId.MotherBrainHead);
        var oam = new OamBuffer();
        EnemyFrameId frame = RoomEnemySlotFrameAccess.FrameAt(MotherBrainVisualToolingDefinitions.Bank, pointer)
            ?? throw new InvalidOperationException("A Mother Brain draw shows a frame.");
        oam.AppendDrawn(sprites => enemies.MotherBrainBehavior.Head.DrawMotherBrainWorldSpritemap(sprites,
            new SpriteSource(SpriteSourceKind.MotherBrainBrain), new MotherBrainWorldFrame(frame, world, PictureShading.Standard),
            new Position16<RoomPixels>(0x0100, 0x0010)));
        return oam;
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
