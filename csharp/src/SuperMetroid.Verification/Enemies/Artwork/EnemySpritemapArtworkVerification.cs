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
using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Alcoon.AlcoonInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Beetom.BeetomInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Botwoon.BotwoonVisualsVerification;
using static SuperMetroid.Verification.Enemies.Corpses.DeadTorizoArtworkVerification;
using static SuperMetroid.Verification.Enemies.Evir.EvirVisualsVerification;
using static SuperMetroid.Verification.Enemies.Crawlers.HZoomerArtworkVerification;
using static SuperMetroid.Verification.Enemies.Hoppers.HopperAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Enemies.Metroids.MochtroidVisualsVerification;
using static SuperMetroid.Verification.Enemies.NativeEnemyVisualSelectorsVerification;
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
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Elevator;
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
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Areas.Ceres.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed enemy OAM compositions against pinned cartridge frames and drawing paths.</summary>
internal static class EnemySpritemapArtworkVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Runs the Alcoon in a real room with the installed artwork.</summary>
    private static void VerifyInstalledAlcoonInstructionFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        Suite(nameof(VerifyAlcoonInstructionProgramDefinitions), () => VerifyAlcoonInstructionProgramDefinitions(rom, stock));
    }

    /// <summary>Runs the Beetom programs with the installed artwork.</summary>
    private static void VerifyInstalledBeetomInstructionFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        Suite(nameof(VerifyBeetomInstructionProgramDefinitions), () => VerifyBeetomInstructionProgramDefinitions(rom, stock));
    }

    /// <summary>Runs the hopper family's typed animations with the installed stock artwork.</summary>
    private static void VerifyInstalledHopperInstructionFrames(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        Suite(nameof(VerifyHopperAnimationDefinitions), () => VerifyHopperAnimationDefinitions(rom, stock));
    }

    /// <summary>Runs Choot's idle, jumping and falling animations with the installed stock artwork.</summary>
    private static void VerifyInstalledChootInstructionFrames(
        EnemyTileArtworkCatalog stock)
    {
        Suite(nameof(ChootBehaviorVerification.VerifyChootBehavior), () => ChootBehaviorVerification.VerifyChootBehavior(stock));
    }

    /// <summary>Checks Bull frame selectors and exercises normal and immune-shot programs without cartridge visual reads.</summary>
    private static void VerifyInstalledBullInstructionFrames(
        EnemyTileArtworkCatalog stock)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock };
        RoomEnemySlot bull = enemies.SlotTable.Slots[0];
        bull.Bind(EnemyDefinitionId.Bull, bull.Definition);
        bull.Bind(bull.EnemyDefinitionPointer, default(RoomEnemyDefinition) with
            { Bank = EnemySpritemapDefinitions.BullBank });
        new Action<RoomEnemySlot>(enemies.BullBehavior.InitializeBull)(bull);
        object?[] arguments =
            [bull, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
        for (int frame = 0; frame < 41; frame++)
            EnemyInstructionRunner.Process(arguments);
        AssertEqual(BullAnimationsConstants.Spritemaps(BullProgram.Normal).First(), bull.SpritemapPointer,
            "installed Bull normal animation loops back to its first frame on time");
        BullEnemyState state = enemies.BullStates[0] ?? throw new InvalidDataException(
            "Bull initializer did not publish typed state.");
        BullBehavior.ResolveBullImmuneShot(bull, state, (ushort)2);
        for (int frame = 0; frame < 61; frame++)
            EnemyInstructionRunner.Process(arguments);
        AssertEqual(BullAnimationsConstants.Spritemaps(BullProgram.Normal).First(), bull.SpritemapPointer,
            "installed Bull immune-shot program returns to the normal loop");
        AssertEqual((ushort)0, bull.Timer.RawValue,
            "installed Bull immune-shot loop exhausts its native repeat timer");
    }

    /// <summary>Runs Puyo's grounded and airborne programs with the installed stock artwork.</summary>
    private static void VerifyInstalledPuyoInstructionFrames(
        EnemyTileArtworkCatalog stock) =>
        Suite(nameof(PuyoInstructionProgramDefinitionsVerification.VerifyPuyoInstructionProgramDefinitions),
            () => PuyoInstructionProgramDefinitionsVerification.VerifyPuyoInstructionProgramDefinitions(stock));

    /// <summary>
    /// The stock catalog from <paramref name="stockDirectory"/> with its compositions replaced, as the installation
    /// loader selects a composition override; nothing is derived from the compositions when the catalog is built.
    /// </summary>
    private static EnemyTileArtworkCatalog StockWithSpritemaps(string stockDirectory, EnemySpritemapCatalog spritemaps)
    {
        EnemyTileArtworkCatalog catalog = EnemyTileArtworkFiles.Load(stockDirectory, null);
        catalog.Spritemaps = Ensure.NotNull(spritemaps);
        return catalog;
    }

    /// <summary>Compares installed enemy compositions and rendering with the cartridge's OAM definitions.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyInstalledEnemySpritemaps(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        AssertTrue(stock.Spritemaps is not null,
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
                BotwoonVisualDefinitions.Frames().Length,
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
                HibashiVisualDefinitions.FrameCount + ZebetiteVisualDefinitions.FrameCount +
                WreckedShipGhostVisualDefinitions.FrameCount + PowampVisualDefinitions.FrameCount +
                SparkVisualDefinitions.FrameCount + ShitroidVisualDefinitions.FrameCount +
                KraidLintVisualDefinitions.FrameCount + NuclearWaffleVisualDefinitions.FrameCount +
                2 + // Single-frame Kzan and Polyp.
                RidleyBreakupVisualDefinitions.Legs.Length + RidleyBreakupVisualDefinitions.Torso.Length +
                RidleyBreakupVisualDefinitions.Head.Length + RidleyBreakupVisualDefinitions.Claw.Length,
            EnemySpritemapDefinitions.Frames.Length,
            "post-Gunship installed composition groups account for the complete current catalog");
        HashSet<ushort> installedHunterPointers = EnemySpritemapDefinitions.Frames
            
            .Where(frame => frame.Name.StartsWith("ki_hunter_a8_", StringComparison.Ordinal))
            .Select(frame => frame.Pointer)
            .ToHashSet();
        AssertTrue(KiHunterAnimations.Spritemaps(Enum.GetValues<KiHunterProgram>()).ToHashSet().SetEquals(
                installedHunterPointers),
            "the Ki-Hunter's programs show exactly its installed frames");
        AssertEqual(41, installedHunterPointers.Count, "KiHunter's programs show 41 distinct OAM frames");
        HashSet<ushort> installedMawPointers = EnemySpritemapDefinitions.Frames
            
            .Where(frame => frame.Name.StartsWith("yapping_maw_a8_", StringComparison.Ordinal))
            .Select(frame => frame.Pointer)
            .ToHashSet();
        AssertTrue(YappingMawAnimations.Spritemaps(Enum.GetValues<YappingMawProgram>()).ToHashSet().SetEquals(installedMawPointers),
            "every installed Yapping Maw frame is shown by a program, and every shown pose is installed");
        AssertEqual(24, installedMawPointers.Count, "Yapping Maw's programs show 24 distinct frames");
        AssertEqual(276, EnemySpritemapDefinitions.Frames.Count(
                frame => frame.Bank == EnemySpritemapDefinitions.RoomSpriteObjectBank),
            "all bank-B4 sprite-object programs select 276 distinct visual frames");
        HashSet<ushort> installedSpritePointers = EnemySpritemapDefinitions.Frames
            
            .Where(frame => frame.Bank == EnemySpritemapDefinitions.RoomSpriteObjectBank)
            .Select(frame => frame.Pointer)
            .ToHashSet();
        AssertTrue(Enum.GetValues<RoomSpriteObjectFrame>().Select(RoomSpriteObjectVisualDefinitions.Spritemap).ToHashSet()
                .SetEquals(SuperMetroid.Verification.Rooms.SpriteObjects.RoomSpriteObjectNativeLists.SpritemapPointers(rom)),
            "the room sprite-object frames are exactly the spritemaps the native lists show");
        foreach (RoomSpriteObjectFrame frame in Enum.GetValues<RoomSpriteObjectFrame>())
            AssertTrue(installedSpritePointers.Contains(RoomSpriteObjectVisualDefinitions.Spritemap(frame)),
                $"room sprite-object frame {frame} has installed art");
        AssertTrue(FuneNamiheAnimationsConstants.Spritemaps(Enum.GetValues<FuneNamiheProgram>()).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.Name.StartsWith("fune_", StringComparison.Ordinal) ||
                    frame.Name.StartsWith("namihe_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Fune's and Namihe's programs show exactly their installed frames");
        AssertTrue(RioAnimationsConstants.Spritemaps(Enum.GetValues<RioProgram>()).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.Name.StartsWith("rio_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Rio's programs show exactly its eight installed frames");
        AssertTrue(LowerNorfairRioAnimationsConstants.Spritemaps(Enum.GetValues<LowerNorfairRioProgram>()).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.Name.StartsWith("lower_norfair_rio_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Lower Norfair Rio's programs show exactly its installed frames");
        AssertTrue(NorfairRioAnimationsConstants.Spritemaps(Enum.GetValues<NorfairRioProgram>()).ToHashSet().SetEquals(
                EnemySpritemapDefinitions.Frames.Where(frame => frame.Name.StartsWith("norfair_rio_", StringComparison.Ordinal)).Select(frame => frame.Pointer)),
            "the Norfair Rio's programs show exactly its installed frames");
        Suite(nameof(VerifyInstalledPuyoInstructionFrames), () => VerifyInstalledPuyoInstructionFrames(stock));
        Suite(nameof(VerifyInstalledBullInstructionFrames), () => VerifyInstalledBullInstructionFrames(stock));
        Suite(nameof(VerifyInstalledAlcoonInstructionFrames), () => VerifyInstalledAlcoonInstructionFrames(rom, stock));
        Suite(nameof(VerifyInstalledMochtroidVisuals), () => VerifyInstalledMochtroidVisuals(rom, stock));
        Suite(nameof(VerifyInstalledEvirVisuals), () => VerifyInstalledEvirVisuals(rom, stock));
        Suite(nameof(VerifyInstalledWorkRobotVisuals), () => VerifyInstalledWorkRobotVisuals(rom, stock));
        Suite(nameof(VerifyInstalledYardVisuals), () => VerifyInstalledYardVisuals(rom, stock));
        Suite(nameof(VerifyInstalledBotwoonVisuals), () => VerifyInstalledBotwoonVisuals(rom, stock));
        Suite(nameof(VerifyInstalledBeetomInstructionFrames), () => VerifyInstalledBeetomInstructionFrames(rom, stock));
        Suite(nameof(VerifyInstalledHopperInstructionFrames), () => VerifyInstalledHopperInstructionFrames(rom, stock));
        Suite(nameof(VerifyInstalledChootInstructionFrames), () => VerifyInstalledChootInstructionFrames(stock));
        Suite(nameof(VerifyInstalledHZoomerInstructionFrames), () => VerifyInstalledHZoomerInstructionFrames(stock));
        Suite(nameof(VerifyInstalledSharedCrawlerFrames), () => VerifyInstalledSharedCrawlerFrames(stock));
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
        AssertTrue(MultiviolaAnimations.Spritemaps(MultiviolaProgram.Flying).ToHashSet().SetEquals(multiviolaFrames),
            "every installed Multiviola composition is shown by its loop");
        HashSet<ushort> lavaJumperFrames = NorfairLavaJumperVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(NorfairLavaJumperVisualDefinitions.FrameCount,
            lavaJumperFrames.Count,
            "Norfair lava jumper has eleven distinct installed identities");
        AssertTrue(lavaJumperFrames.Contains(CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap),
            "Norfair lava jumper retains its native bank-local empty frame");
        AssertTrue(NorfairLavaJumperAnimations.Spritemaps(Enum.GetValues<NorfairLavaJumperProgram>()).ToHashSet().SetEquals(lavaJumperFrames),
            "every installed lava-jumper frame is shown by a typed program");
        HashSet<ushort> chozoFrames = ChozoStatueVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(ChozoStatueVisualDefinitions.FrameCount, chozoFrames.Count,
            "the two Chozo sequences select twenty-six distinct installed frames");
        AssertTrue(ChozoStatueAnimations.Spritemaps(Enum.GetValues<ChozoStatueProgram>()).ToHashSet().SetEquals(chozoFrames),
            "every installed Chozo composition is shown by a typed program");
        HashSet<ushort> violaFrames = ViolaVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(ViolaVisualDefinitions.FrameCount, violaFrames.Count,
            "Viola's fourteen-frame loop selects eight distinct OAM frames");
        AssertTrue(CrawlerAnimations.Spritemaps(CrawlerProgram.ViolaNormal).ToHashSet().SetEquals(violaFrames),
            "every installed Viola composition is shown by its normal loop");
        HashSet<ushort> rinkaFrames = RinkaVisualDefinitions.Frames()
            .Select(frame => frame.Pointer).ToHashSet();
        AssertEqual(RinkaVisualDefinitions.FrameCount, rinkaFrames.Count,
            "both Rinka loops select five distinct OAM frames");
        AssertTrue(RinkaAnimations.Spritemaps(Enum.GetValues<RinkaProgram>()).ToHashSet().SetEquals(rinkaFrames),
            "every installed Rinka composition is shown by a Rinka program");
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        {
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGet(frame.Bank, frame.Pointer, out var parts),
                $"installed enemy frame {frame.Name} exists");
            AssertTrue(stock.Spritemaps.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                $"installed enemy frame {frame.Name} has a display binding");
            foreach ((ushort x, ushort y, ushort palette, ushort baseTile) in
                     new (ushort, ushort, ushort, ushort)[]
                     {
                         (0x0010, 0x0020, 0, 0),
                         (0x01f8, 0x00fc, 0x0c00, 0x01ff),
                         (0x0000, 0x0000, 0x0400, 0x0010),
                     })
            {
                var native = new OamBuffer();
                var installed = new OamBuffer();
                DrawImportedEnemySpritemap(rom, native, new NativeEnemySpritemapDraw { Bank = frame.Bank, Pointer = frame.Pointer, Origin = new(x, y), PaletteBits = palette, BaseTileIndex = baseTile });
                installed.AddEnemySpritemap(parts, new Position16<OamPixels>(x, y), palette, baseTile);
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
            if (frame.Bank == MotherBrainVisualDefinitions.Bank)
                continue;
            if (frame.Name.StartsWith("ridley_supplement_", StringComparison.Ordinal))
                continue;
            var room = DrawEnemy(stock, NewFrameReadGuard(rom, frame.Name), frame.Pointer,
                frame.Name.StartsWith("boyon_", StringComparison.Ordinal)
                    ? EnemyDefinitionId.Boyon
                    : frame.Name.StartsWith("mochtroid_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Mochtroid
                    : frame.Name.StartsWith("evir_projectile_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.EvirProjectile
                    : frame.Name.StartsWith("evir_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Evir
                    : frame.Name.StartsWith("work_robot_unpowered_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.RobotNoPower
                    : frame.Name.StartsWith("work_robot_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Robot
                    : frame.Name.StartsWith("yard_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Yard
                    : frame.Name.StartsWith("botwoon_head_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Botwoon
                    : frame.Name.StartsWith("sciser_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Sciser
                    : frame.Name.StartsWith("fly_shared_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Mellow
                    : frame.Name.StartsWith("kago_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Kago
                    : frame.Name.StartsWith("face_block_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.FaceBlock
                    : frame.Name.StartsWith("morph_eye_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Eye
                    : frame.Name.StartsWith("metroid_body_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Metroid
                    : frame.Name.StartsWith("shaktool_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Shaktool
                    : frame.Name.StartsWith("tripper_kamer_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Kamer2
                    : frame.Name.StartsWith("tripper_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Tripper
                    : frame.Name.StartsWith("dragon_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Dragon
                    : frame.Name.StartsWith("multiviola_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Multiviola
                    : frame.Name.StartsWith("norfair_lava_jumper_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Squeept
                    : frame.Name.StartsWith("chozo_statue_aa_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Chozo
                    : frame.Name.StartsWith("viola_spin_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Viola
                    : frame.Name.StartsWith("rinka_spin_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Rinka
                    : frame.Name.StartsWith("shutter_growing_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.ShutterGrowing
                    : frame.Name.StartsWith("shutter_vertical_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.ShutterShootable
                    : frame.Name.StartsWith("shutter_horizontal", StringComparison.Ordinal)
                        ? EnemyDefinitionId.ShutterHorizShootable
                    : frame.Name.StartsWith("norfair_rio_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Geruta
                    : frame.Name.StartsWith("lower_norfair_rio_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Holtz
                    : frame.Name.StartsWith("rio_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Rio
                    : frame.Name.StartsWith("puyo_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Puyo
                    : frame.Name.StartsWith("bull_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Bull
                    : frame.Name.StartsWith("alcoon_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Alcoon
                    : frame.Name.StartsWith("beetom_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Beetom
                    : frame.Name.StartsWith("large_sidehopper_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.SidehopperLarge
                    : frame.Name.StartsWith("sidehopper_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Sidehopper
                    : frame.Name.StartsWith("large_dessgeega_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.DessgeegaLarge
                    : frame.Name.StartsWith("dessgeega_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Dessgeega
                    : frame.Name.StartsWith("choot_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Choot
                    : frame.Name.StartsWith("hzoomer_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.HZoomer
                    : frame.Name.StartsWith("sbug_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Sbug
                    : frame.Name.StartsWith("fune_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Fune
                    : frame.Name.StartsWith("namihe_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Namihe
                    : frame.Name.StartsWith("kamer_platform_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Kamer
                    : frame.Name.StartsWith("elevator_platform_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Elevator
                    : frame.Name.StartsWith("cacatac_", StringComparison.Ordinal)
                        ? EnemyDefinitionId.Cacatac
                        : frame.Name.StartsWith("boulder_", StringComparison.Ordinal)
                            ? EnemyDefinitionId.Boulder
                            : frame.Name.StartsWith("skultera_", StringComparison.Ordinal)
                                ? EnemyDefinitionId.Skultera
                                : frame.Name.StartsWith("waver_", StringComparison.Ordinal)
                                    ? EnemyDefinitionId.Waver
                                    : frame.Name.StartsWith("zoa_", StringComparison.Ordinal)
                                        ? EnemyDefinitionId.Zoa
                                        : frame.Name.StartsWith("metaree_", StringComparison.Ordinal)
                                            ? EnemyDefinitionId.Metaree
                                            : frame.Name.StartsWith("skree_", StringComparison.Ordinal)
                                                ? EnemyDefinitionId.Skree
                                                : frame.Name.StartsWith("pipe_brinstar_strong_", StringComparison.Ordinal)
                                                    ? EnemyDefinitionId.Zebbo
                                                    : frame.Name.StartsWith("pipe_brinstar_", StringComparison.Ordinal)
                                                        ? EnemyDefinitionId.Zeb
                                                        : frame.Name.StartsWith("pipe_norfair_", StringComparison.Ordinal)
                                                            ? EnemyDefinitionId.Gamet
                                                            : frame.Name.StartsWith("pipe_yellow_", StringComparison.Ordinal)
                                                                ? EnemyDefinitionId.Geega
                                                                : frame.Name.StartsWith("fake_kraid_", StringComparison.Ordinal)
                                                                    ? EnemyDefinitionId.MiniKraid
                                                                : frame.Name.StartsWith("kraid_nail_", StringComparison.Ordinal)
                                                                        ? EnemyDefinitionId.KraidNail
                                                                        : frame.Name.StartsWith("owtch_", StringComparison.Ordinal)
                                                                            ? EnemyDefinitionId.Owtch
                                                                            : frame.Name.StartsWith("stoke_", StringComparison.Ordinal)
                                                                                ? EnemyDefinitionId.Stoke
                                                                                : frame.Name.StartsWith("ripper_shared_", StringComparison.Ordinal)
                                                                                    ? EnemyDefinitionId.GRipper
                                                                                    : frame.Name.StartsWith("ripper_", StringComparison.Ordinal)
                                                                                        ? EnemyDefinitionId.Ripper
                                                                                        : frame.Name.StartsWith("fireflea_", StringComparison.Ordinal)
                                                                                            ? EnemyDefinitionId.Fireflea
                                                                : frame.Name.StartsWith("ceres_door_", StringComparison.Ordinal) ||
                                                                  frame.Name.StartsWith("ceres_baby_", StringComparison.Ordinal)
                                                                    ? EnemyDefinitionId.CeresDoor
                                                                : frame.Name.StartsWith("magdollite_", StringComparison.Ordinal)
                                                                                                ? EnemyDefinitionId.Magdollite
                                : EnemyDefinitionId.Atomic, spritemapBank: frame.Bank);
            var nativeRoom = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeRoom, new NativeEnemySpritemapDraw { Bank = frame.Bank, Pointer = frame.Pointer, Origin = new(0x0040, 0x0080), PaletteBits = 0, BaseTileIndex = 0 });
            AssertTrue(room.ImageEquals(nativeRoom),
                $"production room draws installed {frame.Name} without visual ROM reads");
        }

        Suite(nameof(VerifyInstalledMotherBrainDrawHook), () => VerifyInstalledMotherBrainDrawHook(rom, stock));
        Suite(nameof(VerifyInstalledRidleySupplementalArtwork), () => VerifyInstalledRidleySupplementalArtwork(rom, stock));

        string fileName = EnemySpritemapDefinitions.FileName;
        string stockPath = Path.Combine(stockDirectory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "stock enemy compositions are manifest-hash checked");
        File.WriteAllBytes(stockPath, original);

        EnemySpritemapDocument document = RequiredJson.Deserialize<EnemySpritemapDocument>(
            original, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        // Construct the historical version boundary, not today's catalog minus
        // a few known families. Every older fixture below derives from this set.
        HashSet<string> preYardNames = EnemySpritemapDefinitions.Frames
            .Take(EnemySpritemapDefinitions.PreYardFrameCount)
            .Select(frame => frame.Name).ToHashSet(StringComparer.Ordinal);
        var preYardFrames = document.Frames
            .Where(pair => preYardNames.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preYardBindings = (document.DisplayFrames ?? throw new InvalidOperationException("Verification requires document.DisplayFrames."))
            .Where(pair => preYardFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreYardFrameCount,
            preYardFrames.Count, "version-fifty-eight composition schema count");
        using (var preYardJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreYard,
                Frames = preYardFrames,
                DisplayFrames = preYardBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog yardUpgraded = EnemySpritemapCatalog.Load(
                preYardJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in YardVisualDefinitions.Frames())
                AssertTrue(yardUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty-eight override inherits {frame.Name}");
        }
        HashSet<string> robotNames = WorkRobotVisualDefinitions.Frames()
            .Select(frame => frame.Name).ToHashSet(StringComparer.Ordinal);
        var preRobotFrames = preYardFrames
            .Where(pair => !robotNames.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preRobotBindings = preYardBindings
            .Where(pair => preRobotFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreWorkRobotFrameCount,
            preRobotFrames.Count, "version-fifty-seven composition schema count");
        using (var preRobotJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreWorkRobot,
                Frames = preRobotFrames,
                DisplayFrames = preRobotBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog robotUpgraded = EnemySpritemapCatalog.Load(
                preRobotJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in WorkRobotVisualDefinitions.Frames())
                AssertTrue(robotUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty-seven override inherits {frame.Name}");
        }
        HashSet<string> evirNames = EvirVisualDefinitions.Frames()
            .Select(frame => frame.Name).ToHashSet(StringComparer.Ordinal);
        var preEvirFrames = preRobotFrames
            .Where(pair => !evirNames.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preEvirBindings = preRobotBindings
            .Where(pair => preEvirFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreEvirFrameCount,
            preEvirFrames.Count, "version-fifty-six composition schema count");
        using (var preEvirJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreEvir,
                Frames = preEvirFrames,
                DisplayFrames = preEvirBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog evirUpgraded = EnemySpritemapCatalog.Load(
                preEvirJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in EvirVisualDefinitions.Frames())
                AssertTrue(evirUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty-six override inherits {frame.Name}");
        }
        HashSet<string> mochtroidNames = MochtroidVisualDefinitions.Frames()
            .Select(frame => frame.Name).ToHashSet(StringComparer.Ordinal);
        var preMochtroidFrames = preEvirFrames
            .Where(pair => !mochtroidNames.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preMochtroidBindings = preEvirBindings
            .Where(pair => preMochtroidFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMochtroidFrameCount,
            preMochtroidFrames.Count, "version-fifty-five composition schema count");
        using (var preMochtroidJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreMochtroid,
                Frames = preMochtroidFrames,
                DisplayFrames = preMochtroidBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog mochtroidUpgraded = EnemySpritemapCatalog.Load(
                preMochtroidJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in MochtroidVisualDefinitions.Frames())
                AssertTrue(mochtroidUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty-five override inherits {frame.Name}");
        }
        HashSet<string> deadCorpseNames = DeadTourianCorpseVisualDefinitions.Frames()
            .Select(frame => frame.Name).ToHashSet(StringComparer.Ordinal);
        var preCorpsesFrames = preMochtroidFrames
            .Where(pair => !deadCorpseNames.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preCorpsesBindings = preMochtroidBindings
            .Where(pair => preCorpsesFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreDeadTourianCorpseFrameCount,
            preCorpsesFrames.Count, "version-fifty-four composition schema count");
        using (var preCorpsesJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreDeadTourianCorpse,
                Frames = preCorpsesFrames,
                DisplayFrames = preCorpsesBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog corpsesUpgraded = EnemySpritemapCatalog.Load(
                preCorpsesJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in
                     DeadTourianCorpseVisualDefinitions.Frames())
                AssertTrue(corpsesUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty-four override inherits {frame.Name}");
        }
        var preStationaryFrames = preCorpsesFrames
            .Where(pair => pair.Key != "dead_torizo_stationary_a9_d6e2")
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preStationaryBindings = preCorpsesBindings
            .Where(pair => preStationaryFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreDeadTorizoStationaryFrameCount,
            preStationaryFrames.Count, "version-fifty-three composition schema count");
        using (var preDeadTorizoJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreDeadTorizoStationary,
                Frames = preStationaryFrames,
                DisplayFrames = preStationaryBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog stationaryUpgraded = EnemySpritemapCatalog.Load(
                preDeadTorizoJson, stock.Spritemaps);
            AssertTrue(stationaryUpgraded.TryGetDisplay(
                    DeadTorizoArtworkDefinitions.SpritemapBank,
                    DeadTorizoArtworkDefinitions.StationarySpritemap, out _),
                "version-fifty-three override inherits Dead Torizo stationary OAM");
        }
        var preRinkaFrames = preStationaryFrames
            .Where(pair => !pair.Key.StartsWith("rinka_spin_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preRinkaBindings = preStationaryBindings
            .Where(pair => preRinkaFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreRinkaFrameCount, preRinkaFrames.Count,
            "version-fifty-two composition schema count");
        using (var preRinkaJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreRinka,
                Frames = preRinkaFrames,
                DisplayFrames = preRinkaBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog rinkaUpgraded = EnemySpritemapCatalog.Load(
                preRinkaJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in RinkaVisualDefinitions.Frames())
                AssertTrue(rinkaUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty-two override inherits Rinka frame {frame.Name}");
        }
        var preViolaFrames = preRinkaFrames
            .Where(pair => !pair.Key.StartsWith("viola_spin_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preViolaBindings = document.DisplayFrames
            .Where(pair => preViolaFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreViolaFrameCount, preViolaFrames.Count,
            "version-fifty-one composition schema count");
        using (var preViolaJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreViola,
                Frames = preViolaFrames,
                DisplayFrames = preViolaBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog violaUpgraded = EnemySpritemapCatalog.Load(
                preViolaJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in ViolaVisualDefinitions.Frames())
                AssertTrue(violaUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty-one override inherits Viola frame {frame.Name}");
        }
        var preChozoStatueFrames = preViolaFrames
            .Where(pair => !pair.Key.StartsWith("chozo_statue_aa_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preChozoStatueBindings = document.DisplayFrames
            .Where(pair => preChozoStatueFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreChozoStatueFrameCount,
            preChozoStatueFrames.Count,
            "version-fifty composition schema count");
        using (var preChozoStatueJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreChozoStatue,
                Frames = preChozoStatueFrames,
                DisplayFrames = preChozoStatueBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog chozoUpgraded = EnemySpritemapCatalog.Load(
                preChozoStatueJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in ChozoStatueVisualDefinitions.Frames())
                AssertTrue(chozoUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-fifty override inherits Chozo frame {frame.Name}");
        }
        var preNorfairLavaJumperFrames = preChozoStatueFrames
            .Where(pair => !pair.Key.StartsWith("norfair_lava_jumper_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preNorfairLavaJumperBindings = document.DisplayFrames
            .Where(pair => preNorfairLavaJumperFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreNorfairLavaJumperFrameCount,
            preNorfairLavaJumperFrames.Count,
            "version-forty-nine composition schema count");
        using (var preNorfairLavaJumperJson = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreNorfairLavaJumper,
                Frames = preNorfairLavaJumperFrames,
                DisplayFrames = preNorfairLavaJumperBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog lavaJumperUpgraded = EnemySpritemapCatalog.Load(
                preNorfairLavaJumperJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in NorfairLavaJumperVisualDefinitions.Frames())
                AssertTrue(lavaJumperUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-forty-nine override inherits lava-jumper frame {frame.Name}");
        }
        var preMultiviolaFrames = preNorfairLavaJumperFrames
            .Where(pair => !pair.Key.StartsWith("multiviola_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preMultiviolaBindings = document.DisplayFrames
            .Where(pair => preMultiviolaFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMultiviolaFrameCount,
            preMultiviolaFrames.Count, "version-forty-eight composition schema count");
        using (var preMultiviolaJson = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreMultiviola,
                Frames = preMultiviolaFrames,
                DisplayFrames = preMultiviolaBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog multiviolaUpgraded = EnemySpritemapCatalog.Load(
                preMultiviolaJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in MultiviolaVisualDefinitions.Frames())
                AssertTrue(multiviolaUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-forty-eight override inherits Multiviola frame {frame.Name}");
        }
        var preDragonFrames = preMultiviolaFrames
            .Where(pair => !pair.Key.StartsWith("dragon_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preDragonBindings = document.DisplayFrames
            .Where(pair => preDragonFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreDragonFrameCount,
            preDragonFrames.Count, "version-forty-seven composition schema count");
        using (var preDragonJson = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreDragon,
                Frames = preDragonFrames,
                DisplayFrames = preDragonBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog dragonUpgraded = EnemySpritemapCatalog.Load(
                preDragonJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in DragonVisualDefinitions.Frames())
                AssertTrue(dragonUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-forty-seven override inherits Dragon frame {frame.Name}");
        }
        var preTripperKamerFrames = preDragonFrames
            .Where(pair => !pair.Key.StartsWith("tripper_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preTripperKamerBindings = document.DisplayFrames
            .Where(pair => preTripperKamerFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreTripperKamerFrameCount,
            preTripperKamerFrames.Count, "version-forty-six composition schema count");
        using (var preTripperKamerJson = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreTripperKamer,
                Frames = preTripperKamerFrames,
                DisplayFrames = preTripperKamerBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog platformUpgraded = EnemySpritemapCatalog.Load(
                preTripperKamerJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in TripperKamerVisualDefinitions.Frames())
                AssertTrue(platformUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-forty-six override inherits platform frame {frame.Name}");
        }
        var preShaktoolFrames = preDragonFrames
            .Where(pair => !pair.Key.StartsWith("shaktool_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("tripper_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var preShaktoolBindings = document.DisplayFrames
            .Where(pair => preShaktoolFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreShaktoolFrameCount,
            preShaktoolFrames.Count, "version-forty-five composition schema count");
        using (var preShaktoolJson = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreShaktool,
                Frames = preShaktoolFrames,
                DisplayFrames = preShaktoolBindings,
            }, CamelCaseJson)))
        {
            EnemySpritemapCatalog shaktoolUpgraded = EnemySpritemapCatalog.Load(
                preShaktoolJson, stock.Spritemaps);
            foreach (EnemySpritemapDefinition frame in ShaktoolVisualDefinitions.Frames())
                AssertTrue(shaktoolUpgraded.TryGetDisplay(frame.Bank, frame.Pointer, out _),
                    $"version-forty-five override inherits Shaktool frame {frame.Name}");
        }
        SpriteVisualPart originalPart = document.Frames["boyon_idle_0"][0];
        document.Frames["boyon_idle_0"][0] = originalPart with
        {
            OffsetX = originalPart.OffsetX + 1,
            TileColumn = originalPart.TileColumn + 1,
        };
        SpriteVisualPart cacatacPart = document.Frames["cacatac_upright_idle_0"][0];
        document.Frames["cacatac_upright_idle_0"][0] = cacatacPart with
        {
            OffsetY = cacatacPart.OffsetY + 1,
        };
        SpriteVisualPart boulderPart = document.Frames["boulder_roll_0"][0];
        document.Frames["boulder_roll_0"][0] = boulderPart with
        {
            OffsetY = boulderPart.OffsetY + 1,
        };
        SpriteVisualPart atomicPart = document.Frames["atomic_up_right_0"][0];
        document.Frames["atomic_up_right_0"][0] = atomicPart with
        {
            OffsetY = atomicPart.OffsetY + 1,
        };
        SpriteVisualPart skulteraPart = document.Frames["skultera_swim_left_0"][0];
        document.Frames["skultera_swim_left_0"][0] = skulteraPart with
        {
            OffsetY = skulteraPart.OffsetY + 1,
        };
        SpriteVisualPart waverPart = document.Frames["waver_steady_left"][0];
        document.Frames["waver_steady_left"][0] = waverPart with
        {
            OffsetY = waverPart.OffsetY + 1,
        };
        foreach (string frameName in new[]
                 { "zoa_shoot_left_0", "metaree_idle_0", "skree_idle_0" })
        {
            SpriteVisualPart part = document.Frames[frameName][0];
            document.Frames[frameName][0] = part with { OffsetY = part.OffsetY + 1 };
        }
        foreach (string frameName in new[]
                 {
                     "pipe_brinstar_normal_left_0", "pipe_brinstar_strong_rise_left_0",
                     "pipe_norfair_left_0", "pipe_yellow_fly_left_0",
                 })
        {
            SpriteVisualPart part = document.Frames[frameName][0];
            document.Frames[frameName][0] = part with { OffsetY = part.OffsetY + 1 };
        }
        foreach (string frameName in new[]
                 { "fake_kraid_walk_left_0", "kraid_nail_0",
                   "owtch_left_0", "stoke_walk_left_0" })
        {
            SpriteVisualPart part = document.Frames[frameName][0];
            document.Frames[frameName][0] = part with { OffsetY = part.OffsetY + 1 };
        }
        foreach (string frameName in new[]
                 { "ripper_shared_left_0", "ripper_shared_frozen_left",
                   "ripper_right_0" })
        {
            SpriteVisualPart part = document.Frames[frameName][0];
            document.Frames[frameName][0] = part with { OffsetY = part.OffsetY + 1 };
        }
        SpriteVisualPart firefleaPart = document.Frames["fireflea_cycle_0"][0];
        document.Frames["fireflea_cycle_0"][0] = firefleaPart with
        {
            OffsetY = firefleaPart.OffsetY + 1,
        };
        SpriteVisualPart magdollitePart = document.Frames["magdollite_left_idle_0"][0];
        document.Frames["magdollite_left_idle_0"][0] = magdollitePart with
        {
            OffsetY = magdollitePart.OffsetY + 1,
        };
        SpriteVisualPart ceresDoorPart = document.Frames["ceres_door_right_hold"][0];
        document.Frames["ceres_door_right_hold"][0] = ceresDoorPart with
        {
            OffsetY = ceresDoorPart.OffsetY + 1,
        };
        SpriteVisualPart ceresBabyPart = document.Frames["ceres_baby_round"][0];
        document.Frames["ceres_baby_round"][0] = ceresBabyPart with
        {
            OffsetY = ceresBabyPart.OffsetY + 1,
        };
        SpriteVisualPart rioPart = document.Frames["rio_bd6c"][0];
        document.Frames["rio_bd6c"][0] = rioPart with
        {
            OffsetY = rioPart.OffsetY + 1,
        };
        SpriteVisualPart lowerRioPart = document.Frames["lower_norfair_rio_c8bd"][0];
        document.Frames["lower_norfair_rio_c8bd"][0] = lowerRioPart with
        {
            OffsetY = lowerRioPart.OffsetY + 1,
        };
        SpriteVisualPart norfairRioPart = document.Frames["norfair_rio_c442"][0];
        document.Frames["norfair_rio_c442"][0] = norfairRioPart with
        {
            OffsetY = norfairRioPart.OffsetY + 1,
        };
        SpriteVisualPart puyoPart = document.Frames["puyo_ground_0"][0];
        document.Frames["puyo_ground_0"][0] = puyoPart with
        {
            OffsetY = puyoPart.OffsetY + 1,
        };
        SpriteVisualPart bullPart = document.Frames["bull_idle_0"][0];
        document.Frames["bull_idle_0"][0] = bullPart with
        {
            OffsetY = bullPart.OffsetY + 1,
        };
        SpriteVisualPart alcoonPart = document.Frames["alcoon_left_walk_0"][0];
        document.Frames["alcoon_left_walk_0"][0] = alcoonPart with
        {
            OffsetY = alcoonPart.OffsetY + 1,
        };
        SpriteVisualPart beetomPart = document.Frames["beetom_left_crawl_0"][0];
        document.Frames["beetom_left_crawl_0"][0] = beetomPart with
        {
            OffsetY = beetomPart.OffsetY + 1,
        };
        SpriteVisualPart hopperPart = document.Frames["sidehopper_jump_floor"][0];
        document.Frames["sidehopper_jump_floor"][0] = hopperPart with
        {
            OffsetY = hopperPart.OffsetY + 1,
        };
        SpriteVisualPart chootPart = document.Frames["choot_idle"][0];
        document.Frames["choot_idle"][0] = chootPart with
        {
            OffsetY = chootPart.OffsetY + 1,
        };
        SpriteVisualPart hzoomerPart = document.Frames["hzoomer_upside_right_0"][0];
        document.Frames["hzoomer_upside_right_0"][0] = hzoomerPart with
        {
            OffsetY = hzoomerPart.OffsetY + 1,
        };
        SpriteVisualPart sbugPart = document.Frames["sbug_right_0"][0];
        document.Frames["sbug_right_0"][0] = sbugPart with
        {
            OffsetY = sbugPart.OffsetY + 1,
        };
        SpriteVisualPart funePart = document.Frames["fune_right_idle"][0];
        document.Frames["fune_right_idle"][0] = funePart with
        {
            OffsetY = funePart.OffsetY + 1,
        };
        SpriteVisualPart kamerPart = document.Frames["kamer_platform_0"][0];
        document.Frames["kamer_platform_0"][0] = kamerPart with
        {
            OffsetY = kamerPart.OffsetY + 1,
        };
        SpriteVisualPart elevatorPart = document.Frames["elevator_platform_0"][0];
        document.Frames["elevator_platform_0"][0] = elevatorPart with
        {
            OffsetY = elevatorPart.OffsetY + 1,
        };
        SpriteVisualPart draygonIntroPart = document.Frames["draygon_intro_evir_0"][0];
        document.Frames["draygon_intro_evir_0"][0] = draygonIntroPart with
        {
            OffsetX = draygonIntroPart.OffsetX + 1,
        };
        SpriteVisualPart sharedSpritePart = document.Frames["room_sprite_b4_dc3f"][0];
        document.Frames["room_sprite_b4_dc3f"][0] = sharedSpritePart with
        {
            OffsetY = sharedSpritePart.OffsetY + 1,
        };
        ushort mawIdlePointer = YappingMawVisualDefinitionsConstants.IdleFrame;
        string mawIdleName = $"yapping_maw_a8_{mawIdlePointer:x4}";
        SpriteVisualPart mawPart = document.Frames[mawIdleName][0];
        document.Frames[mawIdleName][0] = mawPart with
        {
            OffsetY = mawPart.OffsetY + 1,
        };
        ushort hunterIdlePointer = KiHunterVisualDefinitionsConstants.IdleFrame;
        string hunterIdleName = $"ki_hunter_a8_{hunterIdlePointer:x4}";
        SpriteVisualPart hunterPart = document.Frames[hunterIdleName][0];
        document.Frames[hunterIdleName][0] = hunterPart with
        {
            OffsetY = hunterPart.OffsetY + 1,
        };
        const string motherBrainHeadName = "mother_brain_a9_a586";
        SpriteVisualPart motherBrainHeadPart = document.Frames[motherBrainHeadName][0];
        document.Frames[motherBrainHeadName][0] = motherBrainHeadPart with
        {
            OffsetX = motherBrainHeadPart.OffsetX + 1,
        };
        const string deadTorizoName = "dead_torizo_corpse_a9_d761";
        SpriteVisualPart deadTorizoPart = document.Frames[deadTorizoName][0];
        document.Frames[deadTorizoName][0] = deadTorizoPart with
        {
            OffsetX = deadTorizoPart.OffsetX + 1,
        };
        const string ridleyWingName = "ridley_supplement_a6_dd4a";
        SpriteVisualPart ridleyWingPart = document.Frames[ridleyWingName][0];
        document.Frames[ridleyWingName][0] = ridleyWingPart with
        {
            OffsetX = ridleyWingPart.OffsetX + 1,
        };
        const string sciserFrameName = "sciser_upside_up_0";
        SpriteVisualPart sciserPart = document.Frames[sciserFrameName][0];
        document.Frames[sciserFrameName][0] = sciserPart with
        {
            OffsetX = sciserPart.OffsetX + 1,
        };
        const string flyFrameName = "fly_shared_0";
        SpriteVisualPart flyPart = document.Frames[flyFrameName][0];
        document.Frames[flyFrameName][0] = flyPart with
        {
            OffsetX = flyPart.OffsetX + 1,
        };
        const string kagoFrameName = "kago_cycle_0";
        SpriteVisualPart kagoPart = document.Frames[kagoFrameName][0];
        document.Frames[kagoFrameName][0] = kagoPart with
        {
            OffsetX = kagoPart.OffsetX + 1,
        };
        const string faceBlockFrameName = "face_block_neutral";
        SpriteVisualPart faceBlockPart = document.Frames[faceBlockFrameName][0];
        document.Frames[faceBlockFrameName][0] = faceBlockPart with
        {
            OffsetX = faceBlockPart.OffsetX + 1,
        };
        const string morphEyeFrameName = "morph_eye_mount_left";
        SpriteVisualPart morphEyePart = document.Frames[morphEyeFrameName][0];
        document.Frames[morphEyeFrameName][0] = morphEyePart with
        {
            OffsetX = morphEyePart.OffsetX + 1,
        };
        const string shutterFrameName = "shutter_vertical_40px";
        SpriteVisualPart shutterPart = document.Frames[shutterFrameName][0];
        document.Frames[shutterFrameName][0] = shutterPart with
        {
            OffsetX = shutterPart.OffsetX + 1,
        };
        const string metroidFrameName = "metroid_body_0";
        SpriteVisualPart metroidPart = document.Frames[metroidFrameName][0];
        document.Frames[metroidFrameName][0] = metroidPart with
        {
            OffsetX = metroidPart.OffsetX + 1,
        };
        const string dragonFrameName = "dragon_body_idle_left";
        SpriteVisualPart dragonPart = document.Frames[dragonFrameName][0];
        document.Frames[dragonFrameName][0] = dragonPart with
        {
            OffsetX = dragonPart.OffsetX + 1,
        };
        const string multiviolaFrameName = "multiviola_spin_0";
        SpriteVisualPart multiviolaPart = document.Frames[multiviolaFrameName][0];
        document.Frames[multiviolaFrameName][0] = multiviolaPart with
        {
            OffsetX = multiviolaPart.OffsetX + 1,
        };
        const string lavaJumperFrameName = "norfair_lava_jumper_c02c";
        SpriteVisualPart lavaJumperPart = document.Frames[lavaJumperFrameName][0];
        document.Frames[lavaJumperFrameName][0] = lavaJumperPart with
        {
            OffsetX = lavaJumperPart.OffsetX + 1,
        };
        const string chozoFrameName = "chozo_statue_aa_efd8";
        SpriteVisualPart chozoPart = document.Frames[chozoFrameName][0];
        document.Frames[chozoFrameName][0] = chozoPart with
        {
            OffsetX = chozoPart.OffsetX + 1,
        };
        const string violaFrameName = "viola_spin_b689";
        SpriteVisualPart violaPart = document.Frames[violaFrameName][0];
        document.Frames[violaFrameName][0] = violaPart with
        {
            OffsetX = violaPart.OffsetX + 1,
        };
        const string rinkaFrameName = "rinka_spin_ba38";
        SpriteVisualPart rinkaPart = document.Frames[rinkaFrameName][0];
        document.Frames[rinkaFrameName][0] = rinkaPart with
        {
            OffsetX = rinkaPart.OffsetX + 1,
        };
        string overrideDirectory = Path.Combine(stockDirectory, "spritemap-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        // Override cases check the spritemap catalog against stock loaded once, exactly as the
        // installation loader does for this file; two full loads above prove the disk wiring.
        EnemySpritemapCatalog LoadSpritemapOverride()
        {
            using FileStream json = File.OpenRead(overridePath);
            return EnemySpritemapCatalog.Load(json, stock.Spritemaps);
        }
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(document,
            CamelCaseJson));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        OamBuffer stockDragon = DrawEnemy(stock, NewFrameReadGuard(rom),
            DragonAnimationsConstants.Spritemaps(DragonProgram.IdleFacingLeft).Single(),
            EnemyDefinitionId.Dragon);
        OamBuffer editedDragon = DrawEnemy(edited, NewFrameReadGuard(rom),
            DragonAnimationsConstants.Spritemaps(DragonProgram.IdleFacingLeft).Single(),
            EnemyDefinitionId.Dragon);
        AssertEqual(unchecked((byte)(stockDragon.LowTable[0] + 1)),
            editedDragon.LowTable[0],
            "Dragon authored body offset changes installed room OAM");
        OamBuffer stockMultiviola = DrawEnemy(stock, NewFrameReadGuard(rom),
            MultiviolaVisualDefinitions.Frames()[0].Pointer,
            EnemyDefinitionId.Multiviola);
        OamBuffer editedMultiviola = DrawEnemy(edited, NewFrameReadGuard(rom),
            MultiviolaVisualDefinitions.Frames()[0].Pointer,
            EnemyDefinitionId.Multiviola);
        AssertEqual(unchecked((byte)(stockMultiviola.LowTable[0] + 1)),
            editedMultiviola.LowTable[0],
            "Multiviola authored body offset changes installed room OAM");
        OamBuffer stockLavaJumper = DrawEnemy(stock, NewFrameReadGuard(rom),
            NorfairLavaJumperAnimations.Spritemaps(NorfairLavaJumperProgram.Hidden).First(),
            EnemyDefinitionId.Squeept);
        OamBuffer editedLavaJumper = DrawEnemy(edited, NewFrameReadGuard(rom),
            NorfairLavaJumperAnimations.Spritemaps(NorfairLavaJumperProgram.Hidden).First(),
            EnemyDefinitionId.Squeept);
        AssertEqual(unchecked((byte)(stockLavaJumper.LowTable[0] + 1)),
            editedLavaJumper.LowTable[0],
            "lava-jumper authored OAM offset changes installed room presentation");
        OamBuffer stockChozo = DrawEnemy(stock, NewFrameReadGuard(rom),
            ChozoStatueAnimations.Spritemaps(ChozoStatueProgram.LowerNorfairInitial).First(),
            EnemyDefinitionId.Chozo);
        OamBuffer editedChozo = DrawEnemy(edited, NewFrameReadGuard(rom),
            ChozoStatueAnimations.Spritemaps(ChozoStatueProgram.LowerNorfairInitial).First(),
            EnemyDefinitionId.Chozo);
        AssertEqual(unchecked((byte)(stockChozo.LowTable[0] + 1)),
            editedChozo.LowTable[0],
            "Chozo authored body offset changes installed room OAM");
        OamBuffer stockViola = DrawEnemy(stock, NewFrameReadGuard(rom),
            CrawlerAnimations.Spritemaps(CrawlerProgram.ViolaNormal).First(),
            EnemyDefinitionId.Viola);
        OamBuffer editedViola = DrawEnemy(edited, NewFrameReadGuard(rom),
            CrawlerAnimations.Spritemaps(CrawlerProgram.ViolaNormal).First(),
            EnemyDefinitionId.Viola);
        AssertEqual(unchecked((byte)(stockViola.LowTable[0] + 1)),
            editedViola.LowTable[0],
            "Viola authored body offset changes installed room OAM");
        OamBuffer stockRinka = DrawEnemy(stock, NewFrameReadGuard(rom),
            RinkaAnimations.Spritemaps(RinkaProgram.Ordinary).First(),
            EnemyDefinitionId.Rinka);
        OamBuffer editedRinka = DrawEnemy(edited, NewFrameReadGuard(rom),
            RinkaAnimations.Spritemaps(RinkaProgram.Ordinary).First(),
            EnemyDefinitionId.Rinka);
        AssertEqual(unchecked((byte)(stockRinka.LowTable[0] + 1)),
            editedRinka.LowTable[0],
            "Rinka authored body offset changes installed room OAM");
        OamBuffer stockSciser = DrawEnemy(stock, NewFrameReadGuard(rom),
            CrawlerAnimations.Spritemaps(CrawlerProgram.SciserUpsideUpLoop).First(),
            EnemyDefinitionId.Sciser);
        OamBuffer editedSciser = DrawEnemy(edited, NewFrameReadGuard(rom),
            CrawlerAnimations.Spritemaps(CrawlerProgram.SciserUpsideUpLoop).First(),
            EnemyDefinitionId.Sciser);
        AssertEqual(unchecked((byte)(stockSciser.LowTable[0] + 1)),
            editedSciser.LowTable[0],
            "Sciser authored OAM offset changes installed room presentation");
        foreach (EnemyDefinitionId definition in new EnemyDefinitionId[]
                 { EnemyDefinitionId.Mellow, EnemyDefinitionId.Mella,
                   EnemyDefinitionId.Menu })
        {
            OamBuffer stockFly = DrawEnemy(stock, NewFrameReadGuard(rom),
                FlyAnimationsConstants.Spritemaps(FlyProgram.Flight).First(), definition);
            OamBuffer editedFly = DrawEnemy(edited, NewFrameReadGuard(rom),
                FlyAnimationsConstants.Spritemaps(FlyProgram.Flight).First(), definition);
            AssertEqual(unchecked((byte)(stockFly.LowTable[0] + 1)),
                editedFly.LowTable[0],
                $"fly ${(int)definition:X4} uses edited shared presentation");
        }
        OamBuffer stockKago = DrawEnemy(stock, NewFrameReadGuard(rom),
            KagoAnimationsConstants.Spritemaps(KagoProgram.Slow).First(),
            EnemyDefinitionId.Kago);
        OamBuffer editedKago = DrawEnemy(edited, NewFrameReadGuard(rom),
            KagoAnimationsConstants.Spritemaps(KagoProgram.Slow).First(),
            EnemyDefinitionId.Kago);
        AssertEqual(unchecked((byte)(stockKago.LowTable[0] + 1)),
            editedKago.LowTable[0],
            "Kago authored offset changes installed presentation");
        ushort faceBlockPointer = BlueBrinstarFaceBlockAnimations.Spritemaps(BlueBrinstarFaceBlockProgram.Initial).Single();
        OamBuffer stockFaceBlock = DrawEnemy(stock, NewFrameReadGuard(rom),
            faceBlockPointer, EnemyDefinitionId.FaceBlock);
        OamBuffer editedFaceBlock = DrawEnemy(edited, NewFrameReadGuard(rom),
            faceBlockPointer, EnemyDefinitionId.FaceBlock);
        AssertEqual(unchecked((byte)(stockFaceBlock.LowTable[0] + 1)),
            editedFaceBlock.LowTable[0],
            "face-block authored offset changes installed presentation");
        ushort morphEyePointer = MorphBallEyeAnimationsConstants.Spritemaps(MorphBallEyeProgram.MountFacingLeft).Single();
        OamBuffer stockMorphEye = DrawEnemy(stock, NewFrameReadGuard(rom),
            morphEyePointer, EnemyDefinitionId.Eye);
        OamBuffer editedMorphEye = DrawEnemy(edited, NewFrameReadGuard(rom),
            morphEyePointer, EnemyDefinitionId.Eye);
        AssertEqual(unchecked((byte)(stockMorphEye.LowTable[0] + 1)),
            editedMorphEye.LowTable[0],
            "Morph Ball eye mount art edit changes installed presentation");
        ushort shutterPointer = ShutterAnimationsConstants.Spritemaps(ShutterProgram.FortyPixels).First();
        foreach (EnemyDefinitionId definition in new EnemyDefinitionId[]
                 { EnemyDefinitionId.ShutterGrowing,
                   EnemyDefinitionId.ShutterShootable,
                   EnemyDefinitionId.ShutterDestroyable })
        {
            OamBuffer stockShutter = DrawEnemy(stock, NewFrameReadGuard(rom),
                shutterPointer, definition);
            OamBuffer editedShutter = DrawEnemy(edited, NewFrameReadGuard(rom),
                shutterPointer, definition);
            AssertEqual(unchecked((byte)(stockShutter.LowTable[0] + 1)),
                editedShutter.LowTable[0],
                $"shutter ${(int)definition:X4} uses edited shared forty-pixel art");
            AssertEqual(stockShutter.LowTable[1], editedShutter.LowTable[1],
                $"shutter ${(int)definition:X4} visual edit leaves physical Y unchanged");
        }
        ushort metroidPointer = MetroidAnimationsConstants.Spritemaps(MetroidProgram.ChasingSamus).First();
        OamBuffer stockMetroid = DrawEnemy(stock, NewFrameReadGuard(rom),
            metroidPointer, EnemyDefinitionId.Metroid);
        OamBuffer editedMetroid = DrawEnemy(edited, NewFrameReadGuard(rom),
            metroidPointer, EnemyDefinitionId.Metroid);
        AssertEqual(unchecked((byte)(stockMetroid.LowTable[0] + 1)),
            editedMetroid.LowTable[0],
            "ordinary-Metroid body art edit changes live OAM X without a ROM read");
        AssertEqual(stockMetroid.LowTable[1], editedMetroid.LowTable[1],
            "ordinary-Metroid body art edit leaves physical Y unchanged");
        OamBuffer stockBrain = DrawInstalledMotherBrainFrame(
            stock, 0xa586, new(0x0140, 0x00a0));
        OamBuffer editedBrain = DrawInstalledMotherBrainFrame(
            edited, 0xa586, new(0x0140, 0x00a0));
        AssertEqual(unchecked((byte)(stockBrain.LowTable[0] + 1)),
            editedBrain.LowTable[0],
            "Mother Brain head artwork edit changes live OAM X without ROM reads");
        AssertEqual(stockBrain.LowTable[1], editedBrain.LowTable[1],
            "Mother Brain head artwork edit leaves physical Y placement unchanged");
        OamBuffer stockDeadTorizo = DrawDeadTorizoCorpseFrame(stock, rom, Position16<RoomPixels>.Zero);
        OamBuffer editedDeadTorizo = DrawDeadTorizoCorpseFrame(edited, rom, Position16<RoomPixels>.Zero);
        AssertEqual(unchecked((byte)(stockDeadTorizo.LowTable[0] + 1)),
            editedDeadTorizo.LowTable[0],
            "Dead Torizo corpse composition edit changes live OAM X without ROM reads");
        AssertEqual(stockDeadTorizo.LowTable[1], editedDeadTorizo.LowTable[1],
            "Dead Torizo corpse composition edit leaves physical Y placement unchanged");
        OamBuffer stockRidleyWing = DrawInstalledRidleySupplement(
            stock, facing: 0, wingFrame: 0, tailDirection: null);
        OamBuffer editedRidleyWing = DrawInstalledRidleySupplement(
            edited, facing: 0, wingFrame: 0, tailDirection: null);
        AssertEqual(unchecked((byte)(stockRidleyWing.LowTable[0] + 1)),
            editedRidleyWing.LowTable[0],
            "Ridley wing artwork edit changes live OAM X without ROM reads");
        AssertEqual(stockRidleyWing.LowTable[1], editedRidleyWing.LowTable[1],
            "Ridley wing artwork edit leaves physical Y placement unchanged");
        OamBuffer stockMaw = DrawEnemy(stock, NewFrameReadGuard(rom),
            mawIdlePointer, EnemyDefinitionId.YappingMaw);
        OamBuffer editedMaw = DrawEnemy(edited, NewFrameReadGuard(rom),
            mawIdlePointer, EnemyDefinitionId.YappingMaw);
        AssertEqual(unchecked((byte)(stockMaw.LowTable[1] + 1)),
            editedMaw.LowTable[1],
            "editable Yapping Maw OAM offset changes its displayed frame");
        AssertEqual(stockMaw.LowTable[0], editedMaw.LowTable[0],
            "Yapping Maw visual edit leaves physical X placement unchanged");
        OamBuffer stockHunter = DrawEnemy(stock, NewFrameReadGuard(rom),
            hunterIdlePointer, EnemyDefinitionId.KihunterGreen);
        OamBuffer editedHunter = DrawEnemy(edited, NewFrameReadGuard(rom),
            hunterIdlePointer, EnemyDefinitionId.KihunterGreen);
        AssertEqual(unchecked((byte)(stockHunter.LowTable[1] + 1)),
            editedHunter.LowTable[1],
            "editable KiHunter OAM offset changes its displayed frame");
        AssertEqual(stockHunter.LowTable[0], editedHunter.LowTable[0],
            "KiHunter visual edit leaves physical X placement unchanged");
        OamBuffer stockIntroEvir = DrawRoomSpriteObject(stock, RoomSpriteObjectKind.DraygonIntroEvir);
        OamBuffer editedIntroEvir = DrawRoomSpriteObject(edited, RoomSpriteObjectKind.DraygonIntroEvir);
        AssertEqual(unchecked((byte)(stockIntroEvir.LowTable[0] + 1)),
            editedIntroEvir.LowTable[0],
            "editable Draygon intro Evir offset changes live room sprite-object OAM");
        AssertEqual(stockIntroEvir.LowTable[1], editedIntroEvir.LowTable[1],
            "Draygon intro Evir edit leaves vertical placement unchanged");
        OamBuffer stockDeathEvir = DrawRoomSpriteObject(stock, RoomSpriteObjectKind.DraygonDeathEvirFacingRight);
        OamBuffer editedDeathEvir = DrawRoomSpriteObject(edited, RoomSpriteObjectKind.DraygonDeathEvirFacingRight);
        AssertEqual(unchecked((byte)(stockDeathEvir.LowTable[1] + 1)),
            editedDeathEvir.LowTable[1],
            "address-named shared sprite-object edit changes live room OAM");
        AssertEqual(stockDeathEvir.LowTable[0], editedDeathEvir.LowTable[0],
            "shared sprite-object edit leaves physical X placement unchanged");
        OamBuffer stockFune = DrawEnemy(stock, NewFrameReadGuard(rom),
            0x94cb, EnemyDefinitionId.Fune);
        OamBuffer editedFune = DrawEnemy(edited, NewFrameReadGuard(rom),
            0x94cb, EnemyDefinitionId.Fune);
        AssertEqual(unchecked((byte)(stockFune.LowTable[1] + 1)),
            editedFune.LowTable[1],
            "editable Fune idle OAM offset changes the displayed enemy frame");
        AssertEqual(stockFune.LowTable[0], editedFune.LowTable[0],
            "Fune visual edit does not change physical X position");
        OamBuffer stockKamer = DrawEnemy(stock, NewFrameReadGuard(rom),
            0xf468, EnemyDefinitionId.Kamer);
        OamBuffer editedKamer = DrawEnemy(edited, NewFrameReadGuard(rom),
            0xf468, EnemyDefinitionId.Kamer);
        AssertEqual(unchecked((byte)(stockKamer.LowTable[1] + 1)),
            editedKamer.LowTable[1],
            "editable Kamer platform OAM offset changes its displayed frame");
        AssertEqual(stockKamer.LowTable[0], editedKamer.LowTable[0],
            "Kamer visual edit leaves its physical X position unchanged");
        OamBuffer stockElevator = DrawEnemy(stock, NewFrameReadGuard(rom),
            0x962f, EnemyDefinitionId.Elevator);
        OamBuffer editedElevator = DrawEnemy(edited, NewFrameReadGuard(rom),
            0x962f, EnemyDefinitionId.Elevator);
        AssertEqual(unchecked((byte)(stockElevator.LowTable[1] + 1)),
            editedElevator.LowTable[1],
            "editable elevator OAM offset changes its displayed frame");
        AssertEqual(stockElevator.LowTable[0], editedElevator.LowTable[0],
            "elevator visual edit leaves its physical X position unchanged");
        ushort framePointer = BoyonAnimationsConstants.Spritemaps(BoyonProgram.IdleLoop).First();
        var stockOam = DrawEnemy(stock, NewFrameReadGuard(rom), framePointer,
            EnemyDefinitionId.Boyon);
        var editedOam = DrawEnemy(edited, NewFrameReadGuard(rom), framePointer,
            EnemyDefinitionId.Boyon);
        AssertEqual(unchecked((byte)(stockOam.LowTable[0] + 1)),
            editedOam.LowTable[0], "authored Boyon X offset changes live room OAM");
        AssertEqual(unchecked((byte)(stockOam.LowTable[2] + 1)),
            editedOam.LowTable[2], "authored Boyon tile changes live room OAM");
        AssertEqual(stockOam.LowTable[1], editedOam.LowTable[1],
            "visual override leaves Boyon Y unchanged");
        OamBuffer stockCeresDoor = DrawEnemy(stock, NewFrameReadGuard(rom),
            0xfa13, EnemyDefinitionId.CeresDoor);
        OamBuffer editedCeresDoor = DrawEnemy(edited, NewFrameReadGuard(rom),
            0xfa13, EnemyDefinitionId.CeresDoor,
            slot =>
            {
                AssertEqual((ushort)0xfa13, slot.SpritemapPointer,
                    "Ceres door art edit retains the native selected pose");
                AssertEqual((ushort)7, slot.InstructionTimer.RawValue,
                    "Ceres door art edit preserves instruction timing");
            });
        AssertEqual(unchecked((byte)(stockCeresDoor.LowTable[1] + 1)),
            editedCeresDoor.LowTable[1],
            "authored Ceres door Y offset changes live room OAM");
        AssertEqual(stockCeresDoor.LowTable[0], editedCeresDoor.LowTable[0],
            "Ceres door visual edit leaves physical X unchanged");
        OamBuffer stockCeresBaby = DrawEnemy(stock, NewFrameReadGuard(rom),
            CeresBabyFrameDefinitions.RoundFrame,
            EnemyDefinitionId.CeresDoor);
        OamBuffer editedCeresBaby = DrawEnemy(edited, NewFrameReadGuard(rom),
            CeresBabyFrameDefinitions.RoundFrame,
            EnemyDefinitionId.CeresDoor);
        AssertEqual(unchecked((byte)(stockCeresBaby.LowTable[1] + 1)),
            editedCeresBaby.LowTable[1],
            "authored Ceres Baby Y offset changes installed OAM");
        ushort cacatacPointer = CacatacAnimationsConstants.Spritemaps(CacatacProgram.UpsideUpIdle).First();
        var stockCacatac = DrawEnemy(stock, NewFrameReadGuard(rom),
            cacatacPointer, EnemyDefinitionId.Cacatac);
        var editedCacatac = DrawEnemy(edited, NewFrameReadGuard(rom),
            cacatacPointer, EnemyDefinitionId.Cacatac);
        AssertEqual(unchecked((byte)(stockCacatac.LowTable[1] + 1)),
            editedCacatac.LowTable[1],
            "authored Cacatac Y offset changes live room OAM");
        AssertEqual(stockCacatac.LowTable[0], editedCacatac.LowTable[0],
            "Cacatac visual override leaves X unchanged");
        ushort boulderPointer = BoulderAnimationsConstants.Spritemaps(BoulderProgram.Left).First();
        var stockBoulder = DrawEnemy(stock, NewFrameReadGuard(rom),
            boulderPointer, EnemyDefinitionId.Boulder);
        var editedBoulder = DrawEnemy(edited, NewFrameReadGuard(rom),
            boulderPointer, EnemyDefinitionId.Boulder);
        AssertEqual(unchecked((byte)(stockBoulder.LowTable[1] + 1)),
            editedBoulder.LowTable[1],
            "authored Boulder Y offset changes live room OAM");
        AssertEqual(stockBoulder.LowTable[0], editedBoulder.LowTable[0],
            "Boulder visual override leaves X unchanged");
        ushort atomicPointer = AtomicAnimationsConstants.Spritemaps(AtomicProgram.UpRight).First();
        var stockAtomic = DrawEnemy(stock, NewFrameReadGuard(rom),
            atomicPointer, EnemyDefinitionId.Atomic);
        var editedAtomic = DrawEnemy(edited, NewFrameReadGuard(rom),
            atomicPointer, EnemyDefinitionId.Atomic);
        AssertEqual(unchecked((byte)(stockAtomic.LowTable[1] + 1)),
            editedAtomic.LowTable[1],
            "authored Atomic Y offset changes live room OAM");
        AssertEqual(stockAtomic.LowTable[0], editedAtomic.LowTable[0],
            "Atomic visual override leaves X unchanged");
        ushort skulteraPointer = SkulteraAnimationsConstants.Spritemaps(SkulteraProgram.SwimmingLeftLoop).First();
        var stockSkultera = DrawEnemy(stock, NewFrameReadGuard(rom),
            skulteraPointer, EnemyDefinitionId.Skultera);
        var editedSkultera = DrawEnemy(edited, NewFrameReadGuard(rom),
            skulteraPointer, EnemyDefinitionId.Skultera);
        AssertEqual(unchecked((byte)(stockSkultera.LowTable[1] + 1)),
            editedSkultera.LowTable[1],
            "authored Skultera Y offset changes live room OAM");
        AssertEqual(stockSkultera.LowTable[0], editedSkultera.LowTable[0],
            "Skultera visual override leaves X unchanged");
        ushort waverPointer = WaverAnimationsConstants.Spritemaps(WaverProgram.SteadyFacingLeft).First();
        var stockWaver = DrawEnemy(stock, NewFrameReadGuard(rom),
            waverPointer, EnemyDefinitionId.Waver);
        var editedWaver = DrawEnemy(edited, NewFrameReadGuard(rom),
            waverPointer, EnemyDefinitionId.Waver);
        AssertEqual(unchecked((byte)(stockWaver.LowTable[1] + 1)),
            editedWaver.LowTable[1],
            "authored Waver Y offset changes live room OAM");
        AssertEqual(stockWaver.LowTable[0], editedWaver.LowTable[0],
            "Waver visual override leaves X unchanged");
        foreach ((ushort pointer, EnemyDefinitionId definition, string name) in new[]
                 {
                     (ZoaAnimationsConstants.Spritemaps(ZoaProgram.FacingLeftShooting).First(),
                         EnemyDefinitionId.Zoa, "Zoa"),
                     (SkreeMetareeAnimationsConstants.Spritemaps(SkreeMetareeProgram.MetareeIdling).First(),
                         EnemyDefinitionId.Metaree, "Metaree"),
                     (SkreeMetareeAnimationsConstants.Spritemaps(SkreeMetareeProgram.SkreeIdling).First(),
                         EnemyDefinitionId.Skree, "Skree"),
                 })
        {
            OamBuffer nativeFrame = DrawEnemy(stock, NewFrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, NewFrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        foreach ((EnemyDefinitionId definition, PipeBugProgram program, string name) in new[]
                 {
                     (EnemyDefinitionId.Zeb, PipeBugProgram.ZebRisingLeft, "Brinstar Pipe Bug"),
                     (EnemyDefinitionId.Zebbo, PipeBugProgram.ZebboRisingLeft, "strong Brinstar Pipe Bug"),
                     (EnemyDefinitionId.Gamet, PipeBugProgram.GametRisingLeft, "Norfair Pipe Bug"),
                     (EnemyDefinitionId.Geega, PipeBugProgram.GeegaFlyingLeft, "yellow Pipe Bug"),
                 })
        {
            ushort pointer = PipeBugAnimationsConstants.Spritemaps(program).First();
            OamBuffer nativeFrame = DrawEnemy(stock, NewFrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, NewFrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        // Fake Kraid's and the fingernail's frames come from their typed programs (#641).
        foreach ((EnemyDefinitionId definition, ushort pointer, string name) in new[]
                 {
                     (EnemyDefinitionId.MiniKraid, FakeKraidAnimationsConstants.Spritemaps(FakeKraidProgram.StepForwardsFacingLeft).First(), "Fake Kraid"),
                     (EnemyDefinitionId.KraidNail, KraidPartFrameDefinitions.Spritemap(new KraidPartFrame(KraidPartFrameKind.Nail, 0)),
                         "Kraid fingernail"),
                 })
        {
            OamBuffer nativeFrame = DrawEnemy(stock, NewFrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, NewFrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        foreach ((EnemyDefinitionId definition, ushort pointer, string name) in new[]
                 {
                     (EnemyDefinitionId.Owtch, OwtchAnimationsConstants.Spritemaps(OwtchProgram.MovingLeftLoop).First(), "Owtch"),
                     (EnemyDefinitionId.Stoke, StokeAnimationsConstants.Spritemaps(StokeProgram.MovingLeftLoop).First(), "Stoke"),
                 })
        {
            OamBuffer nativeFrame = DrawEnemy(stock, NewFrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, NewFrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        foreach ((EnemyDefinitionId definition, RipperProgram program, string name) in new[]
                 {
                     (EnemyDefinitionId.GRipper, RipperProgram.GRipperMovingLeft, "GRipper"),
                     (EnemyDefinitionId.Ripper2, RipperProgram.Ripper2MovingRight, "Ripper II"),
                     (EnemyDefinitionId.Ripper, RipperProgram.RipperMovingRight, "Ripper"),
                 })
        {
            ushort pointer = RipperAnimationsConstants.Spritemaps(program).First();
            OamBuffer nativeFrame = DrawEnemy(stock, NewFrameReadGuard(rom),
                pointer, definition);
            OamBuffer editedFrame = DrawEnemy(edited, NewFrameReadGuard(rom),
                pointer, definition);
            AssertEqual(unchecked((byte)(nativeFrame.LowTable[1] + 1)),
                editedFrame.LowTable[1],
                $"authored {name} Y offset changes live room OAM");
            AssertEqual(nativeFrame.LowTable[0], editedFrame.LowTable[0],
                $"{name} visual override leaves X unchanged");
        }
        OamBuffer stockFrozen = DrawEnemy(stock, NewFrameReadGuard(rom),
            RipperFrameDefinitionsConstants.FrozenFacingLeftSpritemap,
            EnemyDefinitionId.GRipper);
        OamBuffer editedFrozen = DrawEnemy(edited, NewFrameReadGuard(rom),
            RipperFrameDefinitionsConstants.FrozenFacingLeftSpritemap,
            EnemyDefinitionId.GRipper);
        AssertEqual(unchecked((byte)(stockFrozen.LowTable[1] + 1)),
            editedFrozen.LowTable[1], "authored frozen GRipper Y offset changes live room OAM");
        ushort firefleaPointer = FirefleaAnimationsConstants.Spritemaps(FirefleaProgram.Loop).First();
        OamBuffer stockFireflea = DrawEnemy(stock, NewFrameReadGuard(rom),
            firefleaPointer, EnemyDefinitionId.Fireflea);
        OamBuffer editedFireflea = DrawEnemy(edited, NewFrameReadGuard(rom),
            firefleaPointer, EnemyDefinitionId.Fireflea);
        AssertEqual(unchecked((byte)(stockFireflea.LowTable[1] + 1)),
            editedFireflea.LowTable[1],
            "authored Fireflea Y offset changes live room OAM");
        AssertEqual(stockFireflea.LowTable[0], editedFireflea.LowTable[0],
            "Fireflea visual edit leaves X position unchanged");
        ushort magdollitePointer = MagdolliteAnimationsConstants.Spritemaps(MagdolliteProgram.LeftIdle).First();
        OamBuffer stockMagdollite = DrawEnemy(stock, NewFrameReadGuard(rom),
            magdollitePointer, EnemyDefinitionId.Magdollite);
        OamBuffer editedMagdollite = DrawEnemy(edited, NewFrameReadGuard(rom),
            magdollitePointer, EnemyDefinitionId.Magdollite);
        AssertEqual(unchecked((byte)(stockMagdollite.LowTable[1] + 1)),
            editedMagdollite.LowTable[1],
            "authored Magdollite Y offset changes live room OAM");
        AssertEqual(stockMagdollite.LowTable[0], editedMagdollite.LowTable[0],
            "Magdollite visual edit leaves X position unchanged");
        ushort rioPointer = RioAnimationsConstants.Spritemaps(RioProgram.Idle).First();
        OamBuffer stockRio = DrawEnemy(stock, NewFrameReadGuard(rom),
            rioPointer, EnemyDefinitionId.Rio);
        OamBuffer editedRio = DrawEnemy(edited, NewFrameReadGuard(rom),
            rioPointer, EnemyDefinitionId.Rio);
        AssertEqual(unchecked((byte)(stockRio.LowTable[1] + 1)),
            editedRio.LowTable[1],
            "authored Rio Y offset changes installed room OAM");
        AssertEqual(stockRio.LowTable[0], editedRio.LowTable[0],
            "Rio visual edit leaves physical X unchanged");
        ushort lowerRioPointer = LowerNorfairRioAnimationsConstants.Spritemaps(LowerNorfairRioProgram.IdleLoop).First();
        OamBuffer stockLowerRio = DrawEnemy(stock, NewFrameReadGuard(rom),
            lowerRioPointer, EnemyDefinitionId.Holtz);
        OamBuffer editedLowerRio = DrawEnemy(edited, NewFrameReadGuard(rom),
            lowerRioPointer, EnemyDefinitionId.Holtz);
        AssertEqual(unchecked((byte)(stockLowerRio.LowTable[1] + 1)),
            editedLowerRio.LowTable[1],
            "authored Lower Norfair Rio Y offset changes installed room OAM");
        AssertEqual(stockLowerRio.LowTable[0], editedLowerRio.LowTable[0],
            "Lower Norfair Rio visual edit leaves physical X unchanged");
        ushort norfairRioPointer = NorfairRioAnimationsConstants.Spritemaps(NorfairRioProgram.Idle).First();
        OamBuffer stockNorfairRio = DrawEnemy(stock, NewFrameReadGuard(rom),
            norfairRioPointer, EnemyDefinitionId.Geruta);
        OamBuffer editedNorfairRio = DrawEnemy(edited, NewFrameReadGuard(rom),
            norfairRioPointer, EnemyDefinitionId.Geruta);
        AssertEqual(unchecked((byte)(stockNorfairRio.LowTable[1] + 1)),
            editedNorfairRio.LowTable[1],
            "authored Norfair Rio Y offset changes installed room OAM");
        AssertEqual(stockNorfairRio.LowTable[0], editedNorfairRio.LowTable[0],
            "Norfair Rio visual edit leaves physical X unchanged");
        ushort puyoPointer = PuyoAnimationsConstants.Spritemaps(PuyoProgram.GroundedFast).First();
        OamBuffer stockPuyo = DrawEnemy(stock, NewFrameReadGuard(rom),
            puyoPointer, EnemyDefinitionId.Puyo);
        OamBuffer editedPuyo = DrawEnemy(edited, NewFrameReadGuard(rom),
            puyoPointer, EnemyDefinitionId.Puyo);
        AssertEqual(unchecked((byte)(stockPuyo.LowTable[1] + 1)),
            editedPuyo.LowTable[1],
            "authored Puyo Y offset changes installed room OAM");
        AssertEqual(stockPuyo.LowTable[0], editedPuyo.LowTable[0],
            "Puyo visual edit leaves physical X unchanged");
        ushort bullPointer = BullAnimationsConstants.Spritemaps(BullProgram.Normal).First();
        OamBuffer stockBull = DrawEnemy(stock, NewFrameReadGuard(rom),
            bullPointer, EnemyDefinitionId.Bull);
        OamBuffer editedBull = DrawEnemy(edited, NewFrameReadGuard(rom),
            bullPointer, EnemyDefinitionId.Bull);
        AssertEqual(unchecked((byte)(stockBull.LowTable[1] + 1)),
            editedBull.LowTable[1],
            "authored Bull Y offset changes installed room OAM");
        AssertEqual(stockBull.LowTable[0], editedBull.LowTable[0],
            "Bull visual edit leaves physical X unchanged");
        ushort alcoonPointer = AlcoonAnimationsConstants.Spritemaps(AlcoonProgram.WalkingLeftLoop).First();
        OamBuffer stockAlcoon = DrawEnemy(stock, NewFrameReadGuard(rom),
            alcoonPointer, EnemyDefinitionId.Alcoon);
        OamBuffer editedAlcoon = DrawEnemy(edited, NewFrameReadGuard(rom),
            alcoonPointer, EnemyDefinitionId.Alcoon);
        AssertEqual(unchecked((byte)(stockAlcoon.LowTable[1] + 1)),
            editedAlcoon.LowTable[1],
            "authored Alcoon Y offset changes installed room OAM");
        AssertEqual(stockAlcoon.LowTable[0], editedAlcoon.LowTable[0],
            "Alcoon visual edit leaves physical X unchanged");
        ushort beetomPointer = BeetomAnimationsConstants.Spritemaps(BeetomProgram.CrawlingLeftLoop).First();
        OamBuffer stockBeetom = DrawEnemy(stock, NewFrameReadGuard(rom),
            beetomPointer, EnemyDefinitionId.Beetom);
        OamBuffer editedBeetom = DrawEnemy(edited, NewFrameReadGuard(rom),
            beetomPointer, EnemyDefinitionId.Beetom);
        AssertEqual(unchecked((byte)(stockBeetom.LowTable[1] + 1)),
            editedBeetom.LowTable[1],
            "authored Beetom Y offset changes installed room OAM");
        AssertEqual(stockBeetom.LowTable[0], editedBeetom.LowTable[0],
            "Beetom visual edit leaves physical X unchanged");
        ushort hopperPointer = HopperAnimationsConstants.Spritemaps(HopperProgram.SidehopperFloorJumping).Single();
        OamBuffer stockHopper = DrawEnemy(stock, NewFrameReadGuard(rom),
            hopperPointer, EnemyDefinitionId.Sidehopper);
        OamBuffer editedHopper = DrawEnemy(edited, NewFrameReadGuard(rom),
            hopperPointer, EnemyDefinitionId.Sidehopper);
        AssertEqual(unchecked((byte)(stockHopper.LowTable[1] + 1)),
            editedHopper.LowTable[1],
            "authored Sidehopper Y offset changes installed room OAM");
        AssertEqual(stockHopper.LowTable[0], editedHopper.LowTable[0],
            "Sidehopper visual edit leaves physical X unchanged");
        ushort chootPointer = ChootAnimationsConstants.Spritemaps(ChootProgram.Idle).First();
        OamBuffer stockChoot = DrawEnemy(stock, NewFrameReadGuard(rom),
            chootPointer, EnemyDefinitionId.Choot);
        OamBuffer editedChoot = DrawEnemy(edited, NewFrameReadGuard(rom),
            chootPointer, EnemyDefinitionId.Choot);
        AssertEqual(unchecked((byte)(stockChoot.LowTable[1] + 1)),
            editedChoot.LowTable[1],
            "authored Choot Y offset changes installed room OAM");
        AssertEqual(stockChoot.LowTable[0], editedChoot.LowTable[0],
            "Choot visual edit leaves physical X unchanged");
        ushort hzoomerPointer = CrawlerAnimations.Spritemaps(CrawlerProgram.HZoomerUpsideRightLoop).First();
        OamBuffer stockHZoomer = DrawEnemy(stock, NewFrameReadGuard(rom),
            hzoomerPointer, EnemyDefinitionId.HZoomer);
        OamBuffer editedHZoomer = DrawEnemy(edited, NewFrameReadGuard(rom),
            hzoomerPointer, EnemyDefinitionId.HZoomer);
        AssertEqual(unchecked((byte)(stockHZoomer.LowTable[1] + 1)),
            editedHZoomer.LowTable[1],
            "authored HZoomer Y offset changes installed room OAM");
        AssertEqual(stockHZoomer.LowTable[0], editedHZoomer.LowTable[0],
            "HZoomer visual edit leaves physical X unchanged");
        ushort sbugPointer = SbugAnimationsConstants.Spritemaps(SbugProgram.Right).First();
        foreach (EnemyDefinitionId definition in new EnemyDefinitionId[]
                 { EnemyDefinitionId.Sbug, EnemyDefinitionId.Sbug2 })
        {
            OamBuffer stockSbug = DrawEnemy(stock, NewFrameReadGuard(rom),
                sbugPointer, definition);
            OamBuffer editedSbug = DrawEnemy(edited, NewFrameReadGuard(rom),
                sbugPointer, definition);
            AssertEqual(unchecked((byte)(stockSbug.LowTable[1] + 1)),
                editedSbug.LowTable[1],
                $"authored Sbug Y offset changes installed enemy ${(int)definition:X4}");
            AssertEqual(stockSbug.LowTable[0], editedSbug.LowTable[0],
                $"Sbug visual edit leaves enemy ${(int)definition:X4} physical X unchanged");
        }
        Suite(nameof(CrawlerBehaviorVerification.VerifyCrawlerBehavior), () => CrawlerBehaviorVerification.VerifyCrawlerBehavior(edited));
        foreach (EnemyDefinitionId definition in new EnemyDefinitionId[]
                 {
                     EnemyDefinitionId.Zeela,
                     EnemyDefinitionId.Sova,
                     EnemyDefinitionId.Zoomer,
                     EnemyDefinitionId.MZoomer,
                 })
        {
            OamBuffer stockCrawler = DrawEnemy(stock, NewFrameReadGuard(rom),
                hzoomerPointer, definition);
            OamBuffer editedCrawler = DrawEnemy(edited, NewFrameReadGuard(rom),
                hzoomerPointer, definition);
            AssertEqual(unchecked((byte)(stockCrawler.LowTable[1] + 1)),
                editedCrawler.LowTable[1],
                $"shared crawler ${(int)definition:X4} uses the edited visual Y offset");
            AssertEqual(stockCrawler.LowTable[0], editedCrawler.LowTable[0],
                $"shared crawler ${(int)definition:X4} retains physical X");
        }
        AssertTrue((EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .Spritemaps ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory) .Spritemaps.")).TryGet(EnemySpritemapDefinitions.BoyonBank, framePointer, out _),
            "enemy composition override survives catalog reload");
        // Preserve the edited values while selecting exactly the pre-Dragon schema.
        HashSet<string> historicalNames = EnemySpritemapDefinitions.Frames
            .Take(EnemySpritemapDefinitions.PreDragonFrameCount)
            .Select(frame => frame.Name).ToHashSet(StringComparer.Ordinal);
        IEnumerable<KeyValuePair<string, SpriteVisualPart[]>> HistoricalFrames() =>
            document.Frames.Where(pair => historicalNames.Contains(pair.Key));
        var preMetroidFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("metroid_body_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("shaktool_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("tripper_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMetroidFrameCount,
            preMetroidFrames.Count, "pre-Metroid composition schema count");
        var preMetroidBindings = document.DisplayFrames
            .Where(pair => preMetroidFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreMetroid,
                Frames = preMetroidFrames,
                DisplayFrames = preMetroidBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preMetroidUpgraded = LoadSpritemapOverride();
        foreach (EnemySpritemapDefinition frame in MetroidVisualDefinitions.Frames())
            AssertTrue(preMetroidUpgraded.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-forty-four override inherits Metroid frame {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.PreMetroid,
            Frames = preMetroidFrames,
            DisplayFrames = preMetroidBindings,
        };
        var preShutterFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("shutter_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreShutterFrameCount,
            preShutterFrames.Count, "pre-shutter composition schema count");
        var preShutterBindings = document.DisplayFrames
            .Where(pair => preShutterFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreShutter,
                Frames = preShutterFrames,
                DisplayFrames = preShutterBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preShutterUpgraded = LoadSpritemapOverride();
        foreach (EnemySpritemapDefinition frame in ShutterVisualDefinitions.Frames())
            AssertTrue(preShutterUpgraded.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-forty-three override inherits shutter frame {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.PreShutter,
            Frames = preShutterFrames,
            DisplayFrames = preShutterBindings,
        };
        var preMorphBallEyeFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("morph_eye_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMorphBallEyeFrameCount,
            preMorphBallEyeFrames.Count, "pre-Morph-Ball-eye composition schema count");
        var preMorphBallEyeBindings = document.DisplayFrames
            .Where(pair => preMorphBallEyeFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreMorphBallEye,
                Frames = preMorphBallEyeFrames,
                DisplayFrames = preMorphBallEyeBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preMorphBallEyeUpgraded = LoadSpritemapOverride();
        foreach (EnemySpritemapDefinition frame in MorphBallEyeVisualDefinitions.Frames())
            AssertTrue(preMorphBallEyeUpgraded.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-forty-two override inherits eye frame {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.PreMorphBallEye,
            Frames = preMorphBallEyeFrames,
            DisplayFrames = preMorphBallEyeBindings,
        };
        var preFaceBlockFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("face_block_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreFaceBlockFrameCount,
            preFaceBlockFrames.Count, "pre-face-block composition schema frame count");
        var preFaceBlockBindings = document.DisplayFrames
            .Where(pair => preFaceBlockFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreFaceBlock,
                Frames = preFaceBlockFrames,
                DisplayFrames = preFaceBlockBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preFaceBlockUpgraded = LoadSpritemapOverride();
        foreach (EnemySpritemapDefinition frame in BlueBrinstarFaceBlockVisualDefinitions.Frames())
            AssertTrue(preFaceBlockUpgraded.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-forty-one override inherits face-block frame {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.PreFaceBlock,
            Frames = preFaceBlockFrames,
            DisplayFrames = preFaceBlockBindings,
        };
        var preKagoFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("kago_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreKagoFrameCount,
            preKagoFrames.Count, "pre-Kago composition schema frame count");
        var preKagoBindings = document.DisplayFrames
            .Where(pair => preKagoFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreKago,
                Frames = preKagoFrames,
                DisplayFrames = preKagoBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preKagoUpgraded = LoadSpritemapOverride();
        foreach (EnemySpritemapDefinition frame in KagoVisualDefinitions.Frames())
            AssertTrue(preKagoUpgraded.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-forty override inherits Kago frame {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.PreKago,
            Frames = preKagoFrames,
            DisplayFrames = preKagoBindings,
        };
        var preFlyFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("fly_shared_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreFlyFrameCount,
            preFlyFrames.Count, "pre-fly composition schema frame count");
        var preFlyBindings = document.DisplayFrames
            .Where(pair => preFlyFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreFly,
                Frames = preFlyFrames,
                DisplayFrames = preFlyBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preFlyUpgraded = LoadSpritemapOverride();
        foreach (EnemySpritemapDefinition frame in FlyVisualDefinitions.Frames())
            AssertTrue(preFlyUpgraded.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-thirty-nine override inherits fly frame {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.PreFly,
            Frames = preFlyFrames,
            DisplayFrames = preFlyBindings,
        };
        var preSciserFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("sciser_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreSciserFrameCount,
            preSciserFrames.Count, "pre-Sciser composition schema frame count");
        var preSciserBindings = document.DisplayFrames
            .Where(pair => preSciserFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreSciser,
                Frames = preSciserFrames,
                DisplayFrames = preSciserBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preSciserUpgraded = LoadSpritemapOverride();
        foreach (EnemySpritemapDefinition frame in SciserVisualDefinitions.Frames())
            AssertTrue(preSciserUpgraded.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-thirty-eight override inherits Sciser {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.PreSciser,
            Frames = preSciserFrames,
            DisplayFrames = preSciserBindings,
        };
        var preRidleyFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("ridley_supplement_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreRidleySupplementFrameCount,
            preRidleyFrames.Count, "pre-Ridley-supplement composition schema frame count");
        var preRidleyBindings = document.DisplayFrames
            .Where(pair => preRidleyFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreRidleySupplement,
                Frames = preRidleyFrames,
                DisplayFrames = preRidleyBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preRidleyUpgraded = LoadSpritemapOverride();
        foreach (EnemySpritemapDefinition frame in RidleySupplementalVisualDefinitions.Frames())
            AssertTrue(preRidleyUpgraded.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-thirty-seven override inherits Ridley supplement {frame.Name}");
        document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.PreRidleySupplement,
            Frames = preRidleyFrames,
            DisplayFrames = preRidleyBindings,
        };
        var preDeadTorizoFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("dead_torizo_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreDeadTorizoFrameCount,
            preDeadTorizoFrames.Count, "pre-Dead-Torizo composition schema frame count");
        var preDeadTorizoBindings = document.DisplayFrames
            .Where(pair => preDeadTorizoFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreDeadTorizo,
                Frames = preDeadTorizoFrames,
                DisplayFrames = preDeadTorizoBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preDeadTorizoUpgraded = LoadSpritemapOverride();
        AssertTrue(preDeadTorizoUpgraded.TryGetDisplay(
                DeadTorizoArtworkDefinitions.SpritemapBank,
                DeadTorizoArtworkDefinitions.HookSpritemap, out _),
            "version-thirty-six override inherits Dead Torizo corpse frame");
        document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.PreDeadTorizo,
            Frames = preDeadTorizoFrames,
            DisplayFrames = preDeadTorizoBindings,
        };
        var preMotherBrainFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("mother_brain_a9_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMotherBrainFrameCount,
            preMotherBrainFrames.Count, "pre-Mother-Brain composition schema frame count");
        var preMotherBrainBindings = document.DisplayFrames
            .Where(pair => preMotherBrainFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreMotherBrain,
                Frames = preMotherBrainFrames,
                DisplayFrames = preMotherBrainBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preMotherBrainUpgraded = LoadSpritemapOverride();
        foreach (EnemySpritemapDefinition frame in MotherBrainVisualDefinitions.Frames())
            AssertTrue(preMotherBrainUpgraded.TryGetDisplay(
                    frame.Bank, frame.Pointer, out _),
                $"version-thirty-five override inherits Mother Brain frame {frame.Name}");
        // All earlier migration fixtures start from the previous complete schema.
        document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.PreMotherBrain,
            Frames = preMotherBrainFrames,
            DisplayFrames = preMotherBrainBindings,
        };
        var preHunterFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("ki_hunter_a8_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreKiHunterFrameCount,
            preHunterFrames.Count, "pre-KiHunter composition schema frame count");
        var preHunterBindings = document.DisplayFrames
            .Where(pair => preHunterFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreKiHunter,
                Frames = preHunterFrames,
                DisplayFrames = preHunterBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preHunterUpgraded = LoadSpritemapOverride();
        foreach (ushort pointer in installedHunterPointers)
        {
            AssertTrue(preHunterUpgraded.TryGetDisplay(
                    KiHunterVisualDefinitions.Bank, pointer, out _),
                $"version-thirty-four override inherits KiHunter frame ${pointer:X4}");
        }
        var preMawFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("yapping_maw_a8_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("ki_hunter_a8_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreYappingMawFrameCount,
            preMawFrames.Count, "pre-Yapping-Maw composition schema frame count");
        var preMawBindings = document.DisplayFrames
            .Where(pair => preMawFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreYappingMaw,
                Frames = preMawFrames,
                DisplayFrames = preMawBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preMawUpgraded = LoadSpritemapOverride();
        foreach (ushort pointer in installedMawPointers)
        {
            AssertTrue(preMawUpgraded.TryGetDisplay(
                    YappingMawVisualDefinitions.Bank, pointer, out _),
                $"version-thirty-three override inherits Yapping Maw frame ${pointer:X4}");
        }
        var preRoomSpriteFrames = HistoricalFrames()
            .Where(pair => !pair.Key.StartsWith("room_sprite_b4_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("yapping_maw_a8_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("ki_hunter_a8_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreRoomSpriteObjectFrameCount,
            preRoomSpriteFrames.Count, "pre-shared-sprite-object composition schema frame count");
        var preRoomSpriteBindings = document.DisplayFrames
            .Where(pair => preRoomSpriteFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreRoomSpriteObject,
                Frames = preRoomSpriteFrames,
                DisplayFrames = preRoomSpriteBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preRoomSpriteUpgraded = LoadSpritemapOverride();
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        {
            if (!frame.Name.StartsWith("room_sprite_b4_", StringComparison.Ordinal))
                continue;
            AssertTrue(preRoomSpriteUpgraded.TryGetDisplay(
                    EnemySpritemapDefinitions.RoomSpriteObjectBank, frame.Pointer, out _),
                $"version-thirty-two override inherits stock sprite-object frame {frame.Name}");
        }
        var preBreathFrames = preRoomSpriteFrames
            .Where(pair => !pair.Key.StartsWith("draygon_breath_bubble_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreDraygonBreathFrameCount,
            preBreathFrames.Count, "pre-Draygon-breath composition schema frame count");
        var preBreathBindings = preRoomSpriteBindings
            .Where(pair => preBreathFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreDraygonBreath,
                Frames = preBreathFrames,
                DisplayFrames = preBreathBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preBreathUpgraded = LoadSpritemapOverride();
        foreach (ushort pointer in new ushort[]
                 { 0xc920, 0xc927, 0xc938, 0xc949, 0xc95a, 0xc96b, 0xc97c, 0xc98d, 0xc999 })
        {
            AssertTrue(preBreathUpgraded.TryGetDisplay(
                    EnemySpritemapDefinitions.RoomSpriteObjectBank, pointer, out _),
                $"version-thirty-one override inherits stock Draygon breath frame ${pointer:X4}");
        }
        var preDraygonFrames = preBreathFrames
            .Where(pair => !pair.Key.StartsWith("draygon_intro_evir_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreDraygonIntroFrameCount,
            preDraygonFrames.Count, "pre-Draygon composition schema frame count");
        var preDraygonBindings = preBreathBindings
            .Where(pair => preDraygonFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreDraygonIntro,
                Frames = preDraygonFrames,
                DisplayFrames = preDraygonBindings,
            }, CamelCaseJson));
        EnemySpritemapCatalog preDraygonUpgraded = LoadSpritemapOverride();
        foreach (ushort pointer in new ushort[] { 0xdb42, 0xdb80, 0xdbbe, 0xdbfc })
        {
            AssertTrue(preDraygonUpgraded.TryGetDisplay(
                    EnemySpritemapDefinitions.RoomSpriteObjectBank, pointer, out _),
                $"version-thirty override inherits stock Draygon intro frame ${pointer:X4}");
        }
        var preElevatorFrames = preDraygonFrames
            .Where(pair => !pair.Key.StartsWith("elevator_platform_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreElevatorFrameCount,
            preElevatorFrames.Count, "pre-elevator composition schema frame count");
        var preElevatorBindings = document.DisplayFrames
            .Where(pair => preElevatorFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreElevator,
                Frames = preElevatorFrames,
                DisplayFrames = preElevatorBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preElevatorUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        AssertTrue(DrawEnemy(preElevatorUpgraded, NewFrameReadGuard(rom), 0x962f,
                EnemyDefinitionId.Elevator).LowTable
            .SequenceEqual(DrawEnemy(stock, NewFrameReadGuard(rom), 0x962f,
                EnemyDefinitionId.Elevator).LowTable),
            "version-twenty-nine override inherits stock elevator OAM without ROM reads");
        var preKamerFrames = preDraygonFrames
            .Where(pair => !pair.Key.StartsWith("kamer_platform_", StringComparison.Ordinal))
            .Where(pair => !pair.Key.StartsWith("elevator_platform_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreKamerFrameCount,
            preKamerFrames.Count, "pre-Kamer composition schema frame count");
        var preKamerBindings = document.DisplayFrames
            .Where(pair => preKamerFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreKamer,
                Frames = preKamerFrames,
                DisplayFrames = preKamerBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preKamerUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        AssertTrue(DrawEnemy(preKamerUpgraded, NewFrameReadGuard(rom), 0xf468,
                EnemyDefinitionId.Kamer).LowTable
            .SequenceEqual(DrawEnemy(stock, NewFrameReadGuard(rom), 0xf468,
                EnemyDefinitionId.Kamer).LowTable),
            "version-twenty-eight override inherits stock Kamer OAM without ROM reads");
        var preFuneFrames = preDraygonFrames
            .Where(pair => !pair.Key.StartsWith("fune_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("namihe_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("kamer_platform_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("elevator_platform_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreFuneNamiheFrameCount,
            preFuneFrames.Count, "pre-Fune/Namihe composition schema frame count");
        var preFuneBindings = document.DisplayFrames
            .Where(pair => preFuneFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreFuneNamihe,
                Frames = preFuneFrames,
                DisplayFrames = preFuneBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preFuneUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer inheritedFune = DrawEnemy(preFuneUpgraded,
            NewFrameReadGuard(rom), 0x94cb,
            EnemyDefinitionId.Fune);
        AssertTrue(inheritedFune.LowTable.SequenceEqual(stockFune.LowTable),
            "version-twenty-seven override inherits stock Fune OAM without ROM reads");
        AssertTrue(DrawEnemy(preFuneUpgraded, NewFrameReadGuard(rom), 0x88da,
                EnemyDefinitionId.Boyon).LowTable
            .SequenceEqual(DrawEnemy(edited, NewFrameReadGuard(rom), 0x88da,
                EnemyDefinitionId.Boyon).LowTable),
            "version-twenty-seven override retains existing Boyon edits");
        var preSbugFrames = preDraygonFrames
            .Where(pair => !pair.Key.StartsWith("sbug_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("fune_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("namihe_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("kamer_platform_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("elevator_platform_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreSbugFrameCount,
            preSbugFrames.Count, "pre-Sbug composition schema frame count");
        var preSbugBindings = document.DisplayFrames
            .Where(pair => preSbugFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preSbugBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreSbug,
                Frames = preSbugFrames,
                DisplayFrames = preSbugBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preSbugUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        AssertTrue((preSbugUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires preSbugUpgraded.Spritemaps.")).TryGet(
                EnemySpritemapDefinitions.SbugBank, sbugPointer, out _),
            "version-twenty-six override inherits stock Sbug artwork");
        AssertTrue(DrawEnemy(preSbugUpgraded, NewFrameReadGuard(rom),
                0x88da, EnemyDefinitionId.Boyon).LowTable
            .SequenceEqual(DrawEnemy(stock, NewFrameReadGuard(rom),
                0x88e1, EnemyDefinitionId.Boyon).LowTable),
            "version-twenty-six override retains edited display binding");
        var preHZoomerFrames = preDraygonFrames
            .Where(pair => !pair.Key.StartsWith("hzoomer_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("sbug_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("fune_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("namihe_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("kamer_platform_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("elevator_platform_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreHZoomerFrameCount,
            preHZoomerFrames.Count, "pre-HZoomer composition schema frame count");
        var preHZoomerBindings = document.DisplayFrames
            .Where(pair => preHZoomerFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preHZoomerBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreHZoomer,
                Frames = preHZoomerFrames,
                DisplayFrames = preHZoomerBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preHZoomerUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer retainedPreHZoomerEdit = DrawEnemy(preHZoomerUpgraded,
            NewFrameReadGuard(rom), chootPointer, EnemyDefinitionId.Choot);
        AssertEqual(editedChoot.LowTable[1], retainedPreHZoomerEdit.LowTable[1],
            "version-twenty-five override retains edited Choot artwork");
        AssertTrue(DrawEnemy(preHZoomerUpgraded, NewFrameReadGuard(rom), 0x88da,
                EnemyDefinitionId.Boyon).LowTable
            .SequenceEqual(DrawEnemy(stock, NewFrameReadGuard(rom), 0x88e1,
                EnemyDefinitionId.Boyon).LowTable),
            "version-twenty-five override retains edited display binding");
        AssertTrue((preHZoomerUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires preHZoomerUpgraded.Spritemaps.")).TryGet(
                EnemySpritemapDefinitions.HZoomerBank, hzoomerPointer, out _),
            "version-twenty-five override gains stock HZoomer artwork");
        var preChootFrames = preHZoomerFrames
            .Where(pair => !pair.Key.StartsWith("choot_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreChootFrameCount,
            preChootFrames.Count, "pre-Choot composition schema frame count");
        var preChootBindings = preHZoomerBindings
            .Where(pair => preChootFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preChootBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreChoot,
                Frames = preChootFrames,
                DisplayFrames = preChootBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preChootUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer retainedPreChootEdit = DrawEnemy(preChootUpgraded,
            NewFrameReadGuard(rom), hopperPointer, EnemyDefinitionId.Sidehopper);
        AssertEqual(editedHopper.LowTable[1], retainedPreChootEdit.LowTable[1],
            "version-twenty-four override retains edited Sidehopper artwork");
        AssertTrue(DrawEnemy(preChootUpgraded, NewFrameReadGuard(rom), 0x88da,
                EnemyDefinitionId.Boyon).LowTable
            .SequenceEqual(DrawEnemy(stock, NewFrameReadGuard(rom), 0x88e1,
                EnemyDefinitionId.Boyon).LowTable),
            "version-twenty-four override retains edited display binding");
        AssertTrue((preChootUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires preChootUpgraded.Spritemaps.")).TryGet(
                EnemySpritemapDefinitions.ChootBank, chootPointer, out _),
            "version-twenty-four override gains stock Choot artwork");
        var preHopperFrames = preChootFrames
            .Where(pair => !pair.Key.StartsWith("sidehopper_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("dessgeega_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("large_sidehopper_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("large_dessgeega_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreHopperFrameCount,
            preHopperFrames.Count, "pre-Hopper composition schema frame count");
        var preHopperBindings = preChootBindings
            .Where(pair => preHopperFrames.ContainsKey(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preHopperBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreHopper,
                Frames = preHopperFrames,
                DisplayFrames = preHopperBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preHopperUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer retainedPreHopperEdit = DrawEnemy(preHopperUpgraded,
            NewFrameReadGuard(rom), beetomPointer, EnemyDefinitionId.Beetom);
        AssertEqual(editedBeetom.LowTable[1], retainedPreHopperEdit.LowTable[1],
            "version-twenty-three override retains edited Beetom artwork");
        AssertTrue(DrawEnemy(preHopperUpgraded, NewFrameReadGuard(rom), 0x88da,
                EnemyDefinitionId.Boyon).LowTable
            .SequenceEqual(DrawEnemy(stock, NewFrameReadGuard(rom), 0x88e1,
                EnemyDefinitionId.Boyon).LowTable),
            "version-twenty-three override retains edited display binding");
        AssertTrue((preHopperUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires preHopperUpgraded.Spritemaps.")).TryGet(
                EnemySpritemapDefinitions.HopperBank, hopperPointer, out _),
            "version-twenty-three override gains stock Hopper artwork");
        var preBeetomFrames = preHopperFrames
            .Where(pair => !pair.Key.StartsWith("beetom_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreBeetomFrameCount,
            preBeetomFrames.Count, "pre-Beetom composition schema frame count");
        var preBeetomBindings = preHopperBindings
            .Where(pair => !pair.Key.StartsWith("beetom_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preBeetomBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreBeetom,
                Frames = preBeetomFrames,
                DisplayFrames = preBeetomBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preBeetomUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer retainedPreBeetomEdit = DrawEnemy(preBeetomUpgraded,
            NewFrameReadGuard(rom), alcoonPointer, EnemyDefinitionId.Alcoon);
        AssertEqual(editedAlcoon.LowTable[1], retainedPreBeetomEdit.LowTable[1],
            "version-twenty-two override retains an edited Alcoon frame");
        AssertTrue(DrawEnemy(preBeetomUpgraded, NewFrameReadGuard(rom), 0x88da,
                EnemyDefinitionId.Boyon).LowTable
            .SequenceEqual(DrawEnemy(stock, NewFrameReadGuard(rom), 0x88e1,
                EnemyDefinitionId.Boyon).LowTable),
            "version-twenty-two override retains its edited display binding");
        AssertTrue((preBeetomUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires preBeetomUpgraded.Spritemaps.")).TryGet(
                EnemySpritemapDefinitions.BeetomBank, beetomPointer, out _),
            "version-twenty-two override gains stock Beetom artwork");
        var preAlcoonFrames = preBeetomFrames
            .Where(pair => !pair.Key.StartsWith("alcoon_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreAlcoonFrameCount,
            preAlcoonFrames.Count, "pre-Alcoon composition schema frame count");
        var preAlcoonBindings = preBeetomBindings
            .Where(pair => !pair.Key.StartsWith("alcoon_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreAlcoon,
                Frames = preAlcoonFrames,
                DisplayFrames = preAlcoonBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preAlcoonUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer retainedPreAlcoonEdit = DrawEnemy(preAlcoonUpgraded,
            NewFrameReadGuard(rom), bullPointer, EnemyDefinitionId.Bull);
        AssertEqual(editedBull.LowTable[1], retainedPreAlcoonEdit.LowTable[1],
            "version-twenty-one override retains an edited Bull frame");
        AssertTrue((preAlcoonUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires preAlcoonUpgraded.Spritemaps.")).TryGet(
                EnemySpritemapDefinitions.AlcoonBank, alcoonPointer, out _),
            "version-twenty-one override gains stock Alcoon artwork");
        var preBullFrames = preAlcoonFrames
            .Where(pair => !pair.Key.StartsWith("bull_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreBullFrameCount,
            preBullFrames.Count, "pre-Bull composition schema frame count");
        var preBullBindings = preAlcoonBindings
            .Where(pair => !pair.Key.StartsWith("bull_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreBull,
                Frames = preBullFrames,
                DisplayFrames = preBullBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preBullUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer retainedPreBullEdit = DrawEnemy(preBullUpgraded,
            NewFrameReadGuard(rom), puyoPointer, EnemyDefinitionId.Puyo);
        AssertEqual(editedPuyo.LowTable[1], retainedPreBullEdit.LowTable[1],
            "version-twenty override retains an edited Puyo frame");
        AssertTrue((preBullUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires preBullUpgraded.Spritemaps.")).TryGet(
                EnemySpritemapDefinitions.BullBank, bullPointer, out _),
            "version-twenty override gains stock Bull artwork");
        var prePuyoFrames = preBullFrames
            .Where(pair => !pair.Key.StartsWith("puyo_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PrePuyoFrameCount,
            prePuyoFrames.Count, "pre-Puyo composition schema frame count");
        var prePuyoBindings = preBullBindings
            .Where(pair => !pair.Key.StartsWith("puyo_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PrePuyo,
                Frames = prePuyoFrames,
                DisplayFrames = prePuyoBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog prePuyoUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer retainedPrePuyoEdit = DrawEnemy(prePuyoUpgraded,
            NewFrameReadGuard(rom), framePointer, EnemyDefinitionId.Boyon);
        AssertEqual(editedOam.LowTable[1], retainedPrePuyoEdit.LowTable[1],
            "version-nineteen override retains an edited older enemy frame");
        AssertTrue((prePuyoUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires prePuyoUpgraded.Spritemaps.")).TryGet(
                EnemySpritemapDefinitions.PuyoBank, puyoPointer, out _),
            "version-nineteen override gains stock Puyo artwork");
        var preNorfairRioFrames = prePuyoFrames
            .Where(pair => !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreNorfairRioFrameCount,
            preNorfairRioFrames.Count, "pre-Norfair-Rio composition schema frame count");
        var preNorfairRioBindings = prePuyoBindings
            .Where(pair => !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preNorfairRioBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreNorfairRio,
                Frames = preNorfairRioFrames,
                DisplayFrames = preNorfairRioBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preNorfairRioUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer priorNorfairRioEraRemap = DrawEnemy(preNorfairRioUpgraded,
            NewFrameReadGuard(rom), 0x88da, EnemyDefinitionId.Boyon);
        OamBuffer priorNorfairRioEraSelected = DrawEnemy(stock,
            NewFrameReadGuard(rom), 0x88e1, EnemyDefinitionId.Boyon);
        AssertTrue(priorNorfairRioEraSelected.LowTable.SequenceEqual(
                priorNorfairRioEraRemap.LowTable),
            "version-eighteen override retains its display binding");
        AssertTrue((preNorfairRioUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires preNorfairRioUpgraded.Spritemaps.")).TryGet(
                EnemySpritemapDefinitions.NorfairRioBank, norfairRioPointer, out _),
            "version-eighteen override gains stock Norfair Rio artwork");
        var preLowerRioFrames = prePuyoFrames
            .Where(pair => !pair.Key.StartsWith("lower_norfair_rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreLowerNorfairRioFrameCount,
            preLowerRioFrames.Count, "pre-Lower-Norfair-Rio composition schema frame count");
        var preLowerRioBindings = prePuyoBindings
            .Where(pair => !pair.Key.StartsWith("lower_norfair_rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preLowerRioBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreLowerNorfairRio,
                Frames = preLowerRioFrames,
                DisplayFrames = preLowerRioBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preLowerRioUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer priorLowerRioEraRemap = DrawEnemy(preLowerRioUpgraded,
            NewFrameReadGuard(rom), 0x88da, EnemyDefinitionId.Boyon);
        OamBuffer priorLowerRioEraSelected = DrawEnemy(stock,
            NewFrameReadGuard(rom), 0x88e1, EnemyDefinitionId.Boyon);
        AssertTrue(priorLowerRioEraSelected.LowTable.SequenceEqual(
                priorLowerRioEraRemap.LowTable),
            "version-seventeen override retains its display binding");
        AssertTrue((preLowerRioUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires preLowerRioUpgraded.Spritemaps.")).TryGet(
                EnemySpritemapDefinitions.LowerNorfairRioBank, lowerRioPointer, out _),
            "version-seventeen override gains stock Lower Norfair Rio artwork");
        var preRioFrames = prePuyoFrames
            .Where(pair => !pair.Key.StartsWith("rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("lower_norfair_rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreRioFrameCount,
            preRioFrames.Count, "pre-Rio composition schema frame count");
        var preRioBindings = prePuyoBindings
            .Where(pair => !pair.Key.StartsWith("rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("lower_norfair_rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preRioBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreRio,
                Frames = preRioFrames,
                DisplayFrames = preRioBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preRioUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer priorRioEraRemap = DrawEnemy(preRioUpgraded,
            NewFrameReadGuard(rom), 0x88da, EnemyDefinitionId.Boyon);
        OamBuffer priorRioEraSelected = DrawEnemy(stock,
            NewFrameReadGuard(rom), 0x88e1, EnemyDefinitionId.Boyon);
        AssertTrue(priorRioEraSelected.LowTable.SequenceEqual(priorRioEraRemap.LowTable),
            "version-sixteen override retains its display binding");
        AssertTrue((preRioUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires preRioUpgraded.Spritemaps.")).TryGet(
                EnemySpritemapDefinitions.RioBank, rioPointer, out _),
            "version-sixteen override gains stock Rio artwork");
        var preCeresBabyFrames = prePuyoFrames
            .Where(pair => !pair.Key.StartsWith("ceres_baby_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("lower_norfair_rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreCeresBabyFrameCount,
            preCeresBabyFrames.Count, "pre-Ceres-Baby composition schema frame count");
        var preCeresBabyBindings = prePuyoBindings
            .Where(pair => !pair.Key.StartsWith("ceres_baby_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("lower_norfair_rio_", StringComparison.Ordinal) &&
                !pair.Key.StartsWith("norfair_rio_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preCeresBabyBindings["ceres_door_right_hold"] =
            "ceres_door_right_transition_0";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreCeresBaby,
                Frames = preCeresBabyFrames,
                DisplayFrames = preCeresBabyBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preCeresBabyUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer priorCeresRemap = DrawEnemy(preCeresBabyUpgraded,
            NewFrameReadGuard(rom), 0xfa13,
            EnemyDefinitionId.CeresDoor);
        OamBuffer priorCeresSelected = DrawEnemy(stock, NewFrameReadGuard(rom),
            0xfa3d, EnemyDefinitionId.CeresDoor);
        AssertTrue(priorCeresSelected.LowTable.SequenceEqual(priorCeresRemap.LowTable),
            "version-fifteen override retains its Ceres door display binding");
        AssertTrue((preCeresBabyUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires preCeresBabyUpgraded.Spritemaps.")).TryGet(
                CeresBabyFrameDefinitions.Bank,
                CeresBabyFrameDefinitions.RoundFrame, out _),
            "version-fifteen override gains stock Ceres Baby artwork");

        var preCeresDoorFrames = preCeresBabyFrames
            .Where(pair => !pair.Key.StartsWith("ceres_door_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreCeresDoorFrameCount,
            preCeresDoorFrames.Count, "pre-Ceres-door composition schema frame count");
        var preCeresDoorBindings = preCeresBabyBindings
            .Where(pair => !pair.Key.StartsWith("ceres_door_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        preCeresDoorBindings["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreCeresDoor,
                Frames = preCeresDoorFrames,
                DisplayFrames = preCeresDoorBindings,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preCeresDoorUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer priorRemap = DrawEnemy(preCeresDoorUpgraded, NewFrameReadGuard(rom),
            0x88da, EnemyDefinitionId.Boyon);
        OamBuffer priorSelected = DrawEnemy(stock, NewFrameReadGuard(rom),
            0x88e1, EnemyDefinitionId.Boyon);
        AssertTrue(priorSelected.LowTable.SequenceEqual(priorRemap.LowTable),
            "version-fourteen override retains its edited display binding");
        OamBuffer priorEditedCacatac = DrawEnemy(preCeresDoorUpgraded,
            NewFrameReadGuard(rom), cacatacPointer, EnemyDefinitionId.Cacatac);
        AssertTrue(editedCacatac.LowTable.SequenceEqual(priorEditedCacatac.LowTable),
            "version-fourteen override also retains its edited frame parts");
        AssertTrue((preCeresDoorUpgraded.Spritemaps ?? throw new InvalidOperationException("Verification requires preCeresDoorUpgraded.Spritemaps.")).TryGet(
                CeresDoorFrameDefinitions.Bank, 0xfa13, out _),
            "version-fourteen override gains stock Ceres door artwork");
        preCeresDoorBindings.Remove("boyon_idle_0");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreCeresDoor,
                Frames = preCeresDoorFrames,
                DisplayFrames = preCeresDoorBindings,
            }, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadSpritemapOverride(),
            "version-fourteen override rejects a missing authored display binding");

        var preMagdolliteFrames = preCeresDoorFrames
            .Where(pair => !pair.Key.StartsWith("magdollite_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreMagdolliteFrameCount,
            preMagdolliteFrames.Count, "pre-Magdollite composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreMagdollite,
                Frames = preMagdolliteFrames,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preMagdolliteUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer upgradedMagdollite = DrawEnemy(preMagdolliteUpgraded,
            NewFrameReadGuard(rom), magdollitePointer,
            EnemyDefinitionId.Magdollite);
        AssertTrue(stockMagdollite.LowTable.SequenceEqual(upgradedMagdollite.LowTable),
            "version-twelve override gains stock Magdollite composition");
        OamBuffer retainedFireflea = DrawEnemy(preMagdolliteUpgraded,
            NewFrameReadGuard(rom), firefleaPointer,
            EnemyDefinitionId.Fireflea);
        AssertEqual(editedFireflea.LowTable[1], retainedFireflea.LowTable[1],
            "version-twelve override retains edited Fireflea composition");
        var preFirefleaFrames = preMagdolliteFrames
            .Where(pair => !pair.Key.StartsWith("fireflea_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreFirefleaFrameCount,
            preFirefleaFrames.Count, "pre-Fireflea composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreFireflea,
                Frames = preFirefleaFrames,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preFirefleaUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer upgradedFireflea = DrawEnemy(preFirefleaUpgraded,
            NewFrameReadGuard(rom), firefleaPointer, EnemyDefinitionId.Fireflea);
        AssertTrue(stockFireflea.LowTable.SequenceEqual(upgradedFireflea.LowTable),
            "version-eleven override gains stock Fireflea composition");
        OamBuffer retainedRipper = DrawEnemy(preFirefleaUpgraded,
            NewFrameReadGuard(rom), 0xe54b, EnemyDefinitionId.Ripper);
        OamBuffer editedRipper = DrawEnemy(edited,
            NewFrameReadGuard(rom), 0xe54b, EnemyDefinitionId.Ripper);
        AssertEqual(editedRipper.LowTable[1], retainedRipper.LowTable[1],
            "version-eleven override retains edited Ripper composition");
        var preRipperFrames = preFirefleaFrames
            .Where(pair => !pair.Key.StartsWith("ripper_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreRipperFrameCount,
            preRipperFrames.Count, "pre-Ripper composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreRipper,
                Frames = preRipperFrames,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preRipperUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer upgradedRipper = DrawEnemy(preRipperUpgraded, NewFrameReadGuard(rom),
            0xe54b, EnemyDefinitionId.Ripper);
        OamBuffer stockRipper = DrawEnemy(stock, NewFrameReadGuard(rom),
            0xe54b, EnemyDefinitionId.Ripper);
        AssertTrue(stockRipper.LowTable.SequenceEqual(upgradedRipper.LowTable),
            "version-ten override gains stock Ripper composition");
        OamBuffer retainedStoke = DrawEnemy(preRipperUpgraded,
            NewFrameReadGuard(rom), 0x8aca, EnemyDefinitionId.Stoke);
        OamBuffer editedStoke = DrawEnemy(edited, NewFrameReadGuard(rom),
            0x8aca, EnemyDefinitionId.Stoke);
        AssertEqual(editedStoke.LowTable[1], retainedStoke.LowTable[1],
            "version-ten override retains edited Stoke composition");
        var preOwtchStokeFrames = preRipperFrames
            .Where(pair => !pair.Key.StartsWith("owtch_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("stoke_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreOwtchStokeFrameCount,
            preOwtchStokeFrames.Count, "pre-Owtch/Stoke composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.PreOwtchStoke,
                Frames = preOwtchStokeFrames,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog preOwtchStokeUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        ushort owtchPointer = OwtchAnimationsConstants.Spritemaps(OwtchProgram.MovingLeftLoop).First();
        OamBuffer stockOwtch = DrawEnemy(stock, NewFrameReadGuard(rom),
            owtchPointer, EnemyDefinitionId.Owtch);
        OamBuffer upgradedOwtch = DrawEnemy(preOwtchStokeUpgraded,
            NewFrameReadGuard(rom), owtchPointer, EnemyDefinitionId.Owtch);
        AssertTrue(stockOwtch.LowTable.SequenceEqual(upgradedOwtch.LowTable),
            "version-nine override gains stock Owtch composition");
        var previousFrames = preOwtchStokeFrames
            .Where(pair => !pair.Key.StartsWith("fake_kraid_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("kraid_nail_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreviousFrameCount,
            previousFrames.Count, "previous enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.Previous,
                Frames = previousFrames,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog upgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        ushort nailPointer = KraidVisualDefinitions.InitialNailFrame;
        OamBuffer stockNail = DrawEnemy(stock, NewFrameReadGuard(rom),
            nailPointer, EnemyDefinitionId.KraidNail);
        OamBuffer upgradedNail = DrawEnemy(upgraded, NewFrameReadGuard(rom),
            nailPointer, EnemyDefinitionId.KraidNail);
        AssertTrue(stockNail.LowTable.SequenceEqual(upgradedNail.LowTable),
            "previous-version override gains stock Kraid fingernail composition");
        OamBuffer retainedWaver = DrawEnemy(upgraded, NewFrameReadGuard(rom),
            waverPointer, EnemyDefinitionId.Waver);
        AssertEqual(editedWaver.LowTable[1], retainedWaver.LowTable[1],
            "previous-version override retains edited Waver composition");
        ushort retainedPipePointer = PipeBugAnimationsConstants.Spritemaps(PipeBugProgram.GametRisingLeft).First();
        OamBuffer editedPipe = DrawEnemy(edited, NewFrameReadGuard(rom),
            retainedPipePointer, EnemyDefinitionId.Gamet);
        OamBuffer retainedPipe = DrawEnemy(upgraded, NewFrameReadGuard(rom),
            retainedPipePointer, EnemyDefinitionId.Gamet);
        AssertEqual(editedPipe.LowTable[1], retainedPipe.LowTable[1],
            "previous-version override retains edited Pipe Bug composition");
        var priorFrames = previousFrames
            .Where(pair => !pair.Key.StartsWith("pipe_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PriorFrameCount,
            priorFrames.Count, "prior enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.Prior,
                Frames = priorFrames,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog priorUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        ushort pipePointer = PipeBugAnimationsConstants.Spritemaps(PipeBugProgram.GametRisingLeft).First();
        OamBuffer stockPipe = DrawEnemy(stock, NewFrameReadGuard(rom),
            pipePointer, EnemyDefinitionId.Gamet);
        OamBuffer upgradedPipe = DrawEnemy(priorUpgraded, NewFrameReadGuard(rom),
            pipePointer, EnemyDefinitionId.Gamet);
        AssertTrue(stockPipe.LowTable.SequenceEqual(upgradedPipe.LowTable),
            "prior-version override gains stock Pipe Bug composition");
        var earlierFrames = priorFrames
            .Where(pair => !pair.Key.StartsWith("zoa_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("metaree_", StringComparison.Ordinal) &&
                           !pair.Key.StartsWith("skree_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.EarlierFrameCount,
            earlierFrames.Count, "earlier enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.Earlier,
                Frames = earlierFrames,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog earlierUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        var upgradedBoyon = DrawEnemy(earlierUpgraded, NewFrameReadGuard(rom),
            framePointer, EnemyDefinitionId.Boyon);
        var upgradedSkultera = DrawEnemy(earlierUpgraded, NewFrameReadGuard(rom),
            skulteraPointer, EnemyDefinitionId.Skultera);
        AssertEqual(editedOam.LowTable[0], upgradedBoyon.LowTable[0],
            "previous-version override retains edited Boyon composition");
        AssertEqual(editedSkultera.LowTable[1], upgradedSkultera.LowTable[1],
            "previous-version override retains edited Skultera composition");
        var upgradedWaver = DrawEnemy(earlierUpgraded, NewFrameReadGuard(rom),
            waverPointer, EnemyDefinitionId.Waver);
        AssertEqual(editedWaver.LowTable[1], upgradedWaver.LowTable[1],
            "previous-version override retains edited Waver composition");
        ushort zoaPointer = ZoaAnimationsConstants.Spritemaps(ZoaProgram.FacingLeftShooting).First();
        var stockZoa = DrawEnemy(stock, NewFrameReadGuard(rom),
            zoaPointer, EnemyDefinitionId.Zoa);
        var upgradedZoa = DrawEnemy(earlierUpgraded, NewFrameReadGuard(rom),
            zoaPointer, EnemyDefinitionId.Zoa);
        AssertTrue(stockZoa.LowTable.SequenceEqual(upgradedZoa.LowTable),
            "previous-version override gains stock Zoa composition");
        var intermediateFrames = earlierFrames
            .Where(pair => !pair.Key.StartsWith("waver_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.IntermediateFrameCount,
            intermediateFrames.Count, "intermediate enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.Intermediate,
                Frames = intermediateFrames,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog intermediateUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        var intermediateBoyon = DrawEnemy(intermediateUpgraded, NewFrameReadGuard(rom),
            framePointer, EnemyDefinitionId.Boyon);
        var intermediateWaver = DrawEnemy(intermediateUpgraded, NewFrameReadGuard(rom),
            waverPointer, EnemyDefinitionId.Waver);
        AssertEqual(editedOam.LowTable[0], intermediateBoyon.LowTable[0],
            "intermediate override retains edited Boyon composition");
        AssertTrue(stockWaver.LowTable.SequenceEqual(intermediateWaver.LowTable),
            "intermediate override gains stock Waver composition");
        var legacyFrames = intermediateFrames
            .Where(pair => !pair.Key.StartsWith("skultera_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.LegacyFrameCount,
            legacyFrames.Count, "legacy enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = (int)EnemySpritemapSchema.Legacy,
                Frames = legacyFrames,
            }, CamelCaseJson));
        EnemyTileArtworkCatalog legacyUpgraded = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        var legacyBoyon = DrawEnemy(legacyUpgraded, NewFrameReadGuard(rom),
            framePointer, EnemyDefinitionId.Boyon);
        var legacySkultera = DrawEnemy(legacyUpgraded, NewFrameReadGuard(rom),
            skulteraPointer, EnemyDefinitionId.Skultera);
        AssertEqual(editedOam.LowTable[0], legacyBoyon.LowTable[0],
            "legacy override retains edited Boyon composition");
        AssertTrue(stockSkultera.LowTable.SequenceEqual(legacySkultera.LowTable),
            "legacy override gains stock Skultera composition");
        var preBindings = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.PreDisplayBindings,
            Frames = preCeresDoorFrames,
        };
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            preBindings, CamelCaseJson));
        EnemyTileArtworkCatalog upgradedBindings = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        AssertEqual(editedOam.LowTable[0],
            DrawEnemy(upgradedBindings, NewFrameReadGuard(rom), framePointer,
                EnemyDefinitionId.Boyon).LowTable[0],
            "version-thirteen art override retains edits with stock display bindings");

        // A binding may change only the presentation frame. The native pointer,
        // instruction timer, and AI timer must remain untouched by drawing.
        EnemySpritemapDocument remapped = RequiredJson.Deserialize<EnemySpritemapDocument>(
            original, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        (remapped.DisplayFrames ?? throw new InvalidOperationException("Verification requires remapped.DisplayFrames."))["boyon_idle_0"] = "boyon_idle_1";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, CamelCaseJson));
        EnemyTileArtworkCatalog swappedArt = StockWithSpritemaps(stockDirectory, LoadSpritemapOverride());
        OamBuffer nativeFirst = DrawEnemy(stock, NewFrameReadGuard(rom), 0x88da,
            EnemyDefinitionId.Boyon);
        OamBuffer nativeSecond = DrawEnemy(stock, NewFrameReadGuard(rom), 0x88e1,
            EnemyDefinitionId.Boyon);
        OamBuffer displayedSecond = DrawEnemy(swappedArt, NewFrameReadGuard(rom),
            0x88da, EnemyDefinitionId.Boyon, slot =>
            {
                AssertEqual((ushort)0x88da, slot.SpritemapPointer,
                    "display override retains native collision frame pointer");
                AssertEqual((ushort)7, slot.InstructionTimer.RawValue,
                    "display override does not move an instruction frame boundary");
                AssertEqual((ushort)9, slot.Timer.RawValue,
                    "display override does not move an enemy AI timer");
            });
        AssertTrue(!nativeFirst.LowTable.SequenceEqual(nativeSecond.LowTable),
            "selected Boyon test frames are visually distinct");
        AssertTrue(nativeSecond.ImageEquals(displayedSecond),
            "authored frame binding changes live OAM without a ROM visual read");
        AssertTrue(LoadSpritemapOverride().TryGetDisplay(EnemySpritemapDefinitions.BoyonBank,
                    0x88da, out _),
            "display override survives a catalog reload");
        remapped.DisplayFrames["boyon_idle_0"] = "magdollite_left_idle_0";
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadSpritemapOverride(),
            "cross-bank display binding fails loudly");
        remapped.DisplayFrames.Remove("boyon_idle_0");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            remapped, CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => LoadSpritemapOverride(),
            "missing display binding fails loudly");
        File.WriteAllText(overridePath, "{\"version\":1,\"version\":1,\"frames\":{}}");
        AssertThrows<InvalidDataException>(
            () => LoadSpritemapOverride(),
            "duplicate enemy composition keys fail loudly");
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => LoadSpritemapOverride(),
            "malformed enemy composition override fails loudly");

        static OamBuffer DrawEnemy(EnemyTileArtworkCatalog art,
            ISnesAddressSpace guard, ushort pointer, EnemyDefinitionId definition,
            Action<RoomEnemySlot>? inspect = null, byte? spritemapBank = null)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art };
            enemies.Drawing.Enqueue(0, 0);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(definition, slot.Definition);
            slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with
                { Bank = definition == EnemyDefinitionId.YappingMaw
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
                        ? CeresDoorFrameDefinitions.Bank
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
                        ? BotwoonVisualDefinitions.Bank
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
                        : EnemySpritemapDefinitions.BoyonBank });
            // The catalog-wide composition check includes families newer than the
            // fixture's actor-name mapping. Its frame identity owns the actual bank.
            if (spritemapBank is byte bank)
                slot.Bind(slot.EnemyDefinitionPointer, slot.Definition with { Bank = bank });
            slot.SpritemapPointer = pointer;
            slot.InstructionTimer = NativeCountdown.Start(7);
            slot.Timer = NativeCountdown.Start(9);
            slot.Position = slot.Position.WithWhole(new(0x0040, 0x0080));
            var oam = new OamBuffer();
            enemies.Drawing.DrawLayersAsOam(oam, new(0, 0), 0, 0);
            inspect?.Invoke(slot);
            return oam;
        }
    }

    /// <summary>Checks Mother Brain's private draw hook against native head frames and above-screen culling.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static void VerifyInstalledMotherBrainDrawHook(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        foreach (EnemySpritemapDefinition frame in MotherBrainVisualDefinitions.Frames())
        {
            OamBuffer actual = DrawInstalledMotherBrainFrame(
                stock, frame.Pointer, new(0x0140, 0x00a0));
            var expected = new OamBuffer();
            DrawImportedEnemySpritemap(rom, expected, new NativeEnemySpritemapDraw { Bank = frame.Bank, Pointer = frame.Pointer, Origin = new(0x0040, 0x0090), PaletteBits = 0x0400, BaseTileIndex = 0x0010 });
            AssertTrue(actual.ImageEquals(expected) &&
                       actual.NextByteOffset == expected.NextByteOffset,
                $"Mother Brain private draw uses installed {frame.Name} with native OAM");
        }

        OamBuffer aboveScreen = DrawInstalledMotherBrainFrame(
            stock, 0xa586, new(0x0140, 0x000f));
        AssertEqual(0, aboveScreen.NextByteOffset,
            "Mother Brain private draw still culls a head above the viewport");
    }

    /// <summary>Draws one Mother Brain world frame through the installed private renderer under a cartridge-read guard.</summary>
    private static OamBuffer DrawInstalledMotherBrainFrame(
        EnemyTileArtworkCatalog art, ushort pointer,
        Position16<RoomPixels> world)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art };
        var oam = new OamBuffer();
        enemies.MotherBrainBehavior.DrawMotherBrainWorldSpritemap(oam, RoomEnemySlotFrameAccess.FrameAt(MotherBrainVisualDefinitions.Bank, pointer)
            ?? throw new InvalidOperationException("A Mother Brain draw shows a frame."), world, (ushort)0x0400, (ushort)0x0010, new Position16<RoomPixels>((ushort)0x0100, (ushort)0x0010));
        return oam;
    }

    /// <summary>Spawns and draws a room sprite object to inspect its installed OAM output.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    private static OamBuffer DrawRoomSpriteObject(EnemyTileArtworkCatalog art,
        RoomSpriteObjectKind kind)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art };
        Func<RoomEnemySystem, Position16<RoomPixels>, RoomSpriteObjectKind, ushort, RoomSpriteObjectSlot?> spawn =
            (owner, position, kind, graphicsIndex) => owner.SpriteObjects.SpawnRoomSpriteObject(position, kind, graphicsIndex);
        Action<RoomEnemySystem, OamBuffer, Position16<RoomPixels>> draw =
            (owner, oam, camera) => owner.SpriteObjects.DrawRoomSpriteObjectsAsOam(oam, camera);
        AssertTrue(spawn(enemies, new(0x0040, 0x0080), kind, 0x0e00) is not null,
            "installed room sprite object allocated");
        var oam = new OamBuffer();
        draw(enemies, oam, Position16<RoomPixels>.Zero);
        AssertTrue(oam.NextByteOffset > 0, "installed room sprite object drew OAM");
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
