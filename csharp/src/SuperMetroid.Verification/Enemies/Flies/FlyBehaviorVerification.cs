using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.Core.Enemies.Flies.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Flies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that every fly-family definition starts and runs the shared flight loop.</summary>
internal static class FlyBehaviorVerification
{
    /// <summary>
    /// Each of Mellow, Mella and Memu starts the flight loop, shows its four frames two updates apiece and returns to
    /// the first through the loop's goto.
    /// </summary>
    internal static void VerifyFlyBehavior(SuperMetroidAddressSpace rom)
    {
        ushort[] loop = FlyAnimationsConstants.Spritemaps(FlyProgram.Flight).ToArray();
        string expected = EnemyInstructionRunner.ExpectedFrames([.. loop, loop[0]], [2, 2, 2, 2, 2]);
        foreach (EnemyDefinitionId definition in new[] { EnemyDefinitionId.Mellow, EnemyDefinitionId.Mella, EnemyDefinitionId.Menu })
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(rom);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(definition, slot.Definition);
            slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = FlyFrameToolingDefinitions.Bank });
            FliesBehavior.InitializeFly(slot);
            AssertEqual(AnimationProgramCursor<FlyProgram>.Start(FlyProgram.Flight),
                ((enemies.SlotTable.Slots[0].Entity as FliesEntity)?.State ?? throw new InvalidOperationException("The fly has no state.")).Program,
                $"fly ${(int)definition:X4} initializer animation");
            AssertEqual(expected, EnemyInstructionRunner.ShownFrames(slot, 9),
                $"fly ${(int)definition:X4} runs the flight loop");
        }
        Console.WriteLine("  Fly behavior: all three initializers start and run the shared flight loop.");
    }

    /// <summary>Runs the fly-family behavior checks and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--fly-instruction-mechanics")]
    internal static int FlyInstructionMechanicsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyFlyBehavior), () => VerifyFlyBehavior(rom));
        Suite(nameof(FlyAnimationsVerification.VerifyFlyAnimations), () => FlyAnimationsVerification.VerifyFlyAnimations(rom));
        return 0;
    }
}
