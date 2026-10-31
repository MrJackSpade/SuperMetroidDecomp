using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Boulder;

/// <summary>Checks compiled Boulder bounce speeds and their real floor-collision handoff.</summary>
internal static class BoulderBounceDefinitionsVerification
{
    /// <summary>Compares the three native words and verifies the zero-counter bounce enters rolling with Y position preserved.</summary>
    internal static void VerifyCompiledBoulderBounces(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        Action<RoomEnemySlot, BoulderEnemyState, RoomLevelData> fall = enemies.BoulderBehavior.RunBoulderFalling;
        RoomLevelData level = VerificationHelpersVerification.CreateSolidRowsRoom(new(8, 8), firstSolidRow: 4, solidRowCount: 4);
        RoomEnemySlot actor = enemies.SlotTable.Slots[0];
        var state = new BoulderEnemyState();
        actor.Position = actor.Position.WithWholeX(40);
        actor.Radius = new(8, 8);
        for (ushort counter = 0; counter <= 2; counter++)
        {
            int address = 0xa686f1 + (counter - 1) * 2;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, BoulderBounces.SpeedIndex(counter), "Boulder native leading-zero/bounce word");
            actor.Position = actor.Position.WithY(new(55, 0));
            state.BounceCounter = WrappingCountdown.FromRaw(counter);
            state.BounceSpeedAccumulator = 0x2000;
            state.Function = BoulderAiFunction.Falling;
            fall(actor, state, level);
            AssertEqual(expected, state.BounceSpeedAccumulator, "Boulder real collision installs compiled sample without bus");
            AssertEqual(unchecked((ushort)(counter - 1)), state.BounceCounter.RawValue, "Boulder native bounce underflow");
            AssertEqual(counter == 0 ? BoulderAiFunction.Rolling : BoulderAiFunction.Rebound,
                state.Function, "Boulder last collision enters rolling");
            if (counter == 0)
            {
                AssertEqual(actor.Position.Y, state.PreviousY, "Boulder rolling Y and subpixel handoff");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => BoulderBounces.SpeedIndex(3), "Boulder unsupported bounce count");
        AssertThrows<ArgumentOutOfRangeException>(() => BoulderBounces.SpeedIndex(ushort.MaxValue), "Boulder underflow is not another bounce");
        Console.WriteLine("Boulder bounce definitions: all three native words and real solid-floor collisions match without a bus, including zero-to-FFFF rolling handoff.");
    }
}
