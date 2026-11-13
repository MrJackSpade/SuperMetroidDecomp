using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Corpses;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Confirms that the species entities answer the collision capabilities, private shot callbacks and Ceres Ridley scene
/// drawing exactly as the shared definition-keyed catalog and the Ridley room's door-identity test did before those answers
/// moved onto the entities (#1312). The expected tables below are the removed catalog's answers, written out per
/// definition; every bindable definition is bound through the production registry and asked through its entity.
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

    /// <summary>The dead Tourian corpses, whose rot trigger is owned by pointer when their header names it.</summary>
    private static readonly EnemyDefinitionId[] DeadTourianCorpses =
        [EnemyDefinitionId.CorpseZoomer, EnemyDefinitionId.CorpseRipper, EnemyDefinitionId.CorpseSkree];

    /// <summary>Crocomire's body and tongue, whose hitbox shot callbacks are owned by pointer.</summary>
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
            [EnemyDefinitionId.Steam] = BothWalkers,
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
    /// The named private shot callbacks each definition owned in the removed catalog, plus the Metroid's, which its entity
    /// already answered, and the horizontal shutter's, which the bomb pass runs (#1718); every definition not listed owns no
    /// named callback.
    /// </summary>
    private static Dictionary<EnemyDefinitionId, EnemyReaction[]> ExpectedNamedShotCallbacks()
    {
        var expected = new Dictionary<EnemyDefinitionId, EnemyReaction[]>
        {
            [EnemyDefinitionId.BabyTurtle] = [EnemyReaction.BabyTurtleShot],
            [EnemyDefinitionId.Owtch] = [EnemyReaction.OwtchShot],
            [EnemyDefinitionId.Rinka] = [EnemyReaction.RinkaShot],
            [EnemyDefinitionId.Oum] = [EnemyReaction.MaridiaLargeSnailShot],
            [EnemyDefinitionId.GRipper] = [EnemyReaction.GRipperRipper2Shot],
            [EnemyDefinitionId.Ripper2] = [EnemyReaction.GRipperRipper2Shot],
            [EnemyDefinitionId.Dragon] = [EnemyReaction.DragonShot],
            [EnemyDefinitionId.ShutterShootable] = [EnemyReaction.ShootableVerticalShutterShot],
            [EnemyDefinitionId.Kamer] = [EnemyReaction.ShootableVerticalShutterShot],
            [EnemyDefinitionId.ShutterDestroyable] = [EnemyReaction.DestroyableVerticalShutterShot],
            [EnemyDefinitionId.Metaree] = [EnemyReaction.MetareeShot],
            [EnemyDefinitionId.Fireflea] = [EnemyReaction.FirefleaShot],
            [EnemyDefinitionId.Tripper] = [EnemyReaction.TripperShot],
            [EnemyDefinitionId.Mochtroid] = [EnemyReaction.MochtroidShot],
            [EnemyDefinitionId.Skree] = [EnemyReaction.SkreeShot],
            [EnemyDefinitionId.Yard] = [EnemyReaction.YardShot],
            [EnemyDefinitionId.Evir] = [EnemyReaction.EvirShot],
            [EnemyDefinitionId.YappingMaw] = [EnemyReaction.YappingMawShot],
            [EnemyDefinitionId.Kago] = [EnemyReaction.KagoShot],
            [EnemyDefinitionId.Magdollite] = [EnemyReaction.MagdolliteShot],
            [EnemyDefinitionId.Beetom] = [EnemyReaction.BeetomShot],
            [EnemyDefinitionId.Powamp] = [EnemyReaction.PowampShot],
            [EnemyDefinitionId.Robot] = [EnemyReaction.WorkRobotShot, EnemyReaction.WorkRobotNoPowerShot],
            [EnemyDefinitionId.RobotNoPower] = [EnemyReaction.WorkRobotShot, EnemyReaction.WorkRobotNoPowerShot],
            [EnemyDefinitionId.Bull] = [EnemyReaction.BullShot],
            [EnemyDefinitionId.Spark] = [EnemyReaction.SparkShot],
            [EnemyDefinitionId.FaceBlock] = [EnemyReaction.BlueBrinstarFaceBlockShot],
            [EnemyDefinitionId.KihunterGreen] = [EnemyReaction.KiHunterShot],
            [EnemyDefinitionId.KihunterYellow] = [EnemyReaction.KiHunterShot],
            [EnemyDefinitionId.KihunterRed] = [EnemyReaction.KiHunterShot],
            [EnemyDefinitionId.MiniKraid] = [EnemyReaction.FakeKraidShot],
            [EnemyDefinitionId.Zebetite] = [EnemyReaction.ZebetiteShot],
            [EnemyDefinitionId.CorpseTorizo] = [EnemyReaction.DeadTorizoTouchAndShot],
            [EnemyDefinitionId.CorpseSidehopper] = [EnemyReaction.DeadSidehopperShot],
            [EnemyDefinitionId.DraygonBody] = [EnemyReaction.DraygonShot, EnemyReaction.DudShot],
            [EnemyDefinitionId.SporeSpawn] = [EnemyReaction.SporeSpawnShot, EnemyReaction.DudShot],
            [EnemyDefinitionId.PhantoonBody] = [EnemyReaction.PhantoonShot],
            [EnemyDefinitionId.MotherBrainBody] = [EnemyReaction.MotherBrainBodyShot],
            [EnemyDefinitionId.MotherBrainHead] = [EnemyReaction.MotherBrainHeadShot],
            [EnemyDefinitionId.BombTorizo] = [EnemyReaction.BombTorizoShot, EnemyReaction.TorizoStandUpSitDownShot],
            [EnemyDefinitionId.GoldenTorizo] =
            [
                EnemyReaction.GoldenTorizoShot, EnemyReaction.BombTorizoShot,
                EnemyReaction.TorizoStandUpSitDownShot,
            ],
            [EnemyDefinitionId.Shaktool] = [EnemyReaction.ShaktoolShot],
            [EnemyDefinitionId.KraidArm] = [EnemyReaction.KraidArmShot],
            [EnemyDefinitionId.RidleyCeres] = [EnemyReaction.RidleyShot],
            [EnemyDefinitionId.Ridley] = [EnemyReaction.RidleyShot],
            [EnemyDefinitionId.BabyMetroid] = [EnemyReaction.ShitroidShot],
            [EnemyDefinitionId.Botwoon] = [EnemyReaction.BotwoonShot],
            [EnemyDefinitionId.Metroid] = [EnemyReaction.MetroidShot],
        };
        foreach (EnemyDefinitionId pirate in OrdinaryPirates)
        {
            expected.Add(pirate,
            [
                EnemyReaction.SpacePirateShot, EnemyReaction.GoldNinjaVulnerableHitboxShot,
                EnemyReaction.GoldNinjaInvincibleHitboxShot,
            ]);
        }
        return expected;
    }

    /// <summary>
    /// Binds every room and auxiliary definition, and every definition the expected tables name, to one slot of a production
    /// enemy system, then compares its entity's capabilities, its answer for every named callback and every probed callback
    /// pointer, and its Ceres Ridley scene drawing with the hand-drawing flag clear and set, against the expected tables.
    /// </summary>
    internal static void VerifyEnemySpeciesCapabilities()
    {
        Dictionary<EnemyDefinitionId, EnemyCapabilities> capabilities = ExpectedCapabilities();
        Dictionary<EnemyDefinitionId, EnemyReaction[]> namedCallbacks = ExpectedNamedShotCallbacks();
        IEnumerable<EnemyDefinitionId> definitions = RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Id)
            .Concat(RoomEnemyAuxiliaryDefinitionCatalog.Definitions.Keys)
            .Concat(capabilities.Keys)
            .Concat(namedCallbacks.Keys)
            .Concat(DeadTourianCorpses)
            .Concat(CrocomireParts)
            .Where(definition => definition != EnemyDefinitionId.None)
            .Distinct();
        EnemyReaction?[] callbacks =
            [null, .. Enum.GetValues<EnemyReaction>().Select(callback => (EnemyReaction?)callback)];
        EnemyReaction[] crocomireCallbacks =
        [
            EnemyReaction.CrocomireHitCountShot, EnemyReaction.CrocomireDustHitboxShot,
            EnemyReaction.CrocomireMouthShot, EnemyReaction.CrocomireAlternateDustHitboxShot,
        ];

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot slot = enemies.SlotTable.Slots[1];
        int bound = 0;
        int answers = 0;
        foreach (EnemyDefinitionId definition in definitions)
        {
            slot.Bind(definition, HeaderOf(definition));
            IEnemyEntity entity = slot.Entity ?? throw new InvalidOperationException($"{definition} bound no entity.");
            bound++;

            AssertEqual(capabilities.GetValueOrDefault(definition, EnemyCapabilities.None), entity.Capabilities,
                $"{definition} collision capabilities");

            EnemyReaction[] owned = namedCallbacks.GetValueOrDefault(definition, Array.Empty<EnemyReaction>());
            bool ownsHeaderCallback = DeadTourianCorpses.Contains(definition);
            bool ownsCrocomireCallbacks = CrocomireParts.Contains(definition);
            foreach (EnemyReaction? callback in callbacks)
            {
                bool expected = callback is { } named && owned.Contains(named) ||
                    ownsHeaderCallback && DeadTourianBehavior.HasDeadTourianCorpseShotReaction(slot) && callback == EnemyCollision.ShotAi(slot) ||
                    ownsCrocomireCallbacks && callback is { } crocomire && crocomireCallbacks.Contains(crocomire);
                bool actual = entity is IEnemyPrivateShotReaction owner && owner.OwnsPrivateShotReaction(callback);
                AssertEqual(expected, actual, $"{definition} owns private shot callback {callback?.ToString() ?? "none"}");
                answers++;
            }

            var doorState = new CeresDoorEnemyState();
            slot.SetSpeciesState(doorState);
            foreach (bool handed in new[] { false, true })
            {
                doorState.HandsDrawingToRidley = handed;
                bool drawnByRidley = entity is IDrawnByCeresRidleyScene door && door.HandsDrawingToRidley;
                AssertEqual(definition == EnemyDefinitionId.CeresDoor && handed, drawnByRidley,
                    $"{definition} hands its drawing to Ceres Ridley's scene with the hand-drawing flag {(handed ? "set" : "clear")}");
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
