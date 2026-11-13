using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Diagnostics.Input;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Poses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the compiled Samus pose-input graph against every native transition list.</summary>
internal static class PoseInputDefinitionsVerification
{
    /// <summary>Compares first-match, self-match, and fallback results across the input-bit domain without ROM reads.</summary>
    internal static void VerifyPoseInputDefinitions(CartridgeImportAddressSpace rom)
    {
        var forbidden = NewSlopeHeightNoReadBus();
        int comparisons = 0;
        var unique = new HashSet<ushort>();
        int conditionCount = 0, newMask = 0, heldMask = 0;
        var nativeLists = new List<PoseInputDefinitionsNative>[253];
        for (int poseIndex = 0; poseIndex < 253; poseIndex++)
        {
            if (!SamusPoseNumbering.NamesPose(poseIndex)) continue;
            SamusPoseId pose = SamusPoseNumbering.FromNumber(poseIndex);
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((SamusMovementDefinitions.Poses.TransitionListPointers + SamusPoseNumbers.Number(pose) * 2));
            var native = new List<PoseInputDefinitionsNative>();
            for (int address = 0x910000 | pointer; CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(address) != ushort.MaxValue; address += 6)
            {
                if (native.Count >= 1000) throw new InvalidDataException("Native pose-input fixture does not terminate.");
                native.Add(new PoseInputDefinitionsNative(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(address), CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((address + 2)), CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank((address + 4))));
            }
            AssertTrue(SamusPoseInputs.TryGetInputList(pose, out SamusPoseInputList actualPointer), "Authored graph exists");
            AssertEqual(pointer, SamusPoseInputListNative.Address(actualPointer), "Native transition-list diagnostic identity");
            AssertEqual(native.Count != 0, SamusPoseInputs.Match(actualPointer, ControllerButtons.All, ControllerButtons.All).HasConditions, "Native empty versus nonempty input program");
            for (int i = 0; i < native.Count; i++)
            {
                newMask |= native[i].New; heldMask |= native[i].Held;
            }
            if (unique.Add(pointer)) conditionCount += native.Count;
            nativeLists[poseIndex] = native;
        }

        // Native conditions reference eight held bits and six rising-edge bits.
        // Cover their complete cross product, including impossible edge/held pairs.
        // Ignored buttons matter only to the initial no-input branch; test both
        // representatives, not just the masked word. The joypad words become buttons once, here.
        var heldInputs = new ControllerButton[256 * 2];
        for (int held = 0; held < 256; held++)
        for (int noise = 0; noise < 2; noise++)
            heldInputs[held * 2 + noise] = ControllerWordCodec.FromJoypadWord((ushort)((held << 4) | (noise == 0 ? 0 : 0xf000)), "pose input held word");
        var edgeInputs = new ControllerButton[64 * 2];
        for (int pressed = 0; pressed < 64; pressed++)
        for (int noise = 0; noise < 2; noise++)
            edgeInputs[pressed * 2 + noise] = ControllerWordCodec.FromJoypadWord((ushort)((pressed << 6) | (noise == 0 ? 0 : 0xf030)), "pose input edge word");

        // The noise buttons are ones the compiled lookup provably never reads once the held word is nonzero. The lookup
        // tests the whole held word against zero, removes Start and Select, and every rule of SamusPoseInputRulesEarly
        // and SamusPoseInputRulesLate reads the held and new buttons only through SamusPoseInputs.Has (after
        // MirrorButtons, which only exchanges Left and Right). Has reads only the controller buttons of
        // SamusPoseInputDefinitions.ChordButtons, so B and Y, the held noise, are never read. One lookup therefore
        // stands for both held-noise representatives; the held-noise representative is looked up on its own only for
        // the zero held word, the one case it decides, and its native result is still computed and compared. The
        // new-button noise includes L and R, which are chord buttons, so each new-button representative keeps its own
        // lookup.
        ControllerButton chordButtons = ControllerButton.None;
        foreach (SamusPoseChordButton chord in SamusPoseInputDefinitions.ChordButtons)
            chordButtons |= chord.Button;
        AssertEqual(ControllerButton.None, chordButtons & heldInputs[1], "Held noise buttons are no chord button");

        // Each pose's input domain runs on one worker; the lookup reads only immutable tables.
        ParallelSweep.For(0, 253, poseIndex =>
        {
            if (!SamusPoseNumbering.NamesPose(poseIndex)) return;
            SamusPoseId pose = SamusPoseNumbering.FromNumber(poseIndex);
            List<PoseInputDefinitionsNative> native = nativeLists[poseIndex];
            ControllerButton[] nativeHeld = [.. native.Select(condition => ControllerWordCodec.FromJoypadWord(condition.Held, "native pose condition held word"))];
            ControllerButton[] nativeNew = [.. native.Select(condition => ControllerWordCodec.FromJoypadWord(condition.New, "native pose condition new word"))];
            for (int held = 0; held < 256; held++)
            for (int pressed = 0; pressed < 64; pressed++)
            {
                for (int edgeNoise = 0; edgeNoise < 2; edgeNoise++)
                {
                    ControllerButton edges = edgeInputs[pressed * 2 + edgeNoise];
                    SamusPoseTransitionLookup actual = default;
                    for (int heldNoise = 0; heldNoise < 2; heldNoise++)
                    {
                        ControllerButton input = heldInputs[held * 2 + heldNoise];
                        if (heldNoise == 0 || held == 0)
                            actual = SamusPoseTransitionTable.Lookup(pose, input, edges);
                        AssertEqual(NativeLookup(native, nativeHeld, nativeNew, pose, input, edges), actual,
                            "Compiled input graph preserves native first-match, self-match and fallback result");
                    }
                }
            }
            Interlocked.Add(ref comparisons, 256 * 64 * 4);
        });
        AssertEqual(0x0fc0, newMask, "Fixture enumerates every referenced new-input bit");
        AssertEqual(0x0ff0, heldMask, "Fixture enumerates every referenced held-input bit");
        AssertEqual(79, unique.Count, "All distinct native list identities of the records that name a pose");
        AssertEqual(590, conditionCount, "All distinct-list conditions of the records that name a pose");
        for (int pose = 253; pose <= byte.MaxValue; pose++)
        {
            SamusPoseId invalidPose = SamusPoseNumbering.FromNumber(pose);
            AssertTrue(!SamusPoseInputs.TryGetInputList(invalidPose, out _), "Trailing pose indexes have no authored graph");
            AssertThrows<InvalidDataException>(
                () => SamusPoseTransitionTable.Lookup(invalidPose, ControllerButton.R, ControllerButton.R),
                "Non-authored pose graph rejects adjacent code");
            AssertEqual(
                new SamusPoseTransitionLookup(null, true),
                SamusPoseTransitionTable.Lookup(invalidPose, 0, 0),
                "Raw zero input returns before the cartridge would inspect a pose graph");
        }

        // Extra held bits are admitted by the native subset matcher. With both horizontal
        // bits set, the first authored condition wins: the direction opposite the current
        // facing appears first in each standing table, so the chord begins a turn.
        const ControllerButton leftRight = (ControllerButton.Left | ControllerButton.Right);
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

        int buttonBits = ControllerWordCodec.ToJoypadWord(ControllerButtons.All);
        var probeHeld = new ControllerButton[65536];
        var probeEdges = new ControllerButton[65536];
        for (int i = 0; i < 65536; i++)
        {
            probeHeld[i] = ControllerWordCodec.FromJoypadWord((ushort)(i & buttonBits), "allocation probe held word");
            probeEdges[i] = ControllerWordCodec.FromJoypadWord((ushort)(~i & buttonBits), "allocation probe edge word");
        }
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int accepted = 0;
        for (int i = 0; i < 65536; i++)
        {
            if (!SamusPoseNumbering.NamesPose(i % 253)) continue;
            if (SamusPoseTransitionTable.Lookup(SamusPoseNumbering.FromNumber(i % 253), probeHeld[i], probeEdges[i]).Transition.HasValue) accepted++;
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(accepted > 0, "Allocation probe exercises successful matches");
        AssertEqual(0L, allocated, "Warmed production lookup allocates no per-frame graph or records");
        Console.WriteLine($"Pose input graph: the mappings of the records that name a pose, 79 lists, 590 conditions and {comparisons} complete lookup comparisons match native with ROM reads forbidden; non-authored indexes fail loudly before parsing adjacent code.");
    }

    /// <summary>The native transition list's first-match, self-match and fallback result for one held and new input.</summary>
    private static SamusPoseTransitionLookup NativeLookup(List<PoseInputDefinitionsNative> native, ControllerButton[] nativeHeld,
        ControllerButton[] nativeNew, SamusPoseId pose, ControllerButton input, ControllerButton edges)
    {
        if (input == 0) return new(null, true);
        for (int i = 0; i < native.Count; i++)
        {
            if ((input & nativeHeld[i]) == nativeHeld[i] && (edges & nativeNew[i]) == nativeNew[i])
            {
                return native[i].Target == SamusPoseNumbers.Number(pose) ? new(null, false)
                    : new(new SamusPoseTransition(SamusPoseNumbering.FromNumber(native[i].Target)), false);
            }
        }
        return new(null, native.Count != 0);
    }

    /// <summary>Runs the focused pose-input graph comparison.</summary>
    [VerificationCommand("--pose-input-definitions")]
    internal static int PoseInputDefinitionsCommand()
    {
        Suite(nameof(VerifyPoseInputDefinitions), () => VerifyPoseInputDefinitions(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
