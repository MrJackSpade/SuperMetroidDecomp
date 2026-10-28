using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Hoppers;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>The hop arc's quadratic-speed record steps, clamps at the apex and caps on the way back as the cartridge's do.</summary>
internal static class HopperHopArcVerification
{
    /// <summary>
    /// Runs the production main AI for one airborne update of a floor Hopper in open air. Moving away from the surface,
    /// the record retreats by its step; a result below zero is stored as zero and turns the hop back toward the surface,
    /// while a result of exactly zero keeps rising. Returning, the record grows by its step up to 64. The record's slot
    /// variable holds the same value.
    /// </summary>
    internal static void VerifyHopperHopArcRecord()
    {
        var level = new RoomLevelData(new Extent16<RoomBlocks>(8, 8), new RoomLevelPlanes(new ushort[64], new byte[64], new ushort[64]), new BackgroundTile[4]);
        var frame = new ClockedEnemyAiFrame(
            new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState(), Level = level },
            new FrameCounters(0));
        foreach ((bool falling, int start, int step, int expected, bool expectedFalling) in new VerifyHopperHopArcRecordCase[]
                 {
                     new VerifyHopperHopArcRecordCase(false, 2, 3, 0, true),
                     new VerifyHopperHopArcRecordCase(false, 3, 3, 0, false),
                     new VerifyHopperHopArcRecordCase(false, 10, 4, 6, false),
                     new VerifyHopperHopArcRecordCase(true, 59, 4, 63, true),
                     new VerifyHopperHopArcRecordCase(true, 62, 4, 64, true),
                 })
        {
            var enemies = new RoomEnemySystem(new EnemySession());
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Sidehopper, slot.Definition);
            slot.Position = slot.Position.WithWhole(new(64, 48));
            slot.Radius = new(8, 8);
            var state = new HopperEnemyState
            {
                Function = HopperEnemyFunction.JumpingUpsideUpForward,
                Falling = falling,
                XVelocity = 0,
                YSpeedTableIndex = start,
                YSpeedTableIndexDelta = step,
            };
            slot.SetSpeciesState(state, state.ClearWithRecord);

            enemies.HoppersBehavior.RunHoppersMainAi(slot, frame);

            string context = $"Hopper {(falling ? "returning" : "rising")} from record {start} by {step}";
            AssertEqual(expected, state.YSpeedTableIndex, context);
            AssertEqual(expectedFalling, state.Falling, $"{context}: direction");
        }
        Console.WriteLine("  Hopper hop arc: the record clamps to zero past the apex and caps at 64 on the way back.");
    }

    /// <summary>Runs the Hopper hop-arc record check.</summary>
    [VerificationCommand("--hopper-hop-arc")]
    internal static int HopperHopArcCommand()
    {
        Suite(nameof(VerifyHopperHopArcRecord), () => VerifyHopperHopArcRecord());
        return 0;
    }
}
