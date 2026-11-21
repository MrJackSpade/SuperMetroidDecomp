using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Samus.Grapple.GrappleFiringDefinitionsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Hud;

using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Samus HUD posture and movement policy against retail definitions and live consumers.</summary>
internal static class SamusHudDefinitionsVerification
{
    /// <summary>The cartridge's first posture-transition pose number: the transition flags are indexed from it.</summary>
    private const int FirstTransitionNumber = 0x35;

    /// <summary>The cartridge's first pose number the posture-transition handler never admits weapons for.</summary>
    private const int NonFiringTransitionNumber = 0xDB;

    /// <summary>The cartridge's first pose number the posture-transition handler hands to the standard weapon handler.</summary>
    private const int StandardTransitionNumber = 0xF1;

    /// <summary>Compares every native handler and posture branch, then checks Grapple admission and charged-shot preservation.</summary>
    internal static void VerifySamusHudDefinitions(CartridgeImportAddressSpace rom)
    {
        bool Transition(SamusPoseId pose, bool active) => SamusPoseNumbers.Number(pose) >= StandardTransitionNumber ||
            (SamusPoseNumbers.Number(pose) < NonFiringTransitionNumber &&
                (rom.ReadCartridgeByte(SamusCartridgeTablesConstants.HudTransitionFlags + SamusPoseNumbers.Number(pose) -
                    FirstTransitionNumber) == 0 || active));
        SamusPoseId metadataPose = SamusPoseId.FacingRightNormalPose;
        var metadata = NewGrappleFiringReadGuard(rom, () => metadataPose);
        var bus = NewHudPolicyReadGuard(metadata);
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        foreach (bool active in new[] { false, true })
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            if (pose < NonFiringTransitionNumber)
                AssertEqual(rom.ReadCartridgeByte(SamusCartridgeTablesConstants.HudTransitionFlags + pose -
                        FirstTransitionNumber) == 0,
                    SamusHudInput.PostureTransitionAdmitsWeapons(SamusPoseNumbering.FromNumber(pose), grappleActive: false, InstalledFixtures.SamusOverreads.Projectiles.PostureAdmission),
                    "Complete bounded posture-index admission: the byte read is zero");
            AssertEqual(Transition(SamusPoseNumbering.FromNumber(pose), active),
                SamusHudInput.PostureTransitionAdmitsWeapons(SamusPoseNumbering.FromNumber(pose), active, InstalledFixtures.SamusOverreads.Projectiles.PostureAdmission),
                "Every posture pose and active-Grapple branch");
        }
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        Action<SamusState, ushort> setTurn = (state, turn) =>
            state.TransitionShot.Hand(turn == 0 ? null : global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Samus.SamusTransitionShot>([typeof(global::SuperMetroid.Core.Samus.Projectiles.SamusProjectileDirection)], new object?[] { SuperMetroid.Core.Samus.Poses.SamusPoseRecordDecoding.DirectionOfAimByte(turn & byte.MaxValue)
                ?? throw new InvalidOperationException($"Turn word {turn} names no aim; a handed-over shot always names one.") }));
        foreach (int movement in SamusMovementTypeNumbers.Named)
        {
            SamusHudSelectionMode handler = HandlerOf(rom, movement);
            AssertEqual(handler, SamusHudObservations.SelectionMode(SamusMovementTypeNumbers.Of(movement)), "Every named movement's native HUD handler identity");
            for (int pose = 0; pose < 253; pose++)
            foreach (bool locked in new[] { false, true })
            foreach (ushort selected in new ushort[] { 3, 4 })
            foreach (ushort turn in new ushort[] { 0, 1 })
            {
                if (rom.ReadCartridgeByte(SamusSourceDefinitions.PoseDefinitions.Value + pose * 8 + 1) != movement) continue;
                if (!SamusPoseNumbering.NamesPose(pose)) continue;
                samus.ChangePoseKeepingAnimation(SamusPoseNumbering.FromNumber(pose));
                metadataPose = samus.Pose;
                if (locked) samus.Control.LockOutPlayer(); else samus.Control.Unlock();
                samus.SelectedHudItem = SuperMetroid.Diagnostics.Saves.SaveSlotNumbering.SavedHudItem(selected); setTurn(samus, turn);
                bool expected = !locked && pose is not (0 or 0x9b) && selected == 4 &&
                    (handler is SamusHudSelectionMode.Standard or SamusHudSelectionMode.Grapple or SamusHudSelectionMode.DraygonHeld ||
                        handler == SamusHudSelectionMode.Turning && turn != 0 ||
                        handler == SamusHudSelectionMode.Transition && Transition(SamusPoseNumbering.FromNumber(pose), false));
                AssertEqual(expected, SamusGrappleHudInput.IsSelectedAndAdmitted(samus, InstalledFixtures.SamusOverreads.Projectiles.PostureAdmission), "Actual Grapple HUD admission");
            }
        }
        // Exercise the other consumer, the real beam producer. Earn a partial charge
        // normally, then test whether the native HUD class holds it or advances it.
        var level = CreateRoom(new(64, 64), new RoomTerrainBlock[4096], new RoomBlockBehavior[4096]);
        for (int poseIndex = 0; poseIndex < 253; poseIndex++)
        foreach (ushort turn in new ushort[] { 0, 1 })
        {
            if (!SamusPoseNumbering.NamesPose(poseIndex)) continue;
            SamusPoseId pose = SamusPoseNumbering.FromNumber(poseIndex);
            byte movement = rom.ReadCartridgeByte(SamusSourceDefinitions.PoseDefinitions.Value + SamusPoseNumbers.Number(pose) * 8 + 1);
            metadataPose = SamusPoseId.FacingRightNormalPose;
            samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(512, 512)), EquippedBeams = SamusBeamFlags.Charge }.InPose(SamusPoseId.FacingRightNormalPose);
            var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: InstalledFixtures.Projectiles.FrameBindings), new global::SuperMetroid.Core.Fx.RoomEarthquake());
            var bombs = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: InstalledFixtures.Projectiles.FrameBindings));
            for (int frame = 0; frame < 10; frame++)
                shots.StepFrame(SamusTerrain.InRoom(level, SamusRoomObjectFixtures.Uninstalled), samus, new ProjectileButtonInput { ControllerInput = GameplayAction.Shoot, ControllerNewInput = 0, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), bombs, new SharedGameState(new SharedRandom()));
            AssertEqual(10, shots.Charge.UpdatesHeld, "Fixture charge earned through real producer");
            samus.ChangePoseKeepingAnimation(pose);
            metadataPose = samus.Pose;
            setTurn(samus, turn);
            SamusHudSelectionMode handler = HandlerOf(rom, movement);
            bool ball = SamusPoseMetadata.IsStableBallPose(pose);
            bool preserves = ball || pose is SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose || handler == SamusHudSelectionMode.Jump ||
                handler == SamusHudSelectionMode.Turning && turn == 0 || handler == SamusHudSelectionMode.Transition && !Transition(pose, false);
            shots.StepFrame(SamusTerrain.InRoom(level, SamusRoomObjectFixtures.Uninstalled), samus, new ProjectileButtonInput { ControllerInput = GameplayAction.Shoot, ControllerNewInput = 0, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), bombs, new SharedGameState(new SharedRandom()));
            // An admitted nonzero turn handoff forces release before held-input charge
            // processing; with no new Shoot edge this partial charge emits no projectile.
            AssertEqual(preserves ? 10 : turn != 0 ? 0 : 11, shots.Charge.UpdatesHeld, $"Actual projectile HUD charge preservation movement={movement:X2} pose={(int)SamusPoseNumbers.Number(pose):X2} turn={turn}");
            AssertTrue(shots.Shots.All(slot => !slot.IsActive), "Held partial charge does not emit shot");
        }
        Console.WriteLine("Samus HUD definitions: 28 handler words, every bounded posture-index observation, transition case, native-pose Grapple admission case and real charge-preservation case of the records that name a pose pass with policy reads forbidden.");
    }

    /// <summary>Runs the Samus HUD definition checks against the pinned cartridge revision.</summary>
    [VerificationCommand("--samus-hud-definitions")]
    internal static int SamusHudDefinitionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Samus HUD oracle revision");
        Suite(nameof(VerifySamusHudDefinitions), () => VerifySamusHudDefinitions(rom));
        return 0;
    }

    /// <summary>
    /// The HUD handler each cartridge movement-handler pointer names (#1491): the import side of the identity check, so
    /// the comparison is of identities and nothing reads the enum's values.
    /// </summary>
    private static readonly Dictionary<ushort, SamusHudSelectionMode> HandlerAt = new()
    {
        [0xdd3d] = SamusHudSelectionMode.Standard,
        [0xbf9d] = SamusHudSelectionMode.MorphBall,
        [0xddb6] = SamusHudSelectionMode.Jump,
        [0xdd6f] = SamusHudSelectionMode.Grapple,
        [0xddd8] = SamusHudSelectionMode.DraygonHeld,
        [0xdd74] = SamusHudSelectionMode.Turning,
        [0xdd8c] = SamusHudSelectionMode.Transition,
    };

    /// <summary>The HUD handler the cartridge's movement-handler table names for <paramref name="movement"/>.</summary>
    /// <exception cref="InvalidDataException">The table names a handler the identity table does not know.</exception>
    private static SamusHudSelectionMode HandlerOf(CartridgeImportAddressSpace rom, int movement)
    {
        ushort word = rom.ReadCartridgeWord(SamusCartridgeTablesConstants.HudMovementHandlers + movement * 2);
        return HandlerAt.TryGetValue(word, out SamusHudSelectionMode handler) ? handler
            : throw new InvalidDataException($"Movement type {movement} names unknown HUD handler ${word:X4}.");
    }

    /// <summary>Forbids converted movement-handler and posture-flag ROM reads during HUD policy checks.</summary>
    private static MutableCartridgeReadGuard NewHudPolicyReadGuard(ISnesAddressSpace source)
    {
        int postureStart = SamusCartridgeTablesConstants.HudTransitionFlags - FirstTransitionNumber;
        int postureEnd = SamusCartridgeTablesConstants.HudTransitionFlags + NonFiringTransitionNumber - FirstTransitionNumber;
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address >= SamusCartridgeTablesConstants.HudMovementHandlers && address < 0x90dd3d ||
                address >= postureStart && address < postureEnd,
            address => $"Compiled HUD policy read ROM ${address:X6}.");
        return guard;
    }
}
