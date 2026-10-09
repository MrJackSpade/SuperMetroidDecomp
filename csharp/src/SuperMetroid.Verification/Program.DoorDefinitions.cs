using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    /// <summary>Compares compiled door headers and room-list mapping with the retail ROM, then verifies bus-free collision lookup.</summary>
    private static void VerifyCompiledDoorDefinitions()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "Door header oracle revision");
        Suite(nameof(VerifyRetailDoorHeaders), () => VerifyRetailDoorHeaders(bus));
        Suite(nameof(VerifyRetailDoorListMapping), () => VerifyRetailDoorListMapping(bus));
        Suite(nameof(VerifyCompiledDoorListCollision), () => VerifyCompiledDoorListCollision());
        Console.WriteLine("Doors: all597 physical headers, full elevator overlap and262 room lists match original ROM; collision resolves without native reads.");
    }

    /// <summary>Resolves a landing-site door collision using compiled definitions while every bus access is rejected.</summary>
    private static void VerifyCompiledDoorListCollision()
    {
        DoorListDefinition landingDoors = DoorDefinitionsTooling.GetList(
            RoomHeaderDefinitions.Get(RoomHeaderPointers.LandingSite).DoorListPointer);
        var level = new RoomLevelData(
            1, 1, [0], [0], [0], [], doorListPointer: landingDoors.Pointer);
        CartridgeDoorHeader resolved = level.ResolveDoorCollision(
            new DoorNoReadAddressSpace(), behavior: 0, samusPose: 0);
        AssertEqual(DoorDefinitions.Get(landingDoors.DoorPointers.Span[0]), resolved,
            "production collision resolves compiled BTS index without a bus read");
        AssertEqual(resolved, level.PendingDoorTransition!,
            "compiled collision publishes the selected transition");

    }

    /// <summary>Enumerates physical door-header pointers from the ROM ranges before and after the FX block.</summary>
    /// <returns>Each aligned bank-$83 door-header pointer in retail table order.</returns>
    private static IEnumerable<ushort> EnumerateRetailDoorPointers()
    {
        for (int pointer = DoorHeaderRomData.PreFxBlockStart;
             pointer <= DoorHeaderRomData.PreFxBlockEnd;
             pointer += DoorHeaderRomData.RecordByteCount)
        {
            yield return checked((ushort)pointer);
        }

        for (int pointer = DoorHeaderRomData.PostFxBlockStart;
             pointer <= DoorHeaderRomData.PostFxBlockEnd;
             pointer += DoorHeaderRomData.RecordByteCount)
        {
            yield return checked((ushort)pointer);
        }
    }

    /// <summary>Reads a little-endian word from the retail address space for comparison with compiled data.</summary>
    /// <param name="bus">Address space supplying the ROM bytes.</param>
    /// <param name="address">Bus address of the low byte.</param>
    /// <returns>The adjacent bytes combined as an unsigned 16-bit word.</returns>
    private static ushort ReadVerificationWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    /// <summary>Address space fixture that fails immediately if door collision attempts any read or write.</summary>
    private sealed class DoorNoReadAddressSpace : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge reads through the rejecting bus-read method.</summary>
        /// <param name="address">Cartridge address requested by collision handling.</param>
        /// <returns>This implementation never returns because all reads are forbidden.</returns>
        /// <exception cref="InvalidOperationException">A cartridge read was attempted.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects every address-space read to prove collision resolution uses compiled data.</summary>
        /// <param name="address">Bus address requested by the caller.</param>
        /// <returns>This implementation never returns because all reads are forbidden.</returns>
        /// <exception cref="InvalidOperationException">A bus read was attempted.</exception>
        public static byte ReadByte(int address) => throw new InvalidOperationException(
            $"Compiled door collision read address ${address:X6}.");

        /// <summary>Rejects writes because the fixture permits no address-space access.</summary>
        /// <param name="address">Bus address that the caller attempted to modify.</param>
        /// <param name="value">Byte value the caller attempted to store.</param>
        /// <exception cref="InvalidOperationException">A bus write was attempted.</exception>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Compiled door collision wrote address ${address:X6}.");
    }

}
