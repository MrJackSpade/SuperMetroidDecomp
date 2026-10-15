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
    internal static void VerifyBullBehavior(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Bull;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = BullFrameToolingDefinitions.Bank };
        enemies.BullBehavior.InitializeBull(slot);
        BullEnemyState state = enemies.BullStates[0] ?? throw new InvalidDataException("Bull initializer did not publish typed state.");
        AssertEqual(AnimationProgramCursor<BullProgram>.Start(BullProgram.Normal), state.Program, "Bull initializer animation");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BullAnimations.Programs, BullProgram.Normal, BullFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(enemies, slot, EnemyInstructionRunner.LoopUpdates(BullAnimations.Programs, BullProgram.Normal)),
            "Bull normal loop");

        BullBehavior.ResolveBullImmuneShot(slot, state, 2);
        AssertEqual(AnimationProgramCursor<BullProgram>.Start(BullProgram.Shot), state.Program, "Bull immune-shot animation");
        ushort[] shot = BullAnimationsConstants.Spritemaps(BullProgram.ShotLoop).ToArray();
        ushort[] expected = [.. Enumerable.Repeat(shot, 5).SelectMany(frames => frames), BullAnimationsConstants.Spritemaps(BullProgram.Normal).First()];
        AssertEqual(EnemyInstructionRunner.ExpectedFrames(expected, Enumerable.Repeat(3, expected.Length)),
            EnemyInstructionRunner.ShownFrames(enemies, slot, 61), "Bull shot program repeats five times and returns to normal");
        AssertEqual((ushort)0, slot.Timer.RawValue, "Bull shot loop exhausts its native repeat timer");
        Console.WriteLine("  Bull behavior: the normal loop and the five-repeat immune-shot reaction run as the cartridge does.");
    }

    /// <summary>Runs the Bull behavior check and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--bull-instruction-mechanics")]
    internal static int BullInstructionMechanicsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBullBehavior), () => VerifyBullBehavior(rom));
        Suite(nameof(BullAnimationsVerification.VerifyBullAnimations), () => BullAnimationsVerification.VerifyBullAnimations(rom));
        return 0;
    }
}
