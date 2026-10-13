using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Checks compiled elevator direction masks against native data and guarded departure behavior.</summary>
internal static class ElevatorInputDefinitionsVerification
{
    /// <summary>Confirms both direction offsets and departure paths while forbidding migrated mask reads at runtime.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for elevator input definitions")]
    internal static void VerifyElevatorInputDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyElevatorDirectionInputMapping), () => VerifyElevatorDirectionInputMapping(rom));
        using var artworkDirectory = new TestScratchDirectory("map-catalog");
        SuperMetroid.AssetExtraction.SamusBodyArtworkFiles.Extract(rom, artworkDirectory.Root,
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc")))));
        var bodyArtwork = SuperMetroid.AssetExtraction.SamusBodyArtworkFiles.Load(artworkDirectory.Root, null);
        const int inputTable = 0xa394e2;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (ushort direction = 0; direction < 2; direction++)
        {
            ushort tableByteOffset = (ushort)(direction * 2);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(inputTable + tableByteOffset);

            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(NewElevatorInputReadGuard(rom));
            enemies.VerificationServices().IsAreaBossDefeated = (Func<bool>)(() => true);
            Action<RoomEnemySlot, SamusState> initialize = enemies.ElevatorBehavior.InitializeElevator;
            Action<RoomEnemySlot, ElevatorEnemyState, SamusState, ushort,
                    SamusProjectileSystem?> wait = enemies.ElevatorBehavior.WaitForElevatorDirectionInput;

            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Parameter1 = direction;
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0180));
            var samus = new SamusState();
            samus.Presentation.TileTransfers.BindArtwork(bodyArtwork);
            initialize(slot, samus);
            AssertEqual(tableByteOffset, enemies.ElevatorStates[0]!.DirectionTableByteOffset,
                $"elevator doubled direction offset {direction}");

            ushort opposite = (ushort)(SnesLittleEndian.Word(rom.ReadCartridgeByte(inputTable + (direction == 0 ? 2 : 0)), rom.ReadCartridgeByte(inputTable + (direction == 0 ? 3 : 1))));
            ElevatorActorStatus before = enemies.ElevatorBehavior.ElevatorStatus;
            enemies.ElevatorBehavior.PublishElevatorDoorContact();
            wait(slot, enemies.ElevatorStates[0]!, samus, opposite, null);
            AssertEqual(before, enemies.ElevatorBehavior.ElevatorStatus, "opposite elevator direction cannot start departure");
            AssertEqual(0, enemies.Audio.SoundRequests.Count, "opposite elevator input queues no departure sounds");
            enemies.ElevatorBehavior.PublishElevatorDoorContact();
            wait(slot, enemies.ElevatorStates[0]!, samus, native, null);
            AssertEqual(ElevatorActorStatus.Departing, enemies.ElevatorBehavior.ElevatorStatus,
                $"elevator departure input {direction}");
            AssertEqual(ElevatorFrameEvent.DepartureStarted, enemies.ElevatorBehavior.LastElevatorEvent,
                $"elevator departure event {direction}");
            AssertEqual(true, samus.InputLocked, $"elevator Samus input lock {direction}");
            AssertEqual(2, enemies.Audio.SoundRequests.Count, $"elevator departure sounds {direction}");
        }

        Console.WriteLine(
            "Elevator input definitions: both native masks and real departure paths pass with table reads forbidden.");
    }

    /// <summary>Checks the two supported word offsets against $A3:94E2 and rejects malformed offsets.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for elevator direction input mapping")]
    private static void VerifyElevatorDirectionInputMapping(SuperMetroidAddressSpace rom)
    {
        foreach (ushort offset in new ushort[] { 0, 2 })
        {
            int address = 0xa394e2 + offset;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, ElevatorActorDefinitions.RequiredDirectionInput(offset), "elevator named direction matches native mask");
        }
        foreach (ushort offset in new ushort[] { 1, 3, 4, 5, 0x100, 0x102, 0xfffe, 0xffff })
            AssertThrows<InvalidDataException>(() => ElevatorActorDefinitions.RequiredDirectionInput(offset), "elevator invalid full-word direction offset");
    }
    /// <summary>Allows fixture cartridge access except for the elevator input table now owned by compiled definitions.</summary>
    private static CartridgeReadGuard NewElevatorInputReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0xa394e2 and < 0xa394e6,
                address => $"Elevator actor attempted migrated input-mask read ${address:X6}.");
}
