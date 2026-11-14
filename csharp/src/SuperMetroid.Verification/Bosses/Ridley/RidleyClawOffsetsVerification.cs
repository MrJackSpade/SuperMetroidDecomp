using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Compares Ridley's claw geometry, carried Samus placement, and
/// collision bounds with native offset words across wrapped coordinates.</summary>
internal static class RidleyClawOffsetsVerification
{
    /// <summary>Checks authored facing and foot offsets, carry decay and word
    /// wrapping, and claw overlap through production behavior.</summary>
    internal static void VerifyRidleyClawOffsets(CartridgeImportAddressSpace rom)
    {
        short Word(int address) => unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
        short[] x = Enumerable.Range(0, 3).Select(i => Word(EnemyRomTablePointers.Ridley.ClawXOffsetWords + i * 2)).ToArray();
        short[] y = Enumerable.Range(0, 3).Select(i => Word(EnemyRomTablePointers.Ridley.ClawYOffsetWords + i * 2)).ToArray();
        foreach (RidleyFacing facing in Enum.GetValues<RidleyFacing>())
            AssertEqual(x[(int)facing], RidleyClawOffsets.ReadX(facing), "All authored claw X words");
        foreach (RidleyFacing undefined in new[] { (RidleyFacing)3, (RidleyFacing)ushort.MaxValue })
            AssertThrows<InvalidOperationException>(() => RidleyClawOffsets.ReadX(undefined), "Undefined Ridley facing has no claw X word");
        foreach (RidleyClawHeight height in RidleyClawHeightPlaces.TableOrder)
            AssertEqual(y[RidleyClawHeightPlaces.Place(height)], RidleyClawOffsets.ReadY(height), "Each claw height reads its authored claw Y word");
        AssertEqual(Enum.GetValues<RidleyClawHeight>().Length, RidleyClawHeightPlaces.TableOrder.Length, "Ridley has three claw heights");
        foreach (RidleyClawHeight undefined in new[] { (RidleyClawHeight)(-1), (RidleyClawHeight)3, (RidleyClawHeight)ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => RidleyClawOffsets.ReadY(undefined), "Ridley has only three claw heights");
        // The body segments' cues select exactly the authored poses, so no gameplay path reaches past the table.
        foreach (RidleyBodySegment program in Enum.GetValues<RidleyBodySegment>())
        foreach (var beat in RidleyAnimations.Segments.Beats(program))
        {
            if (beat is not AnimationBeat<RidleyCollisionDefinitions.BodyFrame, RidleyBodyCue>.Event { Raised: RidleyBodyCue.SetClawHeight feet })
                continue;
            AssertTrue(Enum.IsDefined(feet.Height),
                $"Ridley {program} selects claw height {feet.Height}");
        }
        Action<RoomEnemySlot, RidleyEnemyState, SamusState> carry = NorfairRidleyCombat.UpdateNorfairRidleyGrabbedSamus;
        Func<RoomEnemySlot, RidleyEnemyState, SamusState, Extent16<RoomPixels>, bool> overlaps = NorfairRidleyCombat.RidleyClawOverlapsSamus;
        var slot = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom()).SlotTable.Slots[0];
        var state = new RidleyEnemyState();
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        static int Decay(short offset) => offset < 0 ? Math.Min(0, offset + 4) : Math.Max(0, offset - 4);
        foreach (RidleyFacing facing in Enum.GetValues<RidleyFacing>())
        foreach (RidleyClawHeight feet in RidleyClawHeightPlaces.TableOrder)
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            slot.Position = slot.Position.WithWhole(new((ushort)raw, (ushort)(ushort.MaxValue - raw)));
            state.FacingDirection = facing;
            state.ClawHeight = feet;
            state.GrabOffset = new Offset16<RoomPixels>(unchecked((short)raw), unchecked((short)(ushort.MaxValue - raw)));
            int dx = Decay(unchecked(state.GrabOffset.X));
            int dy = Decay(unchecked(state.GrabOffset.Y));
            carry(slot, state, samus);
            AssertEqual(unchecked((ushort)(raw + x[(int)facing] + dx)), samus.Position.X.Whole, "Actual carry X includes claw, signed decay and word wrap");
            AssertEqual(unchecked((ushort)(ushort.MaxValue - raw + y[RidleyClawHeightPlaces.Place(feet)] + dy)), samus.Position.Y.Whole, "Actual carry Y includes animated claw, signed decay and word wrap");
            AssertEqual(unchecked((short)dx), state.GrabOffset.X, "Carry X offset decays four pixels");
            AssertEqual(unchecked((short)dy), state.GrabOffset.Y, "Carry Y offset decays four pixels");
        }
        foreach (ushort world in new ushort[] { 0, 128, 65535 })
        foreach (RidleyFacing facing in Enum.GetValues<RidleyFacing>())
        foreach (RidleyClawHeight feet in RidleyClawHeightPlaces.TableOrder)
        for (int dx = -32; dx <= 32; dx++)
        for (int dy = -32; dy <= 32; dy++)
        {
            slot.Position = slot.Position.WithWhole(new(world, world));
            state.FacingDirection = facing;
            state.ClawHeight = feet;
            samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(world + x[(int)facing] + dx)), unchecked((ushort)(world + y[RidleyClawHeightPlaces.Place(feet)] + dy))));
            bool expected = Math.Abs(dx) < samus.Kinematics.Radius.X + 8 && Math.Abs(dy) < samus.Kinematics.Radius.Y + 12;
            AssertEqual(expected, overlaps(slot, state, samus, new Extent16<RoomPixels>(8, 12)), "Actual claw collision boundaries including world wrap");
        }
        Console.WriteLine("Ridley claws: three authored claw heights, out-of-range rejection, 589824 carry placements and 114075 collision probes pass through bus-free consumers.");
    }
}
