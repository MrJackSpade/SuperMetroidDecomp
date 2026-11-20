using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.Metaree;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.Yard;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Confirms that the species entities answer the collision capabilities, private shot reactions and Ceres Ridley scene
/// drawing exactly as the shared definition-keyed catalog and the Ridley room's door-identity test did before those answers
/// moved onto the entities (#1312), with the private shot reactions in the definitions' shared-response form (#1759). The
/// expected tables below are the removed catalog's answers, written out per definition; every bindable definition is bound
/// through the production registry and asked through its entity.
/// </summary>
internal static class EnemySpeciesCapabilitiesVerification
{
    /// <summary>Both extended-spritemap walkers: Samus contact and projectile collision.</summary>
    private const EnemyCapabilities BothWalkers =
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>The ordinary Pirate definitions: the wall, walking and ninja families in every colour.</summary>
    private static readonly EnemyDefinitionId[] OrdinaryPirates =
    [
        EnemyDefinitionId.PirateGreyWall, EnemyDefinitionId.PirateGoldWall, EnemyDefinitionId.PirateMagentaWall, EnemyDefinitionId.PirateSilverWall,
        EnemyDefinitionId.PirateGreyWalking, EnemyDefinitionId.PirateGreenWalking, EnemyDefinitionId.PirateRedWalking,
        EnemyDefinitionId.PirateGoldWalking, EnemyDefinitionId.PirateMagentaWalking, EnemyDefinitionId.PirateSilverWalking,
        EnemyDefinitionId.PirateGoldNinja, EnemyDefinitionId.PirateSilverNinja,
    ];

    /// <summary>Crocomire's body and tongue, whose hitboxes name Crocomire's own shot reactions.</summary>
    private static readonly EnemyDefinitionId[] CrocomireParts = [EnemyDefinitionId.Crocomire, EnemyDefinitionId.CrocomireTongue];

    /// <summary>The removed catalog's capabilities per definition; every definition not listed takes none.</summary>
    private static Dictionary<EnemyDefinitionId, EnemyCapabilities> ExpectedCapabilities()
    {
        var expected = new Dictionary<EnemyDefinitionId, EnemyCapabilities>
        {
            [EnemyDefinitionId.Oum] = BothWalkers,
            [EnemyDefinitionId.BombTorizo] = BothWalkers,
            [EnemyDefinitionId.GoldenTorizo] = BothWalkers,
            [EnemyDefinitionId.Crocomire] = BothWalkers,
            [EnemyDefinitionId.CrocomireTongue] = BothWalkers,
            [EnemyDefinitionId.SporeSpawn] = BothWalkers,
            [EnemyDefinitionId.CeresSteam] = BothWalkers,
            [EnemyDefinitionId.KraidArm] = BothWalkers,
            [EnemyDefinitionId.KraidFoot] = BothWalkers,
            [EnemyDefinitionId.DraygonBody] = BothWalkers,
            [EnemyDefinitionId.PhantoonBody] = EnemyCapabilities.SamusFrameHitboxes,
            [EnemyDefinitionId.Kraid] = EnemyCapabilities.ProjectileFrameHitboxes,
            [EnemyDefinitionId.Ridley] = EnemyCapabilities.ProjectileFrameHitboxes,
            [EnemyDefinitionId.MotherBrainBody] = EnemyCapabilities.ProjectileFrameHitboxes,
            [EnemyDefinitionId.MotherBrainHead] = EnemyCapabilities.ProjectileFrameHitboxes,
        };
        foreach (EnemyDefinitionId pirate in OrdinaryPirates)
            expected.Add(pirate, BothWalkers);
        return expected;
    }

    /// <summary>
    /// The private shot reactions each definition owns (#1759, #2558): the species' own reaction its definition names, and
    /// the species' own identities where its hitboxes name them. The removed catalog's named callbacks map one to one onto
    /// these: a species' definition-level callback is the reaction its definition names. Every definition not listed owns
    /// none.
    /// </summary>
    private static Dictionary<EnemyDefinitionId, EnemyReaction[]> ExpectedPrivateShotReactions()
    {
        var expected = new Dictionary<EnemyDefinitionId, EnemyReaction[]>
        {
            [EnemyDefinitionId.BabyTurtle] = [TurtleReactions.Shot],
            [EnemyDefinitionId.Owtch] = [OwtchReactions.Shot],
            [EnemyDefinitionId.Rinka] = [RinkaReactions.Shot],
            [EnemyDefinitionId.Oum] = [OumReactions.Shot],
            [EnemyDefinitionId.GrappleRipper] = [RipperReactions.Shot],
            [EnemyDefinitionId.JetRipper] = [RipperReactions.Shot],
            [EnemyDefinitionId.Dragon] = [DragonReactions.Shot],
            [EnemyDefinitionId.ShutterShootable] = [ShutterReactions.Shot],
            [EnemyDefinitionId.Kamer] = [ShutterReactions.Shot],
            [EnemyDefinitionId.ShutterDestroyable] = [ShutterReactions.Shot],
            [EnemyDefinitionId.Metaree] = [MetareeReactions.Shot],
            [EnemyDefinitionId.Fireflea] = [FirefleaReactions.Shot],
            [EnemyDefinitionId.Tripper] = [PlatformsReactions.TripperShot],
            [EnemyDefinitionId.Mochtroid] = [MochtroidReactions.Shot],
            [EnemyDefinitionId.Skree] = [SkreeReactions.Shot],
            [EnemyDefinitionId.Yard] = [YardReactions.Shot],
            [EnemyDefinitionId.Evir] = [EvirReactions.Shot],
            [EnemyDefinitionId.YappingMaw] = [YappingMawReactions.Shot],
            [EnemyDefinitionId.Kago] = [KagoReactions.Shot],
            [EnemyDefinitionId.Magdollite] = [MagdolliteReactions.Shot],
            [EnemyDefinitionId.Beetom] = [BeetomReactions.Shot],
            [EnemyDefinitionId.Powamp] = [PowampReactions.Shot],
            [EnemyDefinitionId.WorkRobot] = [WorkRobotReactions.Shot],
            [EnemyDefinitionId.UnpoweredWorkRobot] = [WorkRobotReactions.Shot],
            [EnemyDefinitionId.Bull] = [BullReactions.Shot],
            [EnemyDefinitionId.Spark] = [SparkReactions.Shot],
            [EnemyDefinitionId.FaceBlock] = [FaceBlockReactions.Shot],
            [EnemyDefinitionId.KihunterGreen] = [KiHunterReactions.Shot],
            [EnemyDefinitionId.KihunterYellow] = [KiHunterReactions.Shot],
            [EnemyDefinitionId.KihunterRed] = [KiHunterReactions.Shot],
            [EnemyDefinitionId.FakeKraid] = [KraidReactions.FakeKraidShot],
            [EnemyDefinitionId.Zebetite] = [ZebetiteReactions.Shot],
            [EnemyDefinitionId.CorpseTorizo] = [CorpseReactions.Shot],
            [EnemyDefinitionId.CorpseSidehopper] = [CorpseReactions.Shot],
            [EnemyDefinitionId.CorpseZoomer] = [CorpseReactions.Shot],
            [EnemyDefinitionId.CorpseRipper] = [CorpseReactions.Shot],
            [EnemyDefinitionId.CorpseSkree] = [CorpseReactions.Shot],
            [EnemyDefinitionId.DraygonBody] = [DraygonReactions.Shot, EnemyReaction.DudShot],
            [EnemyDefinitionId.SporeSpawn] = [SporeSpawnReactions.Shot, EnemyReaction.DudShot],
            [EnemyDefinitionId.PhantoonBody] = [PhantoonReactions.Shot],
            [EnemyDefinitionId.MotherBrainBody] = [MotherBrainReactions.BodyShot],
            [EnemyDefinitionId.MotherBrainHead] = [MotherBrainReactions.HeadShot],
            [EnemyDefinitionId.BombTorizo] = [TorizoReactions.Shot, TorizoReactions.StandUpSitDownShot],
            [EnemyDefinitionId.GoldenTorizo] = [TorizoReactions.GoldenShot, TorizoReactions.Shot, TorizoReactions.StandUpSitDownShot],
            [EnemyDefinitionId.Shaktool] = [ShaktoolReactions.Shot],
            [EnemyDefinitionId.KraidArm] = [KraidReactions.ArmShot],
            [EnemyDefinitionId.RidleyCeres] = [RidleyReactions.Shot],
            [EnemyDefinitionId.Ridley] = [RidleyReactions.Shot],
            [EnemyDefinitionId.BabyMetroid] = [BabyMetroidReactions.Shot],
            [EnemyDefinitionId.Botwoon] = [BotwoonReactions.Shot],
            [EnemyDefinitionId.Metroid] = [MetroidReactions.Shot],
        };
        foreach (EnemyDefinitionId pirate in OrdinaryPirates)
        {
            expected.Add(pirate,
            [
                SpacePirateReactions.Shot, SpacePirateReactions.GoldNinjaVulnerableHitboxShot,
                SpacePirateReactions.GoldNinjaInvincibleHitboxShot,
            ]);
        }
        foreach (EnemyDefinitionId part in CrocomireParts)
        {
            expected.Add(part,
            [
                CrocomireReactions.AbsorbedShot, CrocomireReactions.DustShot, CrocomireReactions.MouthShot,
            ]);
        }
        return expected;
    }

    /// <summary>Every reaction a shot can select: the shared ones and each species' own.</summary>
    private static readonly EnemyReaction[] ProbedReactions =
    [
        .. EnemyReactionConstants.Shared,
        BotwoonReactions.Touch, BotwoonReactions.Shot, BabyMetroidReactions.Touch, BabyMetroidReactions.Shot,
        BabyMetroidReactionsConstants.CutsceneTouch, RinkaReactions.Touch, RinkaReactions.Shot, ZebetiteReactions.Touch,
        ZebetiteReactions.Shot, BeetomReactions.Touch, BeetomReactions.Shot, BullReactions.Shot, CorpseReactions.Touch,
        CorpseReactions.Shot, DragonReactions.Touch, DragonReactions.Shot, EvirReactions.Touch, EvirReactions.Shot,
        FaceBlockReactions.Shot, FirefleaReactions.Touch, FirefleaReactions.Shot, KagoReactions.Shot, KiHunterReactions.Shot,
        MagdolliteReactions.Touch, MagdolliteReactions.Shot, TurtleReactions.Touch, TurtleReactions.Shot,
        MetareeReactions.Shot, MetroidReactions.Touch, MetroidReactions.Shot, MochtroidReactions.Touch,
        MochtroidReactions.Shot, OwtchReactions.Shot, PlatformsReactions.TripperShot, PowampReactions.Touch,
        PowampReactions.Shot, RipperReactions.Shot, ShaktoolReactions.Touch, ShaktoolReactions.Shot,
        ShutterReactions.Touch, ShutterReactions.Shot, SkreeReactions.Shot, SparkReactions.Shot, WorkRobotReactions.Touch,
        WorkRobotReactions.Shot, YappingMawReactions.Touch, YappingMawReactions.Shot, YardReactions.Touch,
        YardReactions.Shot, CrocomireReactionsConstants.Shot, KraidReactions.NailTouch, KraidReactions.FakeKraidTouch,
        KraidReactions.FakeKraidShot,
        CeresSteamReactions.Touch,
        CrocomireReactions.ClawTouch, CrocomireReactions.AbsorbedShot, CrocomireReactions.DustShot,
        CrocomireReactions.MouthShot,
        DraygonReactions.Touch, DraygonReactions.Shot,
        KraidReactions.BodyTouch, KraidReactions.ArmTouch, KraidReactions.ArmShot,
        MotherBrainReactions.BodyShot, MotherBrainReactions.HeadShot, MotherBrainReactions.HeadTouch,
        PhantoonReactions.Touch, PhantoonReactions.Shot,
        RidleyReactions.FrameHitboxTouch, RidleyReactions.Shot,
        SporeSpawnReactions.Touch, SporeSpawnReactions.Shot,
        TorizoReactions.Touch, TorizoReactions.Shot, TorizoReactions.StandUpSitDownShot, TorizoReactions.GoldenShot,
        OumReactions.DamagingTouch, OumReactions.NonDamagingTouch, OumReactions.Shot,
        SpacePirateReactions.Touch, SpacePirateReactions.Shot, SpacePirateReactions.GoldNinjaVulnerableHitboxShot,
        SpacePirateReactions.GoldNinjaInvincibleHitboxShot,
    ];

    /// <summary>
    /// Binds every room and auxiliary definition, and every definition the expected tables name, to one slot of a production
    /// enemy system, then compares its entity's capabilities, its answer for every named callback and every probed callback
    /// pointer, and its Ceres Ridley scene drawing with the hand-drawing flag clear and set, against the expected tables.
    /// </summary>
    internal static void VerifyEnemySpeciesCapabilities()
    {
        Dictionary<EnemyDefinitionId, EnemyCapabilities> capabilities = ExpectedCapabilities();
        Dictionary<EnemyDefinitionId, EnemyReaction[]> privateReactions = ExpectedPrivateShotReactions();
        IEnumerable<EnemyDefinitionId> definitions = RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Id)
            .Concat(RoomEnemyAuxiliaryDefinitionCatalog.Definitions.Keys)
            .Concat(capabilities.Keys)
            .Concat(privateReactions.Keys)
            .Distinct();

        var enemies = EnemySystemFixtures.PlacingAnyEnemy(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            services: new VerificationRoomEnemyServices(null));
        // The powered work robot's definition decides its power as it is placed (#2179); either power answers alike.
        enemies.VerificationServices().IsAreaBossDefeated = () => true;
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[1];
        int bound = 0;
        int answers = 0;
        foreach (EnemyDefinitionId definition in definitions)
        {
            LeadingPartPlacement.Prepare(place, definition);
            place.Bind(definition, HeaderOf(definition));
            IEnemyEntity entity = place.Entity ?? throw new InvalidOperationException($"{definition} bound no entity.");
            bound++;

            AssertEqual(capabilities.GetValueOrDefault(definition, EnemyCapabilities.None), entity.Capabilities,
                $"{definition} collision capabilities");

            EnemyReaction[] owned = privateReactions.GetValueOrDefault(definition, []);
            foreach (EnemyReaction reaction in ProbedReactions)
            {
                bool expected = owned.Contains(reaction);
                bool actual = entity is IEnemyPrivateShotReaction owner && owner.OwnsPrivateShotReaction(reaction);
                AssertEqual(expected, actual, $"{definition} owns private shot reaction {reaction}");
                answers++;
            }

            var doorState = new CeresRidleyRoomDoorState();
            place.SetSpeciesState(doorState);
            foreach (CeresRidleyRoomDoorLook look in new[] { CeresRidleyRoomDoorLook.Actor, CeresRidleyRoomDoorLook.Overlay })
            {
                doorState.Look = look;
                bool showsOverlay = entity is CeresRidleyRoomDoorEntity door && door.Look == CeresRidleyRoomDoorLook.Overlay;
                AssertEqual(definition == EnemyDefinitionId.CeresRidleyRoomDoor && look == CeresRidleyRoomDoorLook.Overlay, showsOverlay,
                    $"{definition} shows the room door's overlay when the door's state says {look}");
            }
        }

        Console.WriteLine($"Enemy species capabilities: {bound} bound definitions answer the removed catalog's collision " +
            $"capabilities, {answers} private shot callback queries and Ceres Ridley scene drawing through their entities.");
    }

    /// <summary>Runs the species capability equivalence check.</summary>
    [VerificationCommand("--enemy-species-capabilities")]
    internal static int EnemySpeciesCapabilitiesCommand()
    {
        Suite(nameof(VerifyEnemySpeciesCapabilities), () => VerifyEnemySpeciesCapabilities());
        return 0;
    }

    /// <summary>The definition's retail header, or the empty header for a definition no catalog pins.</summary>
    /// <param name="definition">The definition to bind.</param>
    private static RoomEnemyDefinition HeaderOf(EnemyDefinitionId definition)
    {
        if (RoomEnemyAuxiliaryDefinitionCatalog.TryGet(definition, out RoomEnemyDefinition auxiliary))
            return auxiliary;
        foreach ((EnemyDefinitionId pointer, RoomEnemyDefinition header) in RoomEnemyDefinitionCatalog.Definitions)
        {
            if (pointer == definition)
                return header;
        }
        return default;
    }
}
