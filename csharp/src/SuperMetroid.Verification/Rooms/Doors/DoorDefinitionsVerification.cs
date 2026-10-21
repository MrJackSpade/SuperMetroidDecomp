using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Doors.LookupRetailDoorHeadersVerification;
using static SuperMetroid.Verification.Rooms.Doors.LookupRetailDoorListsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled physical door headers, room door lists, and collision resolution.</summary>
internal static class DoorDefinitionsVerification
{
    /// <summary>Runs retail header and list comparisons plus a no-cartridge-read collision check.</summary>
    internal static void VerifyCompiledDoorDefinitions()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bus.Rom)), "Door header oracle revision");
        Suite(nameof(VerifyRetailDoorHeaders), () => VerifyRetailDoorHeaders(bus));
        Suite(nameof(VerifyRetailDoorListMapping), () => VerifyRetailDoorListMapping(bus));
        Suite(nameof(VerifyCompiledDoorListCollision), () => VerifyCompiledDoorListCollision());
        Console.WriteLine("Doors: all597 physical headers, full elevator overlap and262 room lists match original ROM; collision resolves without native reads.");
    }

    /// <summary>Confirms Landing Site door collision resolves the compiled BTS-indexed transition.</summary>
    internal static void VerifyCompiledDoorListCollision()
    {
        IReadOnlyList<DoorListEntry> landingDoors = RoomDoorListDefinitions.ByRoom[RoomHeaderDefinitions.LandingSite];
        var level = new RoomLevelData(new Extent16<RoomBlocks>(1, 1), new RoomLevelPlanes([0], [0], [0]), [], doors: landingDoors);
        CartridgeDoorHeader resolved = (level.ResolveDoorCollision(
            behavior: 0, samusPose: ((SamusPoseId)(0)).ForRoomCollision()).Door ?? throw new InvalidOperationException("Verification requires level.ResolveDoorCollision( NewDoorNoReadAddressSpace(), behavior: 0.ForRoomCollision(), samusPose: 0).Door."));
        AssertEqual(landingDoors[0].Door, resolved,
            "production collision resolves compiled BTS index without a bus read");
        AssertEqual(resolved, level.PendingDoorTransition,
            "compiled collision publishes the selected transition");

    }

    /// <summary>Enumerates both physical door-header runs around the bank-$83 FX data gap.</summary>
    internal static IEnumerable<ushort> EnumerateRetailDoorPointers()
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

    /// <summary>Reads a native door word solely for verification against compiled definitions.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for door definition words")]
    internal static ushort ReadVerificationWord(ISnesAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    /// <summary>Provides an unbacked address space that fails any attempted door collision read or write.</summary>
    internal static CartridgeReadGuard NewDoorNoReadAddressSpace() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Compiled door collision read address ${address:X6}.")
            .RejectWrites(address => $"Compiled door collision wrote address ${address:X6}.");

    /// <summary>Runs the compiled door definition checks against the retail ROM.</summary>
    [VerificationCommand("--door-definitions")]
    internal static int DoorDefinitionsCommand()
    {
        Suite(nameof(VerifyCompiledDoorDefinitions), () => VerifyCompiledDoorDefinitions());
        return 0;
    }
}
