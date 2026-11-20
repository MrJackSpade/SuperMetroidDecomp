using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileSoundRoutingVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Samus.Hud.SamusHudSelectionVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Samus;

/// <summary>Bounded cartridge comparison for the left-facing Murder Beam setup and its explicitly unsupported unsafe variants.</summary>
internal static class MurderBeamVerification
{
    /// <summary>
    /// Verifies the cartridge-safe left-facing Murder Beam against the bounded original-CPU
    /// probe. The unsafe directions enter unrelated native code and are asserted separately
    /// as unsupported rather than being disguised as an ordinary Wave projectile.
    /// </summary>
    internal static void VerifyMurderBeam()
    {
        var bus = NewProjectileSoundRoutingForbiddenBus(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new RoomTerrainBlock[256], new RoomBlockBehavior[256], new RoomBlockAppearance[256]), RoomLevelData.NothingBeyondGrid(), []);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.FacingLeftNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            CollectedItems = SamusEquipmentFlags.HiJumpBoots,
            EquippedItems = SamusEquipmentFlags.HiJumpBoots,
            CollectedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma,
            EquippedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer,
            SelectedHudItem = HudItem.Beam,
        };
        var projectiles = CreateProjectileFixture();
        var shared = CreateBombFixture();

        HoldCharge(level, samus, projectiles, shared);

        var pause = CreateRetailPauseFixture(new PauseMenuEntry { ScreenContent = global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps, SpriteSheets = global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SpriteSheets, Audio = AudioQueues.Standalone(), Samus = samus, System = new SharedGameState(new SharedRandom()), Area = AreaId.Crateria, RoomMapPosition = new(0, 0), GameplayHud = global::SuperMetroid.Core.Hud.ShownHud.Nothing, MapRevealMode = global::SuperMetroid.Core.Map.MapRevealMode.None });
        EnterPauseEquipment(pause);
        SelectPauseBoots(pause);
        pause.Step(0, (ControllerButton.Left | ControllerButton.A));
        AssertEqual((ushort)0x100f, samus.EquippedBeams.ToEquipmentWord(),
            "same-frame Boots Left+A equips all four beams while retaining Charge");

        shared.StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, 0, 0);
        SamusProjectileSlotObservation slotsBeforeFire = projectiles.ObserveSlots();
        SoundRequest? fired = projectiles.StepFrameFiringSound(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), shared, new SharedGameState(new SharedRandom()));
        AssertEqual(0, slotsBeforeFire.FiredSlot(projectiles),
            "charged-left Murder Beam allocates slot zero");
        AssertTrue(fired is null,
            "Murder Beam's adjacent sound-table entry is zero");

        SamusProjectile shot = projectiles.Shots[0];
        AssertEqual(SamusProjectileType.Beam(SamusBeamCombination.SpazerPlasmaIceWave, charged: true), shot.Type, "Murder Beam retains charged all-beams type");
        AssertEqual(200, shot.Damage, "Murder Beam uses the native 200-damage record");
        AssertEqual(SamusProjectileDirection.Left, shot.Direction,
            "safe Murder Beam faces left");
        AssertEqual((ushort)116, shot.Position.X.Whole, "Murder Beam native muzzle X");
        AssertEqual((ushort)123, shot.Position.Y.Whole, "Murder Beam native muzzle Y");
        AssertEqual(SamusShotBehaviors.MurderBeam,
            shot.Behavior, "Murder Beam retains its overread callback identity");
        AssertEqual(0, shot.InstructionPointer,
            "safe left-facing data record selects no animation list");
        AssertEqual((ushort)0, shot.Radius.X, "safe Murder Beam X radius");
        AssertEqual((ushort)0, shot.Radius.Y, "safe Murder Beam Y radius");
        AssertEqual(0, shot.Velocity.X.InTwoHundredFiftySixthsOfPixel, "safe Murder Beam X speed");
        AssertEqual(0, shot.Velocity.Y.InTwoHundredFiftySixthsOfPixel, "safe Murder Beam Y speed");
        AssertTrue(!shot.IsActive, "zero-list Murder Beam is not animated or drawn");
        AssertTrue(shot.HasEnemyCollisionPayload,
            "zero-list Murder Beam remains visible to bank-$A0 enemy collision");
        AssertEqual(1, projectiles.Pool.ShotCount.Count,
            "Murder Beam retains its ordinary projectile allocation");

        for (int frame = 0; frame < 16; frame++)
        {
            shared.StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, 0, 0);
            projectiles.StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), shared, new SharedGameState(new SharedRandom()));
        }
        AssertEqual(SamusProjectileType.Beam(SamusBeamCombination.SpazerPlasmaIceWave, charged: true), shot.Type, "Murder Beam persists without an instruction list");
        AssertEqual(200, shot.Damage, "persistent Murder Beam retains damage");
        AssertEqual((ushort)116, shot.Position.X.Whole, "persistent Murder Beam remains fixed in world X");
        AssertEqual((ushort)123, shot.Position.Y.Whole, "persistent Murder Beam remains fixed in world Y");

        EnemyDropFixture combat = CreateEnemyDropFixture(samus, [1]);
        RoomEnemyPlace target = combat.System.Places.InUpdateOrder[0];
        target.Bind(EnemyPlaceFixtures.StandIn, target.Definition);
        target.Bind(target.DefinitionId, default(RoomEnemyDefinition) with
        {
            Shot = EnemyReaction.OrdinaryShot,
        });
        target.Position = target.Position.WithWhole(shot.Position.Whole);
        target.Radius = new(16, 16);
        target.Health = 1000;
        target.Frame = EnemyFrameId.SkulteraSwimmingLeftTailStraight;
        combat.System.StepStandaloneFrame(combat.System.StandaloneFrame(combat.Level) with { Camera = new(0, 0), TimeIsFrozen = true, Samus = samus });

        combat.System.ResolveShotsOnInteractiveEnemies(
            projectiles, shared, SuperMetroid.Core.Progression.AreaId.Crateria, samus);
        AssertEqual(800, target.Health,
            "charged vulnerability applies one 200-point Murder Beam hit");
        AssertEqual(SamusProjectileType.Beam(SamusBeamCombination.SpazerPlasmaIceWave, charged: true), shot.Type,
            "Plasma-bearing Murder Beam survives enemy impact");
        AssertEqual(0, shot.InstructionPointer,
            "Murder Beam remains zero-list after enemy impact");

        target.Invincibility.End();
        combat.System.ResolveShotsOnInteractiveEnemies(
            projectiles, shared, SuperMetroid.Core.Progression.AreaId.Crateria, samus);
        AssertEqual(600, target.Health,
            "second Murder Beam contact repeats the native damage");

        target.Invincibility.End();
        target.Freeze.End();
        target.Health = 100;
        combat.System.ResolveShotsOnInteractiveEnemies(
            projectiles, shared, SuperMetroid.Core.Progression.AreaId.Crateria, samus);
        AssertEqual(100, target.Health,
            "lethal Ice-bearing Murder Beam preserves health while freezing");
        AssertEqual(400, target.Freeze.UpdatesLeft,
            "Murder Beam freezes a vulnerable non-Norfair enemy");

        var phaseBoundary = MotherBrainLateFightFixtures.NewSequence();
        phaseBoundary.StartFinishOffSequence();
        phaseBoundary.BeginPhase3RecoveryFromBabyCutscene();
        AssertEqual(MotherBrainPhase3FightPhase.RecoverMakeSomeDistance, phaseBoundary.Encounter.Phase3FightPhase, "Mother Brain enters the phase-three recovery boundary");
        for (int frame = 0; frame < 120; frame++)
        {
            shared.StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, 0, 0);
            projectiles.StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), shared, new SharedGameState(new SharedRandom()));
        }
        AssertEqual(SamusProjectileType.Beam(SamusBeamCombination.SpazerPlasmaIceWave, charged: true), shot.Type,
            "Murder Beam survives the phase-two to phase-three cutscene boundary");
        AssertEqual(200, shot.Damage,
            "phase-three Murder Beam retains its damage payload");

        Suite(nameof(VerifyMissileAutoCancelMurderBeam), () => VerifyMissileAutoCancelMurderBeam(level));
        Suite(nameof(VerifyUnsafeMurderBeamDirection), () => VerifyUnsafeMurderBeamDirection(level));
        Suite(nameof(VerifyUnsafeUnchargedMurderBeam), () => VerifyUnsafeUnchargedMurderBeam(level));
        Console.WriteLine(
            "Murder Beam: precharge and missile auto-cancel setups, left-facing zero-list " +
            "persistence through Mother Brain's phase boundary, repeated 200-point damage " +
            "and Ice substitution match bounded cartridge evidence.");
    }

    /// <summary>Advances held shoot to the fully charged threshold, with an optional preexisting button hold for missile auto-cancel.</summary>
    private static void HoldCharge(
        RoomLevelData level,
        SamusState samus,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem shared,
        bool shootAlreadyHeld = false)
    {
        for (int frame = 0; frame < BeamChargeDefinitions.FullyChargedCounter; frame++)
        {
            GameplayAction newlyPressed = frame == 0 && !shootAlreadyHeld
                ? GameplayAction.Shoot
                : 0;
            shared.StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, GameplayAction.Shoot, newlyPressed);
            projectiles.StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, new ProjectileButtonInput { ControllerInput = GameplayAction.Shoot, ControllerNewInput = newlyPressed, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), shared, new SharedGameState(new SharedRandom()));
        }
        AssertEqual(BeamChargeDefinitions.FullyChargedCounter,
            projectiles.Charge.UpdatesHeld, "pre-pause beam charge reaches charged threshold");
    }

    /// <summary>Checks that the right-facing corrupted beam direction fails explicitly before it can masquerade as a normal shot.</summary>
    private static void VerifyUnsafeMurderBeamDirection(
        RoomLevelData level)
    {
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            EquippedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer,
            SelectedHudItem = HudItem.Beam,
        };
        var projectiles = CreateProjectileFixture();
        var shared = CreateBombFixture();
        HoldCharge(level, samus, projectiles, shared);
        samus.EquippedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma;
        AssertThrows<NotSupportedException>(() =>
        {
            shared.StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, 0, 0);
            projectiles.StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), shared, new SharedGameState(new SharedRandom()));
        }, "unsafe right-facing Murder Beam does not silently become an ordinary projectile");
    }

    /// <summary>Checks the Select plus Item Cancel missile route into a charged Murder Beam after missile cooldown suppresses an uncharged shot.</summary>
    private static void VerifyMissileAutoCancelMurderBeam(
        RoomLevelData level)
    {
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.FacingLeftNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            EquippedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma,
            Missiles = 5,
        };
        var projectiles = CreateProjectileFixture();
        var shared = CreateBombFixture();
        GameplayAction select = GameplayAction.ItemSelect;
        GameplayAction cancel = GameplayAction.ItemCancel;
        AssertTrue(SelectionChanged(samus,
                (select | cancel),
                select),
            "held Item Cancel plus Select chooses Missiles for auto-cancel");
        AssertEqual(HudItem.Missile, samus.AutoCancelledItem,
            "missile auto-cancel request retains the selected HUD index");

        shared.StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, GameplayAction.Shoot, GameplayAction.Shoot);
        SoundRequest? firingSound = projectiles.StepFrameFiringSound(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, new ProjectileButtonInput { ControllerInput = GameplayAction.Shoot, ControllerNewInput = GameplayAction.Shoot, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), shared, new SharedGameState(new SharedRandom()));
        AssertTrue(firingSound is not null,
            "auto-cancel setup fires one Missile");
        AssertEqual(HudItem.Beam, samus.SelectedHudItem,
            "successful Missile auto-cancels back to beams");
        AssertEqual(HudItem.Beam, samus.AutoCancelledItem,
            "successful Missile consumes its auto-cancel request");

        HoldCharge(level, samus, projectiles, shared, shootAlreadyHeld: true);
        shared.StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, 0, 0);
        SamusProjectileSlotObservation slotsBeforeMurder = projectiles.ObserveSlots();
        projectiles.StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), shared, new SharedGameState(new SharedRandom()));
        int? murderSlot = slotsBeforeMurder.FiredSlot(projectiles);
        AssertTrue(murderSlot is not null,
            "missile cooldown suppresses the unsafe uncharged shot and permits charged release");
        AssertEqual(SamusProjectileType.Beam(SamusBeamCombination.SpazerPlasmaIceWave, charged: true), projectiles.Shots[(murderSlot ?? throw new InvalidOperationException("Verification requires murderSlot."))].Type,
            "missile auto-cancel alternative produces Murder Beam");
    }

    /// <summary>Rejects the uncharged corrupted beam combination before the projectile path treats it as an ordinary beam.</summary>
    private static void VerifyUnsafeUnchargedMurderBeam(
        RoomLevelData level)
    {
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.FacingLeftNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            EquippedBeams = SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer | SamusBeamFlags.Plasma,
        };
        AssertThrows<NotSupportedException>(() => CreateProjectileFixture().StepFrame(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, new ProjectileButtonInput { ControllerInput = GameplayAction.Shoot, ControllerNewInput = GameplayAction.Shoot, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), CreateBombFixture(), new SharedGameState(new SharedRandom())),
            "unsafe uncharged Murder Beam does not silently become an ordinary projectile");
    }

    /// <summary>Runs bounded Murder Beam verification from the command line.</summary>
    [VerificationCommand("--murder-beam")]
    internal static int MurderBeamCommand()
    {
        Suite(nameof(VerifyMurderBeam), () => VerifyMurderBeam());
        return 0;
    }
}
