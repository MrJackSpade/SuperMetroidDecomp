using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Rooms.LevelData;

/// <summary>Checks compiled Boulder bounce speeds and their real floor-collision handoff.</summary>
internal static class BoulderBounceDefinitionsVerification
{
    /// <summary>Compares the three native words and verifies the zero-counter bounce enters rolling with Y position preserved.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled Boulder bounces")]
    internal static void VerifyCompiledBoulderBounces(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        Action<RoomEnemySlot, BoulderEnemyState, RoomLevelData> fall = enemies.BoulderBehavior.RunBoulderFalling;
        var blocks = new ushort[64];
        Array.Fill(blocks, (ushort)0x8000, 32, 32);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(8, 8), new RoomLevelPlanes(blocks, new byte[64], new ushort[64]), new byte[8]);
        RoomEnemySlot actor = enemies.SlotTable.Slots[0];
        var state = new BoulderEnemyState(actor);
        actor.Position = actor.Position.WithWholeX(40);
        actor.Radius = new(8, 8);
        for (ushort counter = 0; counter <= 2; counter++)
        {
            int address = 0xa686f1 + (counter - 1) * 2;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, BoulderBounceDefinitions.SpeedIndex(counter), "Boulder native leading-zero/bounce word");
            actor.Position = actor.Position.WithY(new(55, 0));
            state.BounceCounter = NativeCountdown.FromRaw(counter);
            state.BounceSpeedAccumulator = 0x2000;
            state.Function = BoulderAiFunction.Falling;
            fall(actor, state, level);
            AssertEqual(expected, state.BounceSpeedAccumulator, "Boulder real collision installs compiled sample without bus");
            AssertEqual(unchecked((ushort)(counter - 1)), state.BounceCounter.RawValue, "Boulder native bounce underflow");
            AssertEqual(counter == 0 ? BoulderAiFunction.Rolling : BoulderAiFunction.Rebound,
                state.Function, "Boulder last collision enters rolling");
            if (counter == 0)
            {
                AssertEqual(actor.Position.Y.Whole, state.PreviousYPosition, "Boulder rolling Y handoff");
                AssertEqual(actor.Position.Y.Fraction, state.PreviousYSubposition, "Boulder rolling subpixel handoff");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => BoulderBounceDefinitions.SpeedIndex(3), "Boulder unsupported bounce count");
        AssertThrows<ArgumentOutOfRangeException>(() => BoulderBounceDefinitions.SpeedIndex(ushort.MaxValue), "Boulder underflow is not another bounce");
        Console.WriteLine("Boulder bounce definitions: all three native words and real solid-floor collisions match without a bus, including zero-to-FFFF rolling handoff.");
    }
}
