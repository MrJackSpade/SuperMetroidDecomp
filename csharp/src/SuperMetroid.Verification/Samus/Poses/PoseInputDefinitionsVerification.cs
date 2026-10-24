using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Poses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the compiled Samus pose-input graph against every native transition list.</summary>
internal static class PoseInputDefinitionsVerification
{
    /// <summary>Compares first-match, self-match, and fallback results across the input-bit domain without ROM reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for pose input definitions")]
    internal static void VerifyPoseInputDefinitions(SuperMetroidAddressSpace rom)
    {
        var forbidden = NewSlopeHeightNoReadBus();
        int comparisons = 0;
        var unique = new HashSet<ushort>();
        int conditionCount = 0, newMask = 0, heldMask = 0;
        for (int poseIndex = 0; poseIndex < 253; poseIndex++)
        {
            SamusPoseId pose = (SamusPoseId)poseIndex;
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((SamusMovementDefinitions.Poses.TransitionListPointers + (int)pose * 2));
            var native = new List<PoseInputDefinitionsNative>();
            for (int address = 0x910000 | pointer; CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(address) != ushort.MaxValue; address += 6)
            {
                if (native.Count >= 1000) throw new InvalidDataException("Native pose-input fixture does not terminate.");
                native.Add(new PoseInputDefinitionsNative(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(address), CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((address + 2)), CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((address + 4))));
            }
            AssertTrue(SamusPoseInputDefinitions.TryGetInputList(pose, out SamusPoseInputList actualPointer), "Authored graph exists");
            AssertEqual(pointer, SamusPoseInputListNative.Address(actualPointer), "Native transition-list diagnostic identity");
            AssertEqual(native.Count != 0, SamusPoseInputDefinitions.Match(actualPointer, (SnesButton)ushort.MaxValue, (SnesButton)ushort.MaxValue).HasConditions, "Native empty versus nonempty input program");
            for (int i = 0; i < native.Count; i++)
            {
                newMask |= native[i].New; heldMask |= native[i].Held;
            }
            if (unique.Add(pointer)) conditionCount += native.Count;

            // Native conditions reference eight held bits and six rising-edge bits.
            // Cover their complete cross product, including impossible edge/held pairs.
            // Ignored bits matter only to the initial raw-zero-input branch; test both
            // representatives, not just the masked word.
            for (int held = 0; held < 256; held++)
            for (int pressed = 0; pressed < 64; pressed++)
            for (int noise = 0; noise < 4; noise++)
            {
                SnesButton input = (SnesButton)((held << 4) | ((noise & 1) == 0 ? 0 : 0xf00f));
                SnesButton edges = (SnesButton)((pressed << 6) | ((noise & 2) == 0 ? 0 : 0xf03f));
                SamusPoseTransitionLookup expected;
                if (input == 0) expected = new(null, true);
                else
                {
                    int match = -1;
                    for (int i = 0; i < native.Count; i++)
                        if ((input & (SnesButton)native[i].Held) == (SnesButton)native[i].Held && (edges & (SnesButton)native[i].New) == (SnesButton)native[i].New)
                        { match = i; break; }
                    expected = match < 0 ? new(null, native.Count != 0)
                        : native[match].Target == (ushort)pose ? new(null, false)
                        : new(new SamusPoseTransition((SamusPoseId)native[match].Target), false);
                }
                AssertEqual(expected, SamusPoseTransitionTable.Lookup(pose, input, edges),
                    "Compiled input graph preserves native first-match, self-match and fallback result");
                comparisons++;
            }
        }
        AssertEqual(0x0fc0, newMask, "Fixture enumerates every referenced new-input bit");
        AssertEqual(0x0ff0, heldMask, "Fixture enumerates every referenced held-input bit");
        AssertEqual(86, unique.Count, "All distinct native list identities");
        AssertEqual(598, conditionCount, "All distinct-list conditions");
        for (int pose = 253; pose <= byte.MaxValue; pose++)
        {
            SamusPoseId invalidPose = (SamusPoseId)pose;
            AssertTrue(!SamusPoseInputDefinitions.TryGetInputList(invalidPose, out _), "Trailing pose indexes have no authored graph");
            AssertThrows<InvalidDataException>(
                () => SamusPoseTransitionTable.Lookup(invalidPose, (SnesButton)1, (SnesButton)1),
                "Non-authored pose graph rejects adjacent code");
            AssertEqual(
                new SamusPoseTransitionLookup(null, true),
                SamusPoseTransitionTable.Lookup(invalidPose, 0, 0),
                "Raw zero input returns before the cartridge would inspect a pose graph");
        }

        // Extra held bits are admitted by the native subset matcher. With both horizontal
        // bits set, the first authored condition wins: the direction opposite the current
        // facing appears first in each standing table, so the chord begins a turn.
        const SnesButton leftRight = (SnesButton.Left | SnesButton.Right);
        AssertEqual(
            SamusPoseId.TurningRightToLeftPose,
            (SamusPoseTransitionTable.Lookup(
                SamusPoseId.FacingRightNormalPose,
                leftRight,
                leftRight).Transition ?? throw new InvalidOperationException("Verification requires SamusPoseTransitionTable.Lookup( forbidden, SamusPoseId.FacingRightNormalPose, leftRigh....")).ProspectivePose,
            "right-facing Left+Right uses the first native Left condition");
        AssertEqual(
            SamusPoseId.TurningLeftToRightPose,
            (SamusPoseTransitionTable.Lookup(
                SamusPoseId.FacingLeftNormalPose,
                leftRight,
                leftRight).Transition ?? throw new InvalidOperationException("Verification requires SamusPoseTransitionTable.Lookup( forbidden, SamusPoseId.FacingLeftNormalPose, leftRight....")).ProspectivePose,
            "left-facing Left+Right uses the first native Right condition");

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int accepted = 0;
        for (int i = 0; i < 65536; i++)
            if (SamusPoseTransitionTable.Lookup((SamusPoseId)(i % 253), (SnesButton)(ushort)i, (SnesButton)(ushort)~i).Transition.HasValue) accepted++;
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(accepted > 0, "Allocation probe exercises successful matches");
        AssertEqual(0L, allocated, "Warmed production lookup allocates no per-frame graph or records");
        Console.WriteLine($"Pose input graph: 253 mappings, 86 lists, 598 conditions and {comparisons} complete lookup comparisons match native with ROM reads forbidden; non-authored indexes fail loudly before parsing adjacent code.");
    }

    /// <summary>Runs the focused pose-input graph comparison.</summary>
    [VerificationCommand("--pose-input-definitions")]
    internal static int PoseInputDefinitionsCommand()
    {
        Suite(nameof(VerifyPoseInputDefinitions), () => VerifyPoseInputDefinitions(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
