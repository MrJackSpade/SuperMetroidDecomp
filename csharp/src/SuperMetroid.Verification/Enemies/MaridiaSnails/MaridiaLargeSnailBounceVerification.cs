using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.MaridiaSnails;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Oum's bounce record steps, caps, goes below zero on a late impact and clamps on the rise as the cartridge's does.</summary>
internal static class MaridiaLargeSnailBounceVerification
{
    /// <summary>
    /// Runs the production main AI for one bounce update of a freshly initialized Oum. Falling in open air, the record
    /// grows by 384 and is capped at 16384. Falling onto a floor with a record below 4096, the impact leaves it below
    /// zero, which ends the bounces, and the slot variable holds that negative record as the cartridge's signed word.
    /// Rising, a record that would go below zero is stored as zero and the fall begins.
    /// </summary>
    internal static void VerifyMaridiaLargeSnailBounceRecord()
    {
        var air = new RoomLevelData(new Extent16<RoomBlocks>(8, 8), new RoomLevelPlanes(new ushort[64], new byte[64], new ushort[64]));
        var floorBlocks = new ushort[64];
        Array.Fill(floorBlocks, RoomLevelWord.Create(0, 0, RoomCollisionType.SolidBlock).Raw, 32, 32);
        var floor = new RoomLevelData(new Extent16<RoomBlocks>(8, 8), new RoomLevelPlanes(floorBlocks, new byte[64], new ushort[64]));

        foreach ((string context, RoomLevelData level, MaridiaLargeSnailBounceFunction phase, int start, int expected, MaridiaLargeSnailBounceFunction expectedPhase, bool expectedSettled) in
                 new VerifyMaridiaLargeSnailBounceRecordCase[]
                 {
                     new VerifyMaridiaLargeSnailBounceRecordCase("falling in the air past the cap", air, MaridiaLargeSnailBounceFunction.Falling, 16200, 16384, MaridiaLargeSnailBounceFunction.Falling, false),
                     new VerifyMaridiaLargeSnailBounceRecordCase("falling in the air below the cap", air, MaridiaLargeSnailBounceFunction.Falling, 1000, 1384, MaridiaLargeSnailBounceFunction.Falling, false),
                     new VerifyMaridiaLargeSnailBounceRecordCase("landing with a low record", floor, MaridiaLargeSnailBounceFunction.Falling, 1000, -2712, MaridiaLargeSnailBounceFunction.Rising, true),
                     new VerifyMaridiaLargeSnailBounceRecordCase("rising past zero", air, MaridiaLargeSnailBounceFunction.Rising, 200, 0, MaridiaLargeSnailBounceFunction.Falling, false),
                 })
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Oum, slot.Definition);
            MaridiaLargeSnailBehavior.InitializeMaridiaLargeSnail(slot);
            MaridiaLargeSnailEnemyState state = slot.RequireSpeciesState<MaridiaLargeSnailEnemyState>();
            slot.Radius = new(16, 16);
            // On the floor level the lowest pixel row already rests in the floor, so even a sub-pixel fall lands.
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(64, level == floor ? (ushort)49 : (ushort)48));
            state.BounceFunction = phase;
            state.YSpeedTableIndex = start;
            var samus = new SamusState();
            samus.Position = samus.Position.WithWhole(new(1000, 1000));
            var frame = new ClockedEnemyAiFrame(
                new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus, Level = level },
                new FrameCounters(0));

            enemies.MaridiaLargeSnailBehavior.RunMaridiaLargeSnailMainAi(slot, frame);

            AssertEqual(expected, state.YSpeedTableIndex, $"Oum {context}: record");
            AssertEqual(expectedPhase, state.BounceFunction, $"Oum {context}: bounce phase");
            AssertEqual(expectedSettled, state.RemainingBounces.IsZero, $"Oum {context}: bounces ended");
        }
        Console.WriteLine("  Oum bounce record: caps at 16384, keeps a negative record after a late impact and clamps to zero rising.");
    }

    /// <summary>Runs the Oum bounce-record check.</summary>
    [VerificationCommand("--maridia-large-snail-bounce-record")]
    internal static int MaridiaLargeSnailBounceRecordCommand()
    {
        Suite(nameof(VerifyMaridiaLargeSnailBounceRecord), () => VerifyMaridiaLargeSnailBounceRecord());
        return 0;
    }
}
