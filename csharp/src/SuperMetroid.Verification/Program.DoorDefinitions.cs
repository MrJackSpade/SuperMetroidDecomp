using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
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

    private static void VerifyCompiledDoorListCollision()
    {
        DoorListDefinition landingDoors = DoorDefinitionsTooling.GetList(
            RoomHeaderDefinitions.Get(RoomHeaderPointers.LandingSite).DoorListPointer);
        var level = new RoomLevelData(
            1, 1, [0], [0], [0], [], doorListPointer: landingDoors.Pointer);
        CartridgeDoorHeader resolved = level.ResolveDoorCollision(
            new DoorNoReadAddressSpace(), behavior: 0, samusPose: 0).Door!;
        AssertEqual(DoorDefinitions.Get(landingDoors.DoorPointers.Span[0]), resolved,
            "production collision resolves compiled BTS index without a bus read");
        AssertEqual(resolved, level.PendingDoorTransition!,
            "compiled collision publishes the selected transition");

    }

    private static IEnumerable<ushort> EnumerateRetailDoorPointers()
    {
        for (int pointer = DoorHeaderRomDataConstants.PreFxBlockStart;
             pointer <= DoorHeaderRomDataConstants.PreFxBlockEnd;
             pointer += DoorHeaderRomDataConstants.RecordByteCount)
        {
            yield return checked((ushort)pointer);
        }

        for (int pointer = DoorHeaderRomDataConstants.PostFxBlockStart;
             pointer <= DoorHeaderRomDataConstants.PostFxBlockEnd;
             pointer += DoorHeaderRomDataConstants.RecordByteCount)
        {
            yield return checked((ushort)pointer);
        }
    }

    private static ushort ReadVerificationWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private sealed class DoorNoReadAddressSpace : ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public static byte ReadByte(int address) => throw new InvalidOperationException(
            $"Compiled door collision read address ${address:X6}.");

        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Compiled door collision wrote address ${address:X6}.");
    }

}
