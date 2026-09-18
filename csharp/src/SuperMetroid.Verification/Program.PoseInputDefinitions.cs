using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyPoseInputDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8);
        var forbidden = new SlopeHeightNoReadBus();
        int comparisons = 0;
        var unique = new HashSet<ushort>();
        int conditionCount = 0, newMask = 0, heldMask = 0;
        for (int poseIndex = 0; poseIndex < 253; poseIndex++)
        {
            byte pose = (byte)poseIndex;
            ushort pointer = Word(SamusMovementRomData.Poses.TransitionListPointers + pose * 2);
            var native = new List<(ushort New, ushort Held, ushort Target)>();
            for (int address = 0x910000 | pointer; Word(address) != ushort.MaxValue; address += 6)
            {
                if (native.Count >= 1000) throw new InvalidDataException("Native pose-input fixture does not terminate.");
                native.Add((Word(address), Word(address + 2), Word(address + 4)));
            }
            AssertTrue(SamusPoseInputDefinitions.TryGet(pose, out ushort actualPointer, out var rules), "Authored graph exists");
            AssertEqual(pointer, actualPointer, "Native transition-list diagnostic identity");
            AssertEqual(native.Count, rules.Length, "Native transition-list length and empty terminator");
            for (int i = 0; i < native.Count; i++)
            {
                AssertEqual(native[i], (rules[i].RequiredNewInput, rules[i].RequiredHeldInput, rules[i].TargetPose), "Every native condition and target in order");
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
                ushort input = (ushort)((held << 4) | ((noise & 1) == 0 ? 0 : 0xf00f));
                ushort edges = (ushort)((pressed << 6) | ((noise & 2) == 0 ? 0 : 0xf03f));
                SamusPoseTransitionLookup expected;
                if (input == 0) expected = new(null, true);
                else
                {
                    int match = -1;
                    for (int i = 0; i < native.Count; i++)
                        if ((input & native[i].Held) == native[i].Held && (edges & native[i].New) == native[i].New)
                        { match = i; break; }
                    expected = match < 0 ? new(null, native.Count != 0)
                        : native[match].Target == pose ? new(null, false)
                        : new(new SamusPoseTransition(pose, native[match].Target, native[match].New,
                            native[match].Held, 0x910000 | (pointer + match * 6)), false);
                }
                AssertEqual(expected, SamusPoseTransitionTable.Lookup(forbidden, pose, input, edges),
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
            byte invalidPose = (byte)pose;
            AssertTrue(!SamusPoseInputDefinitions.TryGet(invalidPose, out _, out _), "Trailing pose indexes have no authored graph");
            AssertThrows<InvalidDataException>(
                () => SamusPoseTransitionTable.Lookup(forbidden, invalidPose, 1, 1),
                "Non-authored pose graph rejects adjacent code");
            AssertEqual(
                new SamusPoseTransitionLookup(null, true),
                SamusPoseTransitionTable.Lookup(forbidden, invalidPose, 0, 0),
                "Raw zero input returns before the cartridge would inspect a pose graph");
        }

        // Extra held bits are admitted by the native subset matcher. With both horizontal
        // bits set, the first authored condition wins: the direction opposite the current
        // facing appears first in each standing table, so the chord begins a turn.
        const ushort leftRight = (ushort)(SnesButton.Left | SnesButton.Right);
        AssertEqual(
            (ushort)SamusPoseIds.TurningRightToLeftPose,
            SamusPoseTransitionTable.Lookup(
                forbidden,
                SamusPoseIds.FacingRightNormalPose,
                leftRight,
                leftRight).Transition!.Value.ProspectivePose,
            "right-facing Left+Right uses the first native Left condition");
        AssertEqual(
            (ushort)SamusPoseIds.TurningLeftToRightPose,
            SamusPoseTransitionTable.Lookup(
                forbidden,
                SamusPoseIds.FacingLeftNormalPose,
                leftRight,
                leftRight).Transition!.Value.ProspectivePose,
            "left-facing Left+Right uses the first native Right condition");

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int accepted = 0;
        for (int i = 0; i < 65536; i++)
            if (SamusPoseTransitionTable.Lookup(forbidden, (byte)(i % 253), (ushort)i, (ushort)~i).Transition.HasValue) accepted++;
        long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        AssertTrue(accepted > 0, "Allocation probe exercises successful matches");
        AssertEqual(0L, allocated, "Warmed production lookup allocates no per-frame graph or records");
        Console.WriteLine($"Pose input graph: 253 mappings, 86 lists, 598 conditions and {comparisons} complete lookup comparisons match native with ROM reads forbidden; non-authored indexes fail loudly before parsing adjacent code.");
    }
}
