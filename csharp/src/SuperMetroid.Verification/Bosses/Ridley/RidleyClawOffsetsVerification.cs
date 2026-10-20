using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Compares Ridley's claw geometry, carried Samus placement, and
/// collision bounds with native offset words across wrapped coordinates.</summary>
internal static class RidleyClawOffsetsVerification
{
    /// <summary>Checks authored facing and foot offsets, carry decay and word
    /// wrapping, and claw overlap through production behavior.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ridley claw offsets")]
    internal static void VerifyRidleyClawOffsets(SuperMetroidAddressSpace rom)
    {
        short Word(int address) => unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        short[] x = Enumerable.Range(0, 3).Select(i => Word(EnemyRomTablePointers.Ridley.ClawXOffsetWords + i * 2)).ToArray();
        short[] y = Enumerable.Range(0, 3).Select(i => Word(EnemyRomTablePointers.Ridley.ClawYOffsetWords + i * 2)).ToArray();
        foreach (RidleyFacing facing in Enum.GetValues<RidleyFacing>())
            AssertEqual(x[(int)facing], RidleyClawOffsets.ReadX(facing), "All authored claw X words");
        foreach (RidleyFacing undefined in new[] { (RidleyFacing)3, (RidleyFacing)ushort.MaxValue })
            AssertThrows<InvalidOperationException>(() => RidleyClawOffsets.ReadX(undefined), "Undefined Ridley facing has no claw X word");
        for (int pose = 0; pose < 3; pose++)
            AssertEqual(y[pose], RidleyClawOffsets.ReadY(pose), "Each claw-height pose reads its authored claw Y word");
        foreach (int undefined in new[] { -1, 3, 4, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyClawOffsets.ReadY(undefined), "Ridley has only three claw-height poses");
        // The body programs' cues select exactly the authored poses, so no gameplay path reaches past the table.
        foreach (RidleyBodyProgram program in Enum.GetValues<RidleyBodyProgram>())
        foreach (var step in RidleyAnimations.Programs.Steps(program))
        {
            if (step is not AnimationProgramStep<RidleyBodyProgram, RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>.CueStep { Cue: RidleyBodyCue.SetFeetDistance feet })
                continue;
            AssertTrue((uint)feet.FeetDistanceIndex < 3,
                $"Ridley {program} selects claw-height pose {feet.FeetDistanceIndex}");
        }
        Action<RoomEnemySlot, RidleyEnemyState, SamusState> carry = NorfairRidleyCombat.UpdateNorfairRidleyGrabbedSamus;
        Func<RoomEnemySlot, RidleyEnemyState, SamusState, Extent16<RoomPixels>, bool> overlaps = NorfairRidleyCombat.RidleyClawOverlapsSamus;
        var slot = new RoomEnemySystem(new EnemySession()).SlotTable.Slots[0];
        var state = new RidleyEnemyState();
        var samus = new SamusState();
        static int Decay(short offset) => offset < 0 ? Math.Min(0, offset + 4) : Math.Max(0, offset - 4);
        foreach (RidleyFacing facing in Enum.GetValues<RidleyFacing>())
        for (int feet = 0; feet < 3; feet++)
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            slot.Position = slot.Position.WithWhole(new((ushort)raw, (ushort)(ushort.MaxValue - raw)));
            state.FacingDirection = facing;
            state.FeetDistanceIndex = feet;
            state.GrabOffset = new Offset16<RoomPixels>(unchecked((short)raw), unchecked((short)(ushort.MaxValue - raw)));
            int dx = Decay(unchecked(state.GrabOffset.X));
            int dy = Decay(unchecked(state.GrabOffset.Y));
            carry(slot, state, samus);
            AssertEqual(unchecked((ushort)(raw + x[(int)facing] + dx)), samus.Position.X.Whole, "Actual carry X includes claw, signed decay and word wrap");
            AssertEqual(unchecked((ushort)(ushort.MaxValue - raw + y[feet] + dy)), samus.Position.Y.Whole, "Actual carry Y includes animated claw, signed decay and word wrap");
            AssertEqual(unchecked((short)dx), state.GrabOffset.X, "Carry X offset decays four pixels");
            AssertEqual(unchecked((short)dy), state.GrabOffset.Y, "Carry Y offset decays four pixels");
        }
        foreach (ushort world in new ushort[] { 0, 128, 65535 })
        foreach (RidleyFacing facing in Enum.GetValues<RidleyFacing>())
        for (int feet = 0; feet < 3; feet++)
        for (int dx = -32; dx <= 32; dx++)
        for (int dy = -32; dy <= 32; dy++)
        {
            slot.Position = slot.Position.WithWhole(new(world, world));
            state.FacingDirection = facing;
            state.FeetDistanceIndex = feet;
            samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(world + x[(int)facing] + dx)), unchecked((ushort)(world + y[feet] + dy))));
            bool expected = Math.Abs(dx) < samus.Kinematics.Radius.X + 8 && Math.Abs(dy) < samus.Kinematics.Radius.Y + 12;
            AssertEqual(expected, overlaps(slot, state, samus, new Extent16<RoomPixels>(8, 12)), "Actual claw collision boundaries including world wrap");
        }
        Console.WriteLine("Ridley claws: three authored claw-height poses, out-of-range rejection, 589824 carry placements and 114075 collision probes pass through bus-free consumers.");
    }
}
