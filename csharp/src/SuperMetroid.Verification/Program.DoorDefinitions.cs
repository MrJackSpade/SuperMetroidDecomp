using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledDoorDefinitions()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "Door header oracle revision");
        VerifyRetailDoorHeaders(bus);
        VerifyRetailDoorListMapping(bus);
        VerifyCompiledDoorListCollision();
        Console.WriteLine("Doors: all597 physical headers, full elevator overlap and262 room lists match original ROM; collision resolves without native reads.");
    }

    private static void VerifyCompiledDoorListCollision()
    {
        DoorListDefinition landingDoors = DoorDefinitions.GetList(
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
