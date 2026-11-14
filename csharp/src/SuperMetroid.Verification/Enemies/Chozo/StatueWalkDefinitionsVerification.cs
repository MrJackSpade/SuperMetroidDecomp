using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>Checks compiled Golden Torizo walking and Chozo statue carry motion against native tables.</summary>
internal static class StatueWalkDefinitionsVerification
{
    /// <summary>Compares Torizo walking steps and Chozo carry words, then checks production movement and Samus offsets without a bus.</summary>
    internal static void VerifyCompiledStatueWalking(CartridgeImportAddressSpace rom)
    {
        // The cartridge's walking table holds one signed word per step; every walking cue reads a whole entry.
        for (int step = 0; step < GoldenTorizoWalk.StepCount; step++)
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(0xaad59a + step * 2)), GoldenTorizoWalk.Velocity(step), "Torizo walking step velocity");
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        Func<RoomEnemySlot, TorizoEnemyState, SamusState?, RoomLevelData, int, AnimationContinuation<TorizoAnimation>?> walk = enemies.BombTorizoBehavior.Choices.ProcessGoldenTorizoWalkAnimation;
        Action<RoomEnemySlot, SamusState, RoomLevelData, ChozoCarryStep> carry = enemies.ChozoStatueBehavior.ProcessChozoStatueMovement;
        var level = CreateEmptyRoom(new(32, 32));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Radius = new(8, 8);
        var torizo = new TorizoEnemyState(slot, true);
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        for (int step = 0; step < GoldenTorizoWalk.StepCount; step++)
        {
            int offset = step * 2;
            slot.Position = slot.Position.WithWholeY(256).WithWholeX(256).WithFractionX(123);
            AnimationContinuation<TorizoAnimation>? next = walk(slot, torizo, null, level, step);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaad59a + offset)), torizo.Velocity.X.RawValue, "Torizo actual walking velocity without bus");
            AssertEqual(unchecked((ushort)(256 + (short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaad59a + offset)))), slot.Position.X.Whole, "Torizo real whole-pixel movement");
            AssertEqual((ushort)123, slot.Position.X.Fraction, "Torizo preserves subpixel state");
            AssertTrue(next is null, "Torizo walking step runs on without a wall");
        }
        // Every cartridge record that holds a carry step: its facing and pose read the record's words. The four still
        // records closing each facing's sixteen are named by no cue and hold no step.
        int carrySteps = 0;
        foreach (int record in ChozoNativeWords.StepRecords)
        {
            int offset = record * 2;
            ChozoCarryStep step = ChozoNativeWords.CarryStep(record);
            var data = ChozoCarry.Read(step);
            carrySteps++;
            short velocity = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaae630 + offset)));
            short xOffset = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaae670 + offset)));
            short yOffset = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaae6b0 + offset)));
            AssertEqual(velocity, data.Velocity, "Chozo native movement velocity");
            AssertEqual(xOffset, data.SamusOffset.X, "Chozo native hand X");
            AssertEqual(yOffset, data.SamusOffset.Y, "Chozo native hand Y");
            slot.Position = new(new(256, 0x8123), new(256, 0x8123));
            uint start = 0x01008123;
            carry(slot, samus, level, step);
            uint x = unchecked(start + (uint)(SixteenSixteen.FromEightEight(velocity)));
            uint y = start + (uint)(SixteenSixteen.FromEightEight(Math.Abs((int)velocity)));
            AssertEqual(FixedCoordinate.WholeOf(unchecked((int)x)), slot.Position.X.Whole, "Chozo real signed X movement");
            AssertEqual((ushort)x, slot.Position.X.Fraction, "Chozo real fractional X");
            AssertEqual(FixedCoordinate.WholeOf(unchecked((int)y)), slot.Position.Y.Whole, "Chozo real absolute Y movement");
            AssertEqual((ushort)y, slot.Position.Y.Fraction, "Chozo real fractional Y");
            AssertEqual(unchecked((ushort)(slot.Position.X.Whole + xOffset)), samus.Position.X.Whole, "Chozo actual carried Samus X");
            AssertEqual(unchecked((ushort)(slot.Position.Y.Whole + yOffset)), samus.Position.Y.Whole, "Chozo actual carried Samus Y");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => GoldenTorizoWalk.Velocity(GoldenTorizoWalk.StepCount), "Torizo step past the walk");
        AssertThrows<ArgumentOutOfRangeException>(() => GoldenTorizoWalk.Velocity(-1), "Torizo negative walking step");
        AssertEqual(24, carrySteps, "both facings' four holding poses and twice-walked four-step strides");
        Console.WriteLine($"Statue walking: 20 Torizo walking steps, {carrySteps * 3} Chozo words and {GoldenTorizoWalk.StepCount + carrySteps} real movement/carry calls match without a bus.");
    }
}
