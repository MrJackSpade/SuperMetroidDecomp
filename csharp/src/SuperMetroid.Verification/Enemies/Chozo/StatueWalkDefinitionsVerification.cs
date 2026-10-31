using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Programs;
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
    internal static void VerifyCompiledStatueWalking(SuperMetroidAddressSpace rom)
    {
        // The cartridge's walking table holds one signed word per step; every walking cue reads a whole entry.
        for (int step = 0; step < GoldenTorizoWalk.StepCount; step++)
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(0xaad59a + step * 2)), GoldenTorizoWalk.Velocity(step), "Torizo walking step velocity");
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()));
        Func<RoomEnemySlot, TorizoEnemyState, SamusState?, RoomLevelData, int, AnimationProgramCursor<TorizoBodyProgram>?> walk = enemies.BombTorizoBehavior.Choices.ProcessGoldenTorizoWalkInstruction;
        Action<RoomEnemySlot, ChozoStatueState, SamusState, RoomLevelData, int> carry = enemies.ChozoStatueBehavior.ProcessChozoStatueMovement;
        var level = CreateEmptyRoom(new(32, 32));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Radius = new(8, 8);
        var torizo = new TorizoEnemyState(slot, true);
        var statue = new ChozoStatueState();
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence());
        for (int step = 0; step < GoldenTorizoWalk.StepCount; step++)
        {
            int offset = step * 2;
            slot.Position = slot.Position.WithWholeY(256).WithWholeX(256).WithFractionX(123);
            AnimationProgramCursor<TorizoBodyProgram>? next = walk(slot, torizo, null, level, step);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaad59a + offset)), torizo.Velocity.X.RawValue, "Torizo actual walking velocity without bus");
            AssertEqual(unchecked((ushort)(256 + (short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaad59a + offset)))), slot.Position.X.Whole, "Torizo real whole-pixel movement");
            AssertEqual((ushort)123, slot.Position.X.Fraction, "Torizo preserves subpixel state");
            AssertTrue(next is null, "Torizo walking step runs on without a wall");
        }
        for (int record = 0; record < ChozoCarryMotionDefinitions.RecordCount; record++)
        {
            int offset = record * 2;
            var data = ChozoCarry.Read(record);
            short velocity = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaae630 + offset)));
            short xOffset = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaae670 + offset)));
            short yOffset = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaae6b0 + offset)));
            AssertEqual(velocity, data.Velocity, "Chozo native movement velocity");
            AssertEqual(xOffset, data.SamusOffset.X, "Chozo native hand X");
            AssertEqual(yOffset, data.SamusOffset.Y, "Chozo native hand Y");
            slot.Position = new(new(256, 0x8123), new(256, 0x8123));
            uint start = 0x01008123;
            carry(slot, statue, samus, level, record);
            AssertEqual(record, statue.CarryRecord, "Chozo carry record");
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
        AssertThrows<InvalidDataException>(() => ChozoCarry.Read(ChozoCarryMotionDefinitions.RecordCount), "Chozo record past the table remains invalid");
        AssertThrows<InvalidDataException>(() => ChozoCarry.Read(-1), "Chozo negative record remains invalid");
        Console.WriteLine("Statue walking: 20 Torizo walking steps, 96 Chozo words and 52 real movement/carry calls match without a bus.");
    }
}
