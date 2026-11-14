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
        for (int impactsSoFar = 0; impactsSoFar < BoulderBounces.ImpactCount; impactsSoFar++)
        {
            // The cartridge's table is indexed by the bounces left, two for the first impact and zero for the last.
            int bouncesLeft = BoulderBounces.ImpactCount - 1 - impactsSoFar;
            int address = 0xa686f1 + (bouncesLeft - 1) * 2;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual((int)expected, BoulderBounces.RestartPlace(impactsSoFar).InTwoHundredFiftySixthsOfMark, "Boulder native leading-zero/bounce word");
            actor.Position = actor.Position.WithY(new(55, 0));
            state.ImpactsSoFar = impactsSoFar;
            state.BounceSpeedAccumulator = QuadraticSpeedCurvePlace.FromMarks(32);
            state.Action = BoulderAction.Falling;
            fall(actor, state, level);
            AssertEqual((int)expected, state.BounceSpeedAccumulator.InTwoHundredFiftySixthsOfMark, "Boulder real collision installs compiled sample without bus");
            AssertEqual(impactsSoFar + 1, state.ImpactsSoFar, "Boulder counts the impact");
            bool last = impactsSoFar == BoulderBounces.ImpactCount - 1;
            AssertEqual(last ? BoulderAction.Rolling : BoulderAction.Rebound,
                state.Action, "Boulder last collision enters rolling");
            if (last)
            {
                AssertEqual(actor.Position.Y, state.PreviousY, "Boulder rolling Y and subpixel handoff");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => BoulderBounces.RestartPlace(BoulderBounces.ImpactCount), "Boulder has no impact after rolling");
        AssertThrows<ArgumentOutOfRangeException>(() => BoulderBounces.RestartPlace(-1), "Boulder has no impact before the first");
        Console.WriteLine("Boulder bounce definitions: all three native words and real solid-floor collisions match without a bus, including the zero-to-minus-one rolling handoff.");
    }
}
