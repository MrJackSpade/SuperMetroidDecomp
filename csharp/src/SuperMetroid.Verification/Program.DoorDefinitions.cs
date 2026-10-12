using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Spatial;
using static LookupRetailDoorHeadersVerification;
using static LookupRetailDoorListsVerification;
using static SuiteTimingVerification;

/// <summary>Represents door definitions verification.</summary>
internal static class DoorDefinitionsVerification
{
    /// <summary>Verifies compiled door definitions.</summary>
    internal static void VerifyCompiledDoorDefinitions()
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

    /// <summary>Verifies compiled door list collision.</summary>
    internal static void VerifyCompiledDoorListCollision()
    {
        DoorListDefinition landingDoors = DoorDefinitionsTooling.GetList(
            RoomHeaderDefinitions.Get(RoomHeaderPointers.LandingSite).DoorListPointer);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(1, 1), new RoomLevelPlanes([0], [0], [0]), [], doorListPointer: landingDoors.Pointer);
        CartridgeDoorHeader resolved = level.ResolveDoorCollision(
            NewDoorNoReadAddressSpace(), behavior: 0, samusPose: 0).Door!;
        AssertEqual(DoorDefinitions.Get(landingDoors.DoorPointers.Span[0]), resolved,
            "production collision resolves compiled BTS index without a bus read");
        AssertEqual(resolved, level.PendingDoorTransition!,
            "compiled collision publishes the selected transition");

    }

    /// <summary>Enumerate retail door pointers.</summary>
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

    /// <summary>Reads verification word.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for door definition words")]
    internal static ushort ReadVerificationWord(ISnesAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    /// <summary>New door no read address space.</summary>
    internal static CartridgeReadGuard NewDoorNoReadAddressSpace() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Compiled door collision read address ${address:X6}.")
            .RejectWrites(address => $"Compiled door collision wrote address ${address:X6}.");

    /// <summary>Door definitions command.</summary>
    [VerificationCommand("--door-definitions")]
    internal static int DoorDefinitionsCommand()
    {
        Suite(nameof(VerifyCompiledDoorDefinitions), () => VerifyCompiledDoorDefinitions());
        return 0;
    }
}
