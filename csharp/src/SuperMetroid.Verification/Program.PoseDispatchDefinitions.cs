using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static PoseCollisionDefinitionsVerification;
using static PoseProjectileOriginVerification;
using static Program;
using static SamusHudDefinitionsVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SharedSlopeHeightsVerification;

internal static class PoseDispatchDefinitionsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for pose dispatch definitions")]
    internal static void VerifyPoseDispatchDefinitions(SuperMetroidAddressSpace rom)
    {
        var forbidden = NewSlopeHeightNoReadBus();
        var samus = new SamusState();
        var commit = typeof(SamusState).GetMethod("CommitPoseHistory",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .CreateDelegate<Action<SamusState, ISnesAddressSpace>>();
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            int address = SamusMovementRomData.Poses.Definitions + pose * SamusMovementRomData.Poses.DefinitionByteCount;
            byte facing = rom.ReadCartridgeByte(address), movement = rom.ReadCartridgeByte(address + 1), fallback = rom.ReadCartridgeByte(address + 2);
            ISnesAddressSpace source = forbidden;
            samus.Pose = (SamusPoseId)pose;
            byte aim = rom.ReadCartridgeByte(address + 3);
            AssertEqual(aim, samus.ReadShotDirection(source), "Live aim and restrictions retain the full native byte without authored ROM reads");
            AssertEqual(aim, SamusState.ReadShotDirection(source, (SamusPoseId)pose), "Prospective aim retains native byte and adjacent indexes");
            AssertEqual(facing, samus.ReadPoseXDirection(source), "Live pose facing retains native value");
            AssertEqual(facing, SamusState.ReadPoseXDirection(source, (SamusPoseId)pose), "Prospective pose facing retains native value");
            AssertEqual(fallback, (byte)samus.ReadNoInputFallbackPose(source), "No-input fallback retains native pose or sentinel");
            if (movement <= (byte)SamusMovementType.Special)
            {
                AssertEqual(movement, (byte)samus.ReadMovementType(source), "Live movement discriminator matches native");
                AssertEqual(movement, (byte)SamusState.ReadMovementType(source, (SamusPoseId)pose), "Prospective movement discriminator matches native");
                commit(samus, source);
                AssertEqual((ushort)(SnesLittleEndian.Word(facing, movement)), samus.PoseHistory.PreviousDirectionAndMovement,
                    "Actual history publisher retains native facing/movement word");
            }
            else
                AssertThrows<InvalidDataException>(() => samus.ReadMovementType(source), "Adjacent invalid movement remains a loud error");
        }

        // These three compiled tables are queried by ordinary movement, rendering, and
        // no-input transition paths. A collection-expression property can look immutable
        // while still materializing a backing object on every access under a particular
        // compiler/runtime combination, so guard the actual production API rather than
        // assuming its source declaration is allocation-free.
        _ = MeasurePoseDispatchAllocation(forbidden, out _);
        long allocated = MeasurePoseDispatchAllocation(forbidden, out int checksum);
        AssertTrue(checksum != 0, "Pose-dispatch allocation probe consumes live table values");
        AssertEqual(0L, allocated,
            "Warmed pose-dispatch lookups allocate no per-frame table storage");

        Console.WriteLine("Pose dispatch: all 256 facing/movement/fallback/aim byte indexes match native with all ROM reads forbidden, including three adjacent-code records; warmed production lookups allocate nothing.");
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static long MeasurePoseDispatchAllocation(ISnesAddressSpace forbidden, out int checksum)
    {
        checksum = 0;
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int index = 0; index < 65536; index++)
        {
            SamusPoseId pose = unchecked((SamusPoseId)(index % 253));
            checksum += SamusState.ReadPoseXDirection(forbidden, pose);
            checksum += (byte)SamusState.ReadMovementType(forbidden, pose);
            checksum += (int)SamusPoseDispatchDefinitions.ReadNoInputPose(pose);
        }
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [VerificationCommand("--pose-dispatch-definitions")]
    internal static int PoseDispatchDefinitionsCommand()
    {
        var poseRom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPoseDispatchDefinitions), () => VerifyPoseDispatchDefinitions(poseRom));
        Suite(nameof(VerifyPoseCollisionDefinitions), () => VerifyPoseCollisionDefinitions(poseRom));
        Suite(nameof(VerifyPoseProjectileOrigin), () => VerifyPoseProjectileOrigin(poseRom));
        Suite(nameof(VerifySamusHudDefinitions), () => VerifySamusHudDefinitions(poseRom));
        return 0;
    }
}
