using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Bull.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Bull;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Bull's normal loop and its immune-shot reaction animation.</summary>
internal static class BullBehaviorVerification
{
    /// <summary>
    /// The initializer starts the normal loop (four frames, ten updates apiece). An immune shot starts the shot program,
    /// which plays its four three-update frames five times on the native repeat timer and then returns to the normal loop.
    /// </summary>
    internal static void VerifyBullBehavior()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Bull, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = BullFrameToolingDefinitions.Bank });
        enemies.BullBehavior.InitializeBull(slot);
        BullEnemyState state = enemies.BullStates[0] ?? throw new InvalidDataException("Bull initializer did not publish typed state.");
        AssertEqual(AnimationProgramCursor<BullProgram>.Start(BullProgram.Normal), state.Program, "Bull initializer animation");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BullAnimations.Programs, BullProgram.Normal, BullFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(BullAnimations.Programs, BullProgram.Normal)),
            "Bull normal loop");

        BullBehavior.ResolveBullImmuneShot(slot, state, 2);
        AssertEqual(AnimationProgramCursor<BullProgram>.Start(BullProgram.Shot), state.Program, "Bull immune-shot animation");
        ushort[] shot = BullAnimationsConstants.Spritemaps(BullProgram.ShotLoop).ToArray();
        ushort[] expected = [.. Enumerable.Repeat(shot, 5).SelectMany(frames => frames), BullAnimationsConstants.Spritemaps(BullProgram.Normal).First()];
        AssertEqual(EnemyInstructionRunner.ExpectedFrames(expected, Enumerable.Repeat(3, expected.Length)),
            EnemyInstructionRunner.ShownFrames(slot, 61), "Bull shot program repeats five times and returns to normal");
        AssertEqual((ushort)0, slot.Timer.RawValue, "Bull shot loop exhausts its native repeat timer");
        Console.WriteLine("  Bull behavior: the normal loop and the five-repeat immune-shot reaction run as the cartridge does.");
    }

    /// <summary>
    /// A deceleration that overshoots: speed 10 shrinks by the acceleration 24 to the wrapped word 65522, which the slot
    /// variable keeps; the next decelerating update reads that word as below zero and stops the flight, clearing speed,
    /// acceleration and its delta and returning to the movement delay. Both updates run the production main AI.
    /// </summary>
    internal static void VerifyBullDecelerationOvershoot()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Bull, slot.Definition);
        enemies.BullBehavior.InitializeBull(slot);
        BullEnemyState state = BullBehavior.RequireBullState(slot);
        state.Function = BullEnemyFunction.Decelerating;
        state.Speed = 10;
        state.Acceleration = 48;
        state.AccelerationDelta = 24;
        state.AccelerationIntervalTimer = NativeCountdown.Start(1);

        BullBehavior.RunBullMain(slot, state, samus: null);
        AssertEqual(24, state.Acceleration, "the expiring interval shrinks the acceleration by its delta");
        AssertEqual(65522, state.Speed, "the overshooting deceleration leaves the wrapped speed word");
        AssertEqual(BullEnemyFunction.Decelerating, state.Function, "the overshoot is read on the following update");

        BullBehavior.RunBullMain(slot, state, samus: null);
        AssertEqual(BullEnemyFunction.MovementDelay, state.Function, "a speed below zero stops the flight");
        AssertEqual(0, state.Speed, "stopping clears the speed");
        AssertEqual(0, state.Acceleration, "stopping clears the acceleration");
        AssertEqual(0, state.AccelerationDelta, "stopping clears the acceleration delta");
        Console.WriteLine("  Bull deceleration: an overshoot keeps the wrapped speed word and stops the flight on the next update.");
    }

    /// <summary>Runs the Bull behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--bull-instruction-mechanics")]
    internal static int BullInstructionMechanicsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBullBehavior), () => VerifyBullBehavior());
        Suite(nameof(VerifyBullDecelerationOvershoot), () => VerifyBullDecelerationOvershoot());
        Suite(nameof(BullAnimationsVerification.VerifyBullAnimations), () => BullAnimationsVerification.VerifyBullAnimations(rom));
        return 0;
    }
}
