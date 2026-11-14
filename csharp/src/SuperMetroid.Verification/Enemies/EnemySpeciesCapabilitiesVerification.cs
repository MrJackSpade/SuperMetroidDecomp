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
    /// The private shot reactions each definition owns (#1759): none (null) where the definition leaves its shots to a
    /// species with no hitbox reactions of its own, and the species' own identities where its hitboxes name them. The
    /// removed catalog's named callbacks map one to one onto these: a species' definition-level callback is the reaction
    /// its definition now leaves to it. Every definition not listed owns none.
    /// </summary>
    private static Dictionary<EnemyDefinitionId, EnemyReaction?[]> ExpectedPrivateShotReactions()
    {
        EnemyReaction?[] leftToSpecies = [null];
        var expected = new Dictionary<EnemyDefinitionId, EnemyReaction?[]>
        {
            [EnemyDefinitionId.BabyTurtle] = leftToSpecies,
            [EnemyDefinitionId.Owtch] = leftToSpecies,
            [EnemyDefinitionId.Rinka] = leftToSpecies,
            [EnemyDefinitionId.Oum] = [MaridiaLargeSnailReactions.Shot],
            [EnemyDefinitionId.GRipper] = leftToSpecies,
            [EnemyDefinitionId.Ripper2] = leftToSpecies,
            [EnemyDefinitionId.Dragon] = leftToSpecies,
            [EnemyDefinitionId.ShutterShootable] = leftToSpecies,
            [EnemyDefinitionId.Kamer] = leftToSpecies,
            [EnemyDefinitionId.ShutterDestroyable] = leftToSpecies,
            [EnemyDefinitionId.Metaree] = leftToSpecies,
            [EnemyDefinitionId.Fireflea] = leftToSpecies,
            [EnemyDefinitionId.Tripper] = leftToSpecies,
            [EnemyDefinitionId.Mochtroid] = leftToSpecies,
            [EnemyDefinitionId.Skree] = leftToSpecies,
            [EnemyDefinitionId.Yard] = leftToSpecies,
            [EnemyDefinitionId.Evir] = leftToSpecies,
            [EnemyDefinitionId.YappingMaw] = leftToSpecies,
            [EnemyDefinitionId.Kago] = leftToSpecies,
            [EnemyDefinitionId.Magdollite] = leftToSpecies,
            [EnemyDefinitionId.Beetom] = leftToSpecies,
            [EnemyDefinitionId.Powamp] = leftToSpecies,
            [EnemyDefinitionId.Robot] = leftToSpecies,
            [EnemyDefinitionId.RobotNoPower] = leftToSpecies,
            [EnemyDefinitionId.Bull] = leftToSpecies,
            [EnemyDefinitionId.Spark] = leftToSpecies,
            [EnemyDefinitionId.FaceBlock] = leftToSpecies,
            [EnemyDefinitionId.KihunterGreen] = leftToSpecies,
            [EnemyDefinitionId.KihunterYellow] = leftToSpecies,
            [EnemyDefinitionId.KihunterRed] = leftToSpecies,
            [EnemyDefinitionId.MiniKraid] = leftToSpecies,
            [EnemyDefinitionId.Zebetite] = leftToSpecies,
            [EnemyDefinitionId.CorpseTorizo] = leftToSpecies,
            [EnemyDefinitionId.CorpseSidehopper] = leftToSpecies,
            [EnemyDefinitionId.CorpseZoomer] = leftToSpecies,
            [EnemyDefinitionId.CorpseRipper] = leftToSpecies,
            [EnemyDefinitionId.CorpseSkree] = leftToSpecies,
            [EnemyDefinitionId.DraygonBody] = [DraygonReactions.Shot, EnemyReaction.DudShot],
            [EnemyDefinitionId.SporeSpawn] = [SporeSpawnReactions.Shot, EnemyReaction.DudShot],
            [EnemyDefinitionId.PhantoonBody] = [PhantoonReactions.Shot],
            [EnemyDefinitionId.MotherBrainBody] = [MotherBrainReactions.BodyShot],
            [EnemyDefinitionId.MotherBrainHead] = [MotherBrainReactions.HeadShot],
            [EnemyDefinitionId.BombTorizo] = [TorizoReactions.Shot, TorizoReactions.StandUpSitDownShot],
            [EnemyDefinitionId.GoldenTorizo] = [TorizoReactions.GoldenShot, TorizoReactions.Shot, TorizoReactions.StandUpSitDownShot],
            [EnemyDefinitionId.Shaktool] = leftToSpecies,
            [EnemyDefinitionId.KraidArm] = [KraidReactions.ArmShot],
            [EnemyDefinitionId.RidleyCeres] = [RidleyReactions.Shot],
            [EnemyDefinitionId.Ridley] = [RidleyReactions.Shot],
            [EnemyDefinitionId.BabyMetroid] = leftToSpecies,
            [EnemyDefinitionId.Botwoon] = leftToSpecies,
            [EnemyDefinitionId.Metroid] = leftToSpecies,
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
                CrocomireReactions.HitCountShot, CrocomireReactions.DustHitboxShot,
                CrocomireReactions.MouthShot, CrocomireReactions.AlternateDustHitboxShot,
            ]);
        }
        return expected;
    }

    /// <summary>Every reaction a shot can select: none, the shared ones and each species' own.</summary>
    private static readonly EnemyReaction?[] ProbedReactions =
    [
        null, .. EnemyReaction.Shared,
        CeresSteamReactions.Touch,
        CrocomireReactions.ClawTouch, CrocomireReactions.HitCountShot, CrocomireReactions.DustHitboxShot,
        CrocomireReactions.MouthShot, CrocomireReactions.AlternateDustHitboxShot,
        DraygonReactions.Touch, DraygonReactions.Shot,
        KraidReactions.BackgroundTouch, KraidReactions.ArmTouch, KraidReactions.ArmShot,
        MotherBrainReactions.BodyShot, MotherBrainReactions.HeadShot, MotherBrainReactions.HeadTouch,
        PhantoonReactions.Touch, PhantoonReactions.Shot,
        RidleyReactions.FrameHitboxTouch, RidleyReactions.Shot,
        SporeSpawnReactions.Touch, SporeSpawnReactions.Shot,
        TorizoReactions.Touch, TorizoReactions.Shot, TorizoReactions.StandUpSitDownShot, TorizoReactions.GoldenShot,
        MaridiaLargeSnailReactions.DamagingTouch, MaridiaLargeSnailReactions.NonDamagingTouch, MaridiaLargeSnailReactions.Shot,
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
        Dictionary<EnemyDefinitionId, EnemyReaction?[]> privateReactions = ExpectedPrivateShotReactions();
        IEnumerable<EnemyDefinitionId> definitions = RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Id)
            .Concat(RoomEnemyAuxiliaryDefinitionCatalog.Definitions.Keys)
            .Concat(capabilities.Keys)
            .Concat(privateReactions.Keys)
            .Where(definition => definition != EnemyDefinitionId.None)
            .Distinct();

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

            EnemyReaction?[] owned = privateReactions.GetValueOrDefault(definition, []);
            foreach (EnemyReaction? reaction in ProbedReactions)
            {
                bool expected = owned.Contains(reaction);
                bool actual = entity is IEnemyPrivateShotReaction owner && owner.OwnsPrivateShotReaction(reaction);
                AssertEqual(expected, actual, $"{definition} owns private shot reaction {reaction?.ToString() ?? "none"}");
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
