using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Diagnostics.Input;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.SamusNativeCodecs;
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
        var liveConditionsByList = new Dictionary<SamusPoseInputList, string>();
        var listByLiveConditions = new Dictionary<string, SamusPoseInputList>(StringComparer.Ordinal);
        GameplayAction allActions = ControllerBindings.Default.ToActions(ControllerButtons.All);
        for (int poseIndex = 0; poseIndex < 253; poseIndex++)
        {
            if (!SamusPoseNumbering.NamesPose(poseIndex)) continue;
            SamusPoseId pose = SamusPoseNumbering.FromNumber(poseIndex);
            ushort pointer = rom.ReadCartridgeWordFixedBank((SamusMovementDefinitionsPosesConstants.TransitionListPointers + SamusPoseNumbers.Number(pose) * 2));
            var native = new List<PoseInputDefinitionsNative>();
            for (int address = 0x910000 | pointer; rom.ReadCartridgeWordFixedBank(address) != ushort.MaxValue; address += 6)
            {
                if (native.Count >= 1000) throw new InvalidDataException("Native pose-input fixture does not terminate.");
                native.Add(new PoseInputDefinitionsNative(rom.ReadCartridgeWordFixedBank(address), rom.ReadCartridgeWordFixedBank((address + 2)), rom.ReadCartridgeWordFixedBank((address + 4))));
            }
            SamusPoseInputList inputList = SamusPoseTraits.Of(pose).Inputs;
            // Poses share a list exactly when their native lists hold the same live conditions, the conditions no earlier
            // condition of the list shadows (#1929, #2457).
            string live = LiveConditions(native);
            AssertEqual(live, liveConditionsByList.TryAdd(inputList, live) ? live : liveConditionsByList[inputList],
                $"Pose {pose} shares list {inputList} only with poses whose native lists hold its live conditions");
            AssertEqual(inputList, listByLiveConditions.TryAdd(live, inputList) ? inputList : listByLiveConditions[live],
                $"Pose {pose}'s live native conditions select one input list");
            AssertEqual(native.Count != 0, SamusPoseInputRules.Match(inputList, allActions, allActions).HasConditions, "Native empty versus nonempty input program");
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
        // representatives, not just the masked word. The joypad words become actions once, here, through the default
        // bindings the native tables are authored in.
        var heldInputs = new GameplayAction[256 * 2];
        for (int held = 0; held < 256; held++)
        for (int noise = 0; noise < 2; noise++)
            heldInputs[held * 2 + noise] = GameplayActionWordCodec.FromJoypadWord((ushort)((held << 4) | (noise == 0 ? 0 : 0xf000)), "pose input held word");
        var edgeInputs = new GameplayAction[64 * 2];
        for (int pressed = 0; pressed < 64; pressed++)
        for (int noise = 0; noise < 2; noise++)
            edgeInputs[pressed * 2 + noise] = GameplayActionWordCodec.FromJoypadWord((ushort)((pressed << 6) | (noise == 0 ? 0 : 0xf030)), "pose input edge word");

        // The noise actions are ones the compiled lookup provably never reads once the held actions are not none. The
        // lookup tests the held actions against none, removes Start and item selection, and every rule of
        // SamusPoseInputRules reads the held and new actions only through SamusPoseInputs.Has (after MirrorButtons,
        // which only exchanges Left and Right), naming only the directions, the aims, Shoot and Jump, so Dash and item
        // cancel, the held noise, are never read. One lookup therefore stands for both held-noise representatives; the
        // held-noise representative is looked up on its own only for the empty held actions, the one case it decides,
        // and its native result is still computed and compared. The new-action noise includes both aims, which chords
        // name, so each new-action representative keeps its own lookup.
        const GameplayAction chordActions = GameplayActions.Directions | GameplayAction.AimUp | GameplayAction.AimDown |
            GameplayAction.Shoot | GameplayAction.Jump;
        AssertEqual(GameplayAction.None, chordActions & heldInputs[1], "Held noise actions are no chord action");

        // Each pose's input domain runs on one worker; the lookup reads only immutable tables.
        ParallelSweep.For(0, 253, poseIndex =>
        {
            if (!SamusPoseNumbering.NamesPose(poseIndex)) return;
            SamusPoseId pose = SamusPoseNumbering.FromNumber(poseIndex);
            List<PoseInputDefinitionsNative> native = nativeLists[poseIndex];
            GameplayAction[] nativeHeld = [.. native.Select(condition => GameplayActionWordCodec.FromJoypadWord(condition.Held, "native pose condition held word"))];
            GameplayAction[] nativeNew = [.. native.Select(condition => GameplayActionWordCodec.FromJoypadWord(condition.New, "native pose condition new word"))];
            for (int held = 0; held < 256; held++)
            for (int pressed = 0; pressed < 64; pressed++)
            {
                for (int edgeNoise = 0; edgeNoise < 2; edgeNoise++)
                {
                    GameplayAction edges = edgeInputs[pressed * 2 + edgeNoise];
                    SamusPoseTransitionLookup actual = SamusPoseTransitionLookup.KeepsPose;
                    for (int heldNoise = 0; heldNoise < 2; heldNoise++)
                    {
                        GameplayAction input = heldInputs[held * 2 + heldNoise];
                        if (heldNoise == 0 || held == 0)
                            actual = SamusPoseInputTransitions.Lookup(pose, input, edges);
                        AssertEqual(NativeLookup(native, nativeHeld, nativeNew, pose, input, edges),
                            new NativeResult(actual.Transition?.ProspectivePose, actual.UsesPoseDefinitionFallback),
                            "Compiled input graph preserves native first-match, self-match and fallback result");
                    }
                }
            }
            Interlocked.Add(ref comparisons, 256 * 64 * 4);
        });
        AssertEqual(0x0fc0, newMask, "Fixture enumerates every referenced new-input bit");
        AssertEqual(0x0ff0, heldMask, "Fixture enumerates every referenced held-input bit");
        AssertEqual(75, unique.Count, "All distinct native list identities of the records that name a pose");
        AssertEqual(576, conditionCount, "All distinct-list conditions of the records that name a pose");
        AssertEqual(69, liveConditionsByList.Count, "Every one of the 69 input lists is some pose's");

        // Extra held actions are admitted by the native subset matcher. With both horizontal
        // directions held, the first authored condition wins: the direction opposite the current
        // facing appears first in each standing table, so the chord begins a turn.
        const GameplayAction leftRight = GameplayActions.HorizontalDirections;
        AssertEqual(
            SamusPoseId.TurningRightToLeftPose,
            (SamusPoseInputTransitions.Lookup(
                SamusPoseId.FacingRightNormalPose,
                leftRight,
                leftRight).Transition ?? throw new InvalidOperationException("Verification requires SamusPoseInputTransitions.Lookup( forbidden, SamusPoseId.FacingRightNormalPose, leftRigh....")).ProspectivePose,
            "right-facing Left+Right uses the first native Left condition");
        AssertEqual(
            SamusPoseId.TurningLeftToRightPose,
            (SamusPoseInputTransitions.Lookup(
                SamusPoseId.FacingLeftNormalPose,
                leftRight,
                leftRight).Transition ?? throw new InvalidOperationException("Verification requires SamusPoseInputTransitions.Lookup( forbidden, SamusPoseId.FacingLeftNormalPose, leftRight....")).ProspectivePose,
            "left-facing Left+Right uses the first native Right condition");

        int buttonBits = ControllerWordCodec.ToJoypadWord(ControllerButtons.All);
        var probeHeld = new GameplayAction[65536];
        var probeEdges = new GameplayAction[65536];
        for (int i = 0; i < 65536; i++)
        {
            probeHeld[i] = GameplayActionWordCodec.FromJoypadWord((ushort)(i & buttonBits), "allocation probe held word");
            probeEdges[i] = GameplayActionWordCodec.FromJoypadWord((ushort)(~i & buttonBits), "allocation probe edge word");
        }
        // One pass warms the lookup: each outcome it reaches is made the first time it is reached, and kept.
        int Probe()
        {
            int matches = 0;
            for (int i = 0; i < 65536; i++)
            {
                if (!SamusPoseNumbering.NamesPose(i % 253)) continue;
                if (SamusPoseInputTransitions.Lookup(SamusPoseNumbering.FromNumber(i % 253), probeHeld[i], probeEdges[i]).Transition.HasValue) matches++;
            }
            return matches;
        }
        _ = Probe();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int accepted = Probe();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(accepted > 0, "Allocation probe exercises successful matches");
        AssertEqual(0L, allocated, "Warmed production lookup allocates no per-frame graph or records");
        Console.WriteLine($"Pose input graph: the mappings of the records that name a pose, 75 native lists, 576 conditions, {liveConditionsByList.Count} shared input lists and {comparisons} complete lookup comparisons match native with ROM reads forbidden.");
    }

    /// <summary>
    /// The live conditions of a native transition list, one per line: every condition but those an earlier condition
    /// shadows, whose required new and held inputs are each part of the later one's, so the earlier one always wins first.
    /// </summary>
    private static string LiveConditions(List<PoseInputDefinitionsNative> native) =>
        string.Join('\n', native
            .Where((condition, index) => !native.Take(index).Any(earlier =>
                (earlier.New & ~condition.New) == 0 && (earlier.Held & ~condition.Held) == 0))
            .Select(condition => $"{condition.New:X4} {condition.Held:X4} {condition.Target:X2}"));

    /// <summary>
    /// The pose a lookup chooses, if any, and whether it fell through to the pose-definition fallback: what the native
    /// transition lists decide. The setup each compiled transition names is the pose update's, which they do not hold.
    /// </summary>
    /// <param name="Pose">The pose chosen, or none.</param>
    /// <param name="UsesPoseDefinitionFallback">Whether the lookup fell through to the pose-definition fallback.</param>
    private readonly record struct NativeResult(SamusPoseId? Pose, bool UsesPoseDefinitionFallback);

    /// <summary>The native transition list's first-match, self-match and fallback result for one held and new input.</summary>
    private static NativeResult NativeLookup(List<PoseInputDefinitionsNative> native, GameplayAction[] nativeHeld,
        GameplayAction[] nativeNew, SamusPoseId pose, GameplayAction input, GameplayAction edges)
    {
        if (input == 0) return new(null, true);
        for (int i = 0; i < native.Count; i++)
        {
            if ((input & nativeHeld[i]) == nativeHeld[i] && (edges & nativeNew[i]) == nativeNew[i])
            {
                return native[i].Target == SamusPoseNumbers.Number(pose) ? new(null, false)
                    : new(SamusPoseNumbering.FromNumber(native[i].Target), false);
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
