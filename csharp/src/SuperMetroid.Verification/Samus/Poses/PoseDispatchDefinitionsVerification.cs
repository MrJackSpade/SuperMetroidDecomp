using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Samus.Poses.PoseCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Samus.Poses.PoseProjectileOriginVerification;
using static SuperMetroid.Verification.Samus.Hud.SamusHudDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Poses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled Samus pose dispatch words and allocation-free live lookups.</summary>
internal static class PoseDispatchDefinitionsVerification
{
    /// <summary>Compares all 256 pose records with the ROM while forbidding live pose-table reads.</summary>
    internal static void VerifyPoseDispatchDefinitions(SuperMetroidAddressSpace rom)
    {
        var forbidden = NewSlopeHeightNoReadBus();
        var samus = new SamusState(InstalledFixtures.AnimationContinuation);
        Action<SamusState, ISnesAddressSpace> commit = (state, memory) => state.CommitPoseHistory();
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            int address = SamusSourceDefinitions.PoseDefinitions.Value + pose * SamusSourceDefinitions.PoseDefinitionByteCount;
            byte facing = rom.ReadCartridgeByte(address), movement = rom.ReadCartridgeByte(address + 1), fallback = rom.ReadCartridgeByte(address + 2);
            ISnesAddressSpace source = forbidden;
            samus.Pose = (SamusPoseId)pose;
            byte aim = rom.ReadCartridgeByte(address + 3);
            AssertEqual(aim, SamusPoseMetadata.ReadShotDirection(samus), "Live aim and restrictions retain the full native byte without authored ROM reads");
            AssertEqual(aim, SamusPoseMetadata.ReadShotDirection((SamusPoseId)pose), "Prospective aim retains native byte and adjacent indexes");
            AssertEqual(facing, SamusPoseMetadata.ReadPoseXDirection(samus), "Live pose facing retains native value");
            AssertEqual(facing, SamusPoseMetadata.ReadPoseXDirection((SamusPoseId)pose), "Prospective pose facing retains native value");
            AssertEqual(fallback, (byte)SamusPoseMetadata.ReadNoInputFallbackPose(samus), "No-input fallback retains native pose or sentinel");
            if (movement <= (byte)SamusMovementType.Special)
            {
                AssertEqual(movement, (byte)SamusPoseMetadata.ReadMovementType(samus), "Live movement discriminator matches native");
                AssertEqual(movement, (byte)SamusPoseMetadata.ReadMovementType((SamusPoseId)pose), "Prospective movement discriminator matches native");
                commit(samus, source);
                AssertEqual(SamusSnapshotWords.DecodeHistoryMovement(LittleEndianBytes.Word(facing, movement)), samus.PoseHistory.PreviousMovement,
                    "Actual history publisher retains native facing/movement word");
            }
            else
                AssertThrows<InvalidDataException>(() => SamusPoseMetadata.ReadMovementType(samus), "Adjacent invalid movement remains a loud error");
        }

        // These three compiled tables are queried by ordinary movement, rendering, and
        // no-input transition paths. A collection-expression property can look immutable
        // while still materializing a backing object on every access under a particular
        // compiler/runtime combination, so guard the actual production API rather than
        // assuming its source declaration is allocation-free.
        _ = MeasurePoseDispatchAllocation(out _);
        long allocated = MeasurePoseDispatchAllocation(out int checksum);
        AssertTrue(checksum != 0, "Pose-dispatch allocation probe consumes live table values");
        AssertEqual(0L, allocated,
            "Warmed pose-dispatch lookups allocate no per-frame table storage");

        Console.WriteLine("Pose dispatch: all 256 facing/movement/fallback/aim byte indexes match native with all ROM reads forbidden, including three adjacent-code records; warmed production lookups allocate nothing.");
    }
    /// <summary>Measures allocations across warmed facing, movement, and fallback lookups.</summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static long MeasurePoseDispatchAllocation(out int checksum)
    {
        checksum = 0;
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int index = 0; index < 65536; index++)
        {
            SamusPoseId pose = unchecked((SamusPoseId)(index % 253));
            checksum += SamusPoseMetadata.ReadPoseXDirection(pose);
            checksum += (byte)SamusPoseMetadata.ReadMovementType(pose);
            checksum += (int)SamusPoseDispatch.ReadNoInputPose(pose);
        }
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    /// <summary>Runs pose dispatch, collision, projectile-origin, and HUD definition checks.</summary>
    [VerificationCommand("--pose-dispatch-definitions")]
    internal static int PoseDispatchDefinitionsCommand()
    {
        var poseRom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPoseDispatchDefinitions), () => VerifyPoseDispatchDefinitions(poseRom));
        Suite(nameof(VerifyPoseCollisionDefinitions), () => VerifyPoseCollisionDefinitions(poseRom));
        Suite(nameof(VerifyPoseProjectileOrigin), () => VerifyPoseProjectileOrigin(poseRom));
        Suite(nameof(VerifySamusHudDefinitions), () => VerifySamusHudDefinitions(poseRom));
        return 0;
    }
}
