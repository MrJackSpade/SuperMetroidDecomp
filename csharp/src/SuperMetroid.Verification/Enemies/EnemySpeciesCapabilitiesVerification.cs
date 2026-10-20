using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Corpses;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

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
        EnemyCapabilities.ExtendedSamusHitboxes | EnemyCapabilities.ExtendedProjectileHitboxes;

    /// <summary>The ordinary Pirate definitions: the wall, walking and ninja families in every colour.</summary>
    private static readonly EnemyDefinitionId[] OrdinaryPirates =
    [
        EnemyDefinitionId.PirateGreyWall, EnemyDefinitionId.PirateGreenWall, EnemyDefinitionId.PirateRedWall,
        EnemyDefinitionId.PirateGoldWall, EnemyDefinitionId.PirateMagentaWall, EnemyDefinitionId.PirateSilverWall,
        EnemyDefinitionId.PirateGreyWalking, EnemyDefinitionId.PirateGreenWalking, EnemyDefinitionId.PirateRedWalking,
        EnemyDefinitionId.PirateGoldWalking, EnemyDefinitionId.PirateMagentaWalking, EnemyDefinitionId.PirateSilverWalking,
        EnemyDefinitionId.PirateGreyNinja, EnemyDefinitionId.PirateGreenNinja, EnemyDefinitionId.PirateRedNinja,
        EnemyDefinitionId.PirateGoldNinja, EnemyDefinitionId.PirateMagentaNinja, EnemyDefinitionId.PirateSilverNinja,
    ];

    /// <summary>The dead Tourian corpses, whose rot trigger is owned by pointer when their header names it.</summary>
    private static readonly EnemyDefinitionId[] DeadTourianCorpses =
        [EnemyDefinitionId.CorpseZoomer, EnemyDefinitionId.CorpseRipper, EnemyDefinitionId.CorpseSkree];

    /// <summary>Crocomire's body and tongue, whose hitbox shot callbacks are owned by pointer.</summary>
    private static readonly EnemyDefinitionId[] CrocomireParts = [EnemyDefinitionId.Crocomire, EnemyDefinitionId.CrocomireTongue];

    /// <summary>Room-referenced definitions with no ported species; binding one fails by design, so none is asked.</summary>
    private static readonly EnemyDefinitionId[] UnportedDefinitions = [EnemyDefinitionId.Reflec, EnemyDefinitionId.CorpseSidehopper2];

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
            [EnemyDefinitionId.PhantoonBody] = EnemyCapabilities.ExtendedSamusHitboxes,
            [EnemyDefinitionId.Kraid] = EnemyCapabilities.ExtendedProjectileHitboxes,
            [EnemyDefinitionId.Ridley] = EnemyCapabilities.ExtendedProjectileHitboxes,
            [EnemyDefinitionId.MotherBrainBody] = EnemyCapabilities.ExtendedProjectileHitboxes,
            [EnemyDefinitionId.MotherBrainHead] = EnemyCapabilities.ExtendedProjectileHitboxes,
        };
        foreach (EnemyDefinitionId pirate in OrdinaryPirates)
            expected.Add(pirate, BothWalkers);
        return expected;
    }

    /// <summary>
    /// The named private shot callbacks each definition owned in the removed catalog, plus the Metroid's, which its entity
    /// already answered; every definition not listed owns no named callback.
    /// </summary>
    private static Dictionary<EnemyDefinitionId, EnemyInteractionCallback[]> ExpectedNamedShotCallbacks()
    {
        var expected = new Dictionary<EnemyDefinitionId, EnemyInteractionCallback[]>
        {
            [EnemyDefinitionId.BabyTurtle] = [EnemyInteractionCallback.BabyTurtleShot],
            [EnemyDefinitionId.Owtch] = [EnemyInteractionCallback.OwtchShot],
            [EnemyDefinitionId.Rinka] = [EnemyInteractionCallback.RinkaShot],
            [EnemyDefinitionId.Oum] = [EnemyInteractionCallback.MaridiaLargeSnailShot],
            [EnemyDefinitionId.GRipper] = [EnemyInteractionCallback.GRipperRipper2Shot],
            [EnemyDefinitionId.Ripper2] = [EnemyInteractionCallback.GRipperRipper2Shot],
            [EnemyDefinitionId.Dragon] = [EnemyInteractionCallback.DragonShot],
            [EnemyDefinitionId.ShutterShootable] = [EnemyInteractionCallback.ShootableVerticalShutterShot],
            [EnemyDefinitionId.Kamer] = [EnemyInteractionCallback.ShootableVerticalShutterShot],
            [EnemyDefinitionId.ShutterDestroyable] = [EnemyInteractionCallback.DestroyableVerticalShutterShot],
            [EnemyDefinitionId.Metaree] = [EnemyInteractionCallback.MetareeShot],
            [EnemyDefinitionId.Fireflea] = [EnemyInteractionCallback.FirefleaShot],
            [EnemyDefinitionId.Tripper] = [EnemyInteractionCallback.TripperShot],
            [EnemyDefinitionId.Mochtroid] = [EnemyInteractionCallback.MochtroidShot],
            [EnemyDefinitionId.Skree] = [EnemyInteractionCallback.SkreeShot],
            [EnemyDefinitionId.Yard] = [EnemyInteractionCallback.YardShot],
            [EnemyDefinitionId.Evir] = [EnemyInteractionCallback.EvirShot],
            [EnemyDefinitionId.YappingMaw] = [EnemyInteractionCallback.YappingMawShot],
            [EnemyDefinitionId.Kago] = [EnemyInteractionCallback.KagoShot],
            [EnemyDefinitionId.Magdollite] = [EnemyInteractionCallback.MagdolliteShot],
            [EnemyDefinitionId.Beetom] = [EnemyInteractionCallback.BeetomShot],
            [EnemyDefinitionId.Powamp] = [EnemyInteractionCallback.PowampShot],
            [EnemyDefinitionId.Robot] = [EnemyInteractionCallback.WorkRobotShot, EnemyInteractionCallback.WorkRobotNoPowerShot],
            [EnemyDefinitionId.RobotNoPower] = [EnemyInteractionCallback.WorkRobotShot, EnemyInteractionCallback.WorkRobotNoPowerShot],
            [EnemyDefinitionId.Bull] = [EnemyInteractionCallback.BullShot],
            [EnemyDefinitionId.Spark] = [EnemyInteractionCallback.SparkShot],
            [EnemyDefinitionId.FaceBlock] = [EnemyInteractionCallback.BlueBrinstarFaceBlockShot],
            [EnemyDefinitionId.KihunterGreen] = [EnemyInteractionCallback.KiHunterShot],
            [EnemyDefinitionId.KihunterYellow] = [EnemyInteractionCallback.KiHunterShot],
            [EnemyDefinitionId.KihunterRed] = [EnemyInteractionCallback.KiHunterShot],
            [EnemyDefinitionId.MiniKraid] = [EnemyInteractionCallback.FakeKraidShot],
            [EnemyDefinitionId.Zebetite] = [EnemyInteractionCallback.ZebetiteShot],
            [EnemyDefinitionId.CorpseTorizo] = [EnemyInteractionCallback.DeadTorizoTouchAndShot],
            [EnemyDefinitionId.CorpseSidehopper] = [EnemyInteractionCallback.DeadSidehopperShot],
            [EnemyDefinitionId.DraygonBody] = [EnemyInteractionCallback.DraygonShot, EnemyInteractionCallback.DudShot],
            [EnemyDefinitionId.SporeSpawn] = [EnemyInteractionCallback.SporeSpawnShot, EnemyInteractionCallback.DudShot],
            [EnemyDefinitionId.PhantoonBody] = [EnemyInteractionCallback.PhantoonShot],
            [EnemyDefinitionId.MotherBrainBody] = [EnemyInteractionCallback.MotherBrainBodyShot],
            [EnemyDefinitionId.MotherBrainHead] = [EnemyInteractionCallback.MotherBrainHeadShot],
            [EnemyDefinitionId.BombTorizo] = [EnemyInteractionCallback.BombTorizoShot, EnemyInteractionCallback.TorizoStandUpSitDownShot],
            [EnemyDefinitionId.GoldenTorizo] =
            [
                EnemyInteractionCallback.GoldenTorizoShot, EnemyInteractionCallback.BombTorizoShot,
                EnemyInteractionCallback.TorizoStandUpSitDownShot,
            ],
            [EnemyDefinitionId.Shaktool] = [EnemyInteractionCallback.ShaktoolShot],
            [EnemyDefinitionId.KraidArm] = [EnemyInteractionCallback.KraidArmShot],
            [EnemyDefinitionId.RidleyCeres] = [EnemyInteractionCallback.RidleyShot],
            [EnemyDefinitionId.Ridley] = [EnemyInteractionCallback.RidleyShot],
            [EnemyDefinitionId.BabyMetroid] = [EnemyInteractionCallback.ShitroidShot],
            [EnemyDefinitionId.Botwoon] = [EnemyInteractionCallback.BotwoonShot],
            [EnemyDefinitionId.Metroid] = [EnemyInteractionCallback.MetroidShot],
        };
        foreach (EnemyDefinitionId pirate in OrdinaryPirates)
        {
            expected.Add(pirate,
            [
                EnemyInteractionCallback.SpacePirateShot, EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot,
                EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot,
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
        Dictionary<EnemyDefinitionId, EnemyInteractionCallback[]> namedCallbacks = ExpectedNamedShotCallbacks();
        IEnumerable<EnemyDefinitionId> definitions = RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Pointer)
            .Concat(RoomEnemyAuxiliaryDefinitionCatalog.Definitions.Keys)
            .Concat(capabilities.Keys)
            .Concat(namedCallbacks.Keys)
            .Concat(DeadTourianCorpses)
            .Concat(CrocomireParts)
            .Where(definition => definition != EnemyDefinitionId.None && !UnportedDefinitions.Contains(definition))
            .Distinct();
        EnemyInteractionCallback?[] callbacks =
            [null, .. Enum.GetValues<EnemyInteractionCallback>().Select(callback => (EnemyInteractionCallback?)callback)];
        EnemyInteractionCallback[] crocomireCallbacks =
        [
            EnemyInteractionCallback.CrocomireNoOpHitboxShot, EnemyInteractionCallback.CrocomireDustHitboxShot,
            EnemyInteractionCallback.CrocomireMouthShot, EnemyInteractionCallback.CrocomireAlternateDustHitboxShot,
            EnemyInteractionCallback.CrocomireHeaderTouch,
        ];

        var enemies = new RoomEnemySystem(new EnemySession());
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

            EnemyInteractionCallback[] owned = namedCallbacks.GetValueOrDefault(definition, Array.Empty<EnemyInteractionCallback>());
            bool ownsHeaderCallback = DeadTourianCorpses.Contains(definition);
            bool ownsCrocomireCallbacks = CrocomireParts.Contains(definition);
            foreach (EnemyInteractionCallback? callback in callbacks)
            {
                bool expected = callback is { } named && owned.Contains(named) ||
                    ownsHeaderCallback && DeadTourianBehavior.HasDeadTourianCorpseShotCallback(slot) && callback == EnemyCollision.ShotAi(slot) ||
                    ownsCrocomireCallbacks && callback is { } crocomire && crocomireCallbacks.Contains(crocomire);
                bool actual = entity is IEnemyPrivateShotCallback owner && owner.OwnsPrivateShotCallback(callback);
                AssertEqual(expected, actual, $"{definition} owns private shot callback {callback?.ToString() ?? "none"}");
                answers++;
            }

            var doorState = new CeresDoorEnemyState();
            slot.SetSpeciesState(doorState, doorState.ClearWithRecord);
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
