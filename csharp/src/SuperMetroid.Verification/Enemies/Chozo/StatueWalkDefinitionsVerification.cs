using SuperMetroid.Core.Audio;
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
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>Checks Golden Torizo's walking movement and Chozo statue carry motion against native tables.</summary>
internal static class StatueWalkDefinitionsVerification
{
    /// <summary>Golden Torizo's walking velocity table: one signed word per step, which each walking cue carries (#1970).</summary>
    private const int GoldenWalkTable = 0xaad59a;

    /// <summary>The words of Golden Torizo's walking velocity table.</summary>
    private const int GoldenWalkSteps = 20;

    /// <summary>Moves Golden Torizo by each native walking step and compares Chozo carry words, checking production movement and Samus offsets without a bus.</summary>
    internal static void VerifyCompiledStatueWalking(CartridgeImportAddressSpace rom)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        Func<RoomEnemySlot, TorizoEnemyState, SamusState?, RoomLevelData, int, AnimationContinuation<TorizoAnimation>?> walk = enemies.TorizoBehavior.Choices.ProcessGoldenTorizoWalkAnimation;
        Action<RoomEnemySlot, SamusState, RoomLevelData, ChozoCarryStep> carry = enemies.ChozoStatueBehavior.ProcessChozoStatueMovement;
        var level = CreateEmptyRoom(new(32, 32));
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Radius = new(8, 8);
        var torizo = new TorizoEnemyState(true);
        slot.SetSpeciesState(torizo);
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), new SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding());
        for (int step = 0; step < GoldenWalkSteps; step++)
        {
            int offset = step * 2;
            short velocity = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(GoldenWalkTable + offset));
            slot.Position = slot.Position.WithWholeY(256).WithWholeX(256).WithFractionX(123);
            AnimationContinuation<TorizoAnimation>? next = walk(slot, torizo, null, level, velocity);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(GoldenWalkTable + offset), torizo.Velocity.X.RawValue, "Torizo actual walking velocity without bus");
            AssertEqual(unchecked((ushort)(256 + velocity)), slot.Position.X.Whole, "Torizo real whole-pixel movement");
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
            AssertEqual((ushort)SixteenSixteenWords.WholeOf(x), slot.Position.X.Whole, "Chozo real signed X movement");
            AssertEqual((ushort)x, slot.Position.X.Fraction, "Chozo real fractional X");
            AssertEqual((ushort)SixteenSixteenWords.WholeOf(y), slot.Position.Y.Whole, "Chozo real absolute Y movement");
            AssertEqual((ushort)y, slot.Position.Y.Fraction, "Chozo real fractional Y");
            AssertEqual(unchecked((ushort)(slot.Position.X.Whole + xOffset)), samus.Position.X.Whole, "Chozo actual carried Samus X");
            AssertEqual(unchecked((ushort)(slot.Position.Y.Whole + yOffset)), samus.Position.Y.Whole, "Chozo actual carried Samus Y");
        }
        AssertEqual(24, carrySteps, "both facings' four holding poses and twice-walked four-step strides");
        Console.WriteLine($"Statue walking: 20 Torizo walking steps, {carrySteps * 3} Chozo words and {GoldenWalkSteps + carrySteps} real movement/carry calls match without a bus.");
    }
}
