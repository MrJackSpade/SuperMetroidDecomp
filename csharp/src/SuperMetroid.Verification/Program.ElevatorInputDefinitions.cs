using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies both native elevator direction masks against real departure behavior while blocking runtime table reads.</summary>
    /// <param name="rom">Cartridge address space supplying the native direction masks and extracted Samus artwork.</param>
    private static void VerifyElevatorInputDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyElevatorDirectionInputMapping), () => VerifyElevatorDirectionInputMapping(rom));
        using var artworkDirectory = new TestTempDirectory("map-catalog");
        SuperMetroid.AssetExtraction.SamusBodyArtworkFiles.Extract(rom, artworkDirectory.Root,
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc")))));
        var bodyArtwork = SuperMetroid.AssetExtraction.SamusBodyArtworkFiles.Load(artworkDirectory.Root, null);
        const int inputTable = 0xa394e2;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (ushort direction = 0; direction < 2; direction++)
        {
            ushort tableByteOffset = (ushort)(direction * 2);
            ushort native = (ushort)(rom.ReadByte(inputTable + tableByteOffset) |
                rom.ReadByte(inputTable + tableByteOffset + 1) << 8);

            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
                enemies,
                new ElevatorInputReadGuard(rom));
            typeof(RoomEnemySystem).GetField("_isAreaBossDefeated", flags)!.SetValue(
                enemies,
                (Func<bool>)(() => true));
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeElevator", flags)!
                .CreateDelegate<Action<RoomEnemySlot, SamusState>>(enemies);
            var wait = typeof(RoomEnemySystem).GetMethod("WaitForElevatorDirectionInput", flags)!
                .CreateDelegate<Action<RoomEnemySlot, ElevatorEnemyState, SamusState, ushort,
                    SamusProjectileSystem?>>(enemies);

            RoomEnemySlot slot = enemies.Slots[0];
            slot.Parameter1 = direction;
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0180;
            var samus = new SamusState();
            samus.TileTransfers.BindArtwork(bodyArtwork);
            initialize(slot, samus);
            AssertEqual(tableByteOffset, enemies.ElevatorStates[0]!.DirectionTableByteOffset,
                $"elevator doubled direction offset {direction}");

            ushort opposite = (ushort)(rom.ReadByte(inputTable + (direction == 0 ? 2 : 0)) |
                rom.ReadByte(inputTable + (direction == 0 ? 3 : 1)) << 8);
            ElevatorActorStatus before = enemies.ElevatorStatus;
            enemies.PublishElevatorDoorContact();
            wait(slot, enemies.ElevatorStates[0]!, samus, opposite, null);
            AssertEqual(before, enemies.ElevatorStatus, "opposite elevator direction cannot start departure");
            AssertEqual(0, enemies.SoundRequests.Count, "opposite elevator input queues no departure sounds");
            enemies.PublishElevatorDoorContact();
            wait(slot, enemies.ElevatorStates[0]!, samus, native, null);
            AssertEqual(ElevatorActorStatus.Departing, enemies.ElevatorStatus,
                $"elevator departure input {direction}");
            AssertEqual(ElevatorFrameEvent.DepartureStarted, enemies.LastElevatorEvent,
                $"elevator departure event {direction}");
            AssertEqual(true, samus.InputLocked, $"elevator Samus input lock {direction}");
            AssertEqual(2, enemies.SoundRequests.Count, $"elevator departure sounds {direction}");
        }

        Console.WriteLine(
            "Elevator input definitions: both native masks and real departure paths pass with table reads forbidden.");
    }

    /// <summary>Checks that the named elevator direction masks match the cartridge table and rejects malformed offsets.</summary>
    /// <param name="rom">Cartridge address space containing the native elevator input table.</param>
    private static void VerifyElevatorDirectionInputMapping(SuperMetroidAddressSpace rom)
    {
        foreach (ushort offset in new ushort[] { 0, 2 })
        {
            int address = 0xa394e2 + offset;
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, ElevatorActorDefinitions.RequiredDirectionInput(offset), "elevator named direction matches native mask");
        }
        foreach (ushort offset in new ushort[] { 1, 3, 4, 5, 0x100, 0x102, 0xfffe, 0xffff })
            AssertThrows<InvalidDataException>(() => ElevatorActorDefinitions.RequiredDirectionInput(offset), "elevator invalid full-word direction offset");
    }
    /// <summary>Wraps an address space to reject reads from the migrated elevator input-mask table.</summary>
    /// <param name="source">Underlying address space used for reads outside the guarded table and for writes.</param>
    private sealed class ElevatorInputReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes a cartridge-byte read through the guard's protected address check.</summary>
        /// <param name="address">Cartridge address of the requested byte.</param>
        /// <returns>The underlying byte unless the address is in the guarded table, in which case the read throws.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Reads a byte while rejecting accesses to the native elevator input-mask table.</summary>
        /// <param name="address">Address of the requested byte.</param>
        /// <returns>The underlying byte when the address is outside the guarded table.</returns>
        public byte ReadByte(int address) => address is >= 0xa394e2 and < 0xa394e6
            ? throw new InvalidOperationException(
                $"Elevator actor attempted migrated input-mask read ${address:X6}.")
            : source.ReadByte(address);

        /// <summary>Forwards a byte write to the underlying address space.</summary>
        /// <param name="address">Address where the byte is written.</param>
        /// <param name="value">Byte value to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
