using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Samus.Projectiles.BeamSpeedRowsVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks beam pre-instruction pointers and their installation by production firing paths.</summary>
internal static class BeamCallbackTablesVerification
{
    /// <summary>Compares charged and uncharged callback tables with the cartridge, then checks beam initialization or firing.</summary>
    internal static void VerifyBeamCallbackTables(bool initializeOnly = false)
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool charged in new[] { false, true })
        {
            int table = charged
                ? SamusBeamFrameBehaviorCodes.ChargedTable
                : SamusBeamFrameBehaviorCodes.UnchargedTable;
            for (int combination = 0; combination < Enum.GetValues<SamusBeamCombination>().Length; combination++)
            {
                AssertEqual(
                    CartridgeImportSource.Require(retail).ReadCartridgeWord((table + combination * sizeof(ushort))),
                    SamusBeamShotBehaviors.Resolve(charged, SamusBeamCombinationTableOrderConstants.FromTableIndex(combination)).NativePointer,
                    $"charged={charged} callback {combination:X1} matches cartridge");

            }
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusBeamShotBehaviors.Resolve(false, SamusBeamCombinationTableOrderConstants.FromTableIndex(16)),
            "beam combination rejects index beyond low nibble");

        var room = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new RoomTerrainBlock[256], new RoomBlockBehavior[256], new RoomBlockAppearance[256]), RoomLevelData.NothingBeyondGrid(), []);
        foreach (bool charged in new[] { false, true })
        foreach (int beamType in Enumerable.Range(0, 12))
        {
            var bus = new BeamSpeedRowAddressSpace(retail);
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
                EquippedBeams = SamusEquipmentWordCodec.FromBeamWord(checked((ushort)beamType)) | (charged ? SamusBeamFlags.Charge : SamusBeamFlags.None),
            }.InPose(SamusPoseId.FacingRightNormalPose);
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            if (initializeOnly)
            {
                _ = charged
                    ? projectiles.FireChargedBeam(SamusTerrain.InRoom(room, SamusRoomObjectFixtures.Uninstalled), samus, shared, GameplayAction.Shoot)
                    : projectiles.FireUnchargedBeam(SamusTerrain.InRoom(room, SamusRoomObjectFixtures.Uninstalled), samus, shared);
                AssertTrue(projectiles.Shots[0].IsOccupied || projectiles.Shots[0].InstructionPointer != 0,
                    $"charged={charged} beam {beamType:X1} initializes");
                AssertEqual(SamusBeamShotBehaviors.Resolve(charged, SamusBeamCombinationTableOrderConstants.FromTableIndex(beamType)),
                    projectiles.Shots[0].Behavior,
                    $"charged={charged} beam {beamType:X1} initializer installs compiled callback");
                continue;
            }
            if (charged)
            {
                for (int frame = 0; frame < 60; frame++)
                {
                    shared.StepFrame(SamusTerrain.InRoom(room, SamusRoomObjectFixtures.Uninstalled), samus, 0, 0);
                    projectiles.StepFrame(SamusTerrain.InRoom(room, SamusRoomObjectFixtures.Uninstalled), samus, new ProjectileButtonInput { ControllerInput = GameplayAction.Shoot, ControllerNewInput = frame == 0 ? GameplayAction.Shoot : 0, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), shared, new SharedGameState(new SharedRandom()));
                }
            }
            SoundRequest? firingSound = projectiles.StepFrameFiringSound(SamusTerrain.InRoom(room, SamusRoomObjectFixtures.Uninstalled), samus, new ProjectileButtonInput { ControllerInput = charged ? 0 : GameplayAction.Shoot, ControllerNewInput = charged ? 0 : GameplayAction.Shoot, ControllerPreviousNewInput = GameplayAction.None }, new(0, 0), shared, new SharedGameState(new SharedRandom()));
            AssertTrue(firingSound is not null,
                $"charged={charged} beam {beamType:X1} fires");
            // The fixture starts with every ordinary slot empty, so the fired shot is the
            // lowest live slot after the release frame.
            SamusProjectile fired = projectiles.Shots.First(slot => slot.IsActive);
            SamusShotFiring expected =
                SamusBeamShotBehaviors.Resolve(charged, SamusBeamCombinationTableOrderConstants.FromTableIndex(beamType));
            AssertEqual(
                expected,
                fired.Behavior,
                $"charged={charged} beam {beamType:X1} installs compiled callback");
        }
        Console.WriteLine(
            "Beam callback definitions: all 32 low-nibble words and 24 production firing paths pass with both source ranges forbidden.");
    }

    /// <summary>
    /// Compares the callback tables with the cartridge, then checks that each beam's initializer installs its compiled
    /// callback without firing it: the stream-one oracle's beam check, its own suite since the stream-one group was split
    /// into its suites (#1757).
    /// </summary>
    internal static void VerifyBeamCallbackInitializers() => VerifyBeamCallbackTables(initializeOnly: true);

    /// <summary>Runs the beam callback table and firing-path verification.</summary>
    [VerificationCommand("--beam-callback-tables")]
    internal static int BeamCallbackTablesCommand()
    {
        Suite(nameof(VerifyBeamCallbackTables), () => VerifyBeamCallbackTables());
        return 0;
    }
}
