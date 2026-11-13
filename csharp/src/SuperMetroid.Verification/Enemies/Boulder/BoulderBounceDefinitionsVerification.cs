using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Boulder;

/// <summary>Checks compiled Boulder bounce speeds and their real floor-collision handoff.</summary>
internal static class BoulderBounceDefinitionsVerification
{
    /// <summary>Compares the three native words and verifies the zero-counter bounce enters rolling with Y position preserved.</summary>
    internal static void VerifyCompiledBoulderBounces(CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
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
            AssertEqual((int)expected, BoulderBounces.SpeedIndex(counter).InTwoHundredFiftySixthsOfRow, "Boulder native leading-zero/bounce word");
            actor.Position = actor.Position.WithY(new(55, 0));
            state.BounceCounter = counter;
            state.BounceSpeedAccumulator = EnemyQuadraticSpeedIndex.FromRows(32);
            state.Action = BoulderAction.Falling;
            fall(actor, state, level);
            AssertEqual((int)expected, state.BounceSpeedAccumulator.InTwoHundredFiftySixthsOfRow, "Boulder real collision installs compiled sample without bus");
            AssertEqual(counter - 1, state.BounceCounter, "Boulder bounce count after the impact, minus one after the last");
            AssertEqual(counter == 0 ? BoulderAction.Rolling : BoulderAction.Rebound,
                state.Action, "Boulder last collision enters rolling");
            if (counter == 0)
            {
                AssertEqual(actor.Position.Y, state.PreviousY, "Boulder rolling Y and subpixel handoff");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => BoulderBounces.SpeedIndex(3), "Boulder unsupported bounce count");
        AssertThrows<ArgumentOutOfRangeException>(() => BoulderBounces.SpeedIndex(ushort.MaxValue), "Boulder underflow is not another bounce");
        Console.WriteLine("Boulder bounce definitions: all three native words and real solid-floor collisions match without a bus, including the zero-to-minus-one rolling handoff.");
    }
}
