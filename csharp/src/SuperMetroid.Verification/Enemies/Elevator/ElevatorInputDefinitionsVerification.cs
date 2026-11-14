using SuperMetroid.Diagnostics.Input;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Elevator;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks compiled elevator direction masks against native data and guarded departure behavior.</summary>
internal static class ElevatorInputDefinitionsVerification
{
    /// <summary>Confirms both direction offsets and departure paths while forbidding migrated mask reads at runtime.</summary>
    internal static void VerifyElevatorInputDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyElevatorDirectionInputMapping), () => VerifyElevatorDirectionInputMapping(rom));
        var bodyArtwork = InstalledFixtures.SamusBody;
        const int inputTable = 0xa394e2;

        for (ushort direction = 0; direction < 2; direction++)
        {
            ushort tableByteOffset = (ushort)(direction * 2);
            ElevatorTravelDirection travel = direction == 0 ? ElevatorTravelDirection.Down : ElevatorTravelDirection.Up;
            ControllerButton native = ControllerWordCodec.FromJoypadWord(CartridgeImportSource.Require(rom).ReadCartridgeWord(inputTable + tableByteOffset), "native elevator departure mask");

            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().IsAreaBossDefeated = () => true;
            Action<RoomEnemySlot, SamusState> initialize = enemies.ElevatorBehavior.InitializeElevator;
            Action<RoomEnemySlot, ElevatorEnemyState, SamusState, ControllerButton,
                    SamusProjectileSystem?> wait = enemies.ElevatorBehavior.WaitForElevatorDirectionInput;

            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Elevator, slot.Definition);
            slot.SetSpawnParameter1(EnemyDefinitionId.Elevator, direction);
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0180));
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
            samus.Presentation.BindBodyFixture(bodyArtwork);
            initialize(slot, samus);
            ElevatorEnemyState elevator = enemies.ElevatorStates[0] ?? throw new InvalidOperationException("Verification requires enemies.ElevatorStates[0].");
            AssertEqual(travel, elevator.Direction,
                $"elevator parameter {direction} travels {travel}");

            ControllerButton opposite = ControllerWordCodec.FromJoypadWord(LittleEndianBytes.Word(rom.ReadCartridgeByte(inputTable + (direction == 0 ? 2 : 0)), rom.ReadCartridgeByte(inputTable + (direction == 0 ? 3 : 1))), "native opposite elevator mask");
            ElevatorActorStatus before = enemies.ElevatorBehavior.ElevatorStatus;
            enemies.ElevatorBehavior.PublishElevatorDoorContact();
            wait(slot, elevator, samus, opposite, null);
            AssertEqual(before, enemies.ElevatorBehavior.ElevatorStatus, "opposite elevator direction cannot start departure");
            AssertEqual(0, enemies.Audio.SoundRequests.Count, "opposite elevator input queues no departure sounds");
            enemies.ElevatorBehavior.PublishElevatorDoorContact();
            wait(slot, elevator, samus, native, null);
            AssertEqual(ElevatorActorStatus.Departing, enemies.ElevatorBehavior.ElevatorStatus,
                $"elevator departure input {direction}");
            AssertEqual(ElevatorFrameEvent.DepartureStarted, enemies.LastEnemyFrame.Elevator,
                $"elevator departure event {direction}");
            AssertEqual(true, samus.Control.InputLocked, $"elevator Samus input lock {direction}");
            AssertEqual(2, enemies.Audio.SoundRequests.Count, $"elevator departure sounds {direction}");
        }

        Console.WriteLine(
            "Elevator input definitions: both native masks and real departure paths pass with table reads forbidden.");
    }

    /// <summary>Checks both directions' inputs against the two-entry mask table at $A3:94E2.</summary>
    private static void VerifyElevatorDirectionInputMapping(CartridgeImportAddressSpace rom)
    {
        ElevatorTravelDirection[] tableOrder = [ElevatorTravelDirection.Down, ElevatorTravelDirection.Up];
        for (int entry = 0; entry < tableOrder.Length; entry++)
        {
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa394e2 + entry * 2);
            AssertEqual(ControllerWordCodec.FromJoypadWord(native, "native elevator direction mask"), ElevatorActorDefinitions.RequiredDirectionInputs[tableOrder[entry]], "elevator named direction matches native mask");
        }
    }

}
