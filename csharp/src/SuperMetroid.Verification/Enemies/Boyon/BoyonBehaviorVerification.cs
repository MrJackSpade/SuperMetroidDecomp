using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyVisualSelectorInventoryVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Boyon.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Boyon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Boyon's idle and bouncing loops and the cues each hands to main AI.</summary>
internal static class BoyonBehaviorVerification
{
    /// <summary>
    /// The real initializer starts the idle loop, which stops off-screen processing and loops on time; Samus close by
    /// starts the bounce, which processes off screen, plays the bounce sound, permits the bounce arc and loops on time.
    /// </summary>
    internal static void VerifyBoyonBehavior(SuperMetroidAddressSpace rom)
    {
        (RoomEnemySystem idleSystem, RoomEnemySlot idle, BoyonEnemyState idleState) = Create(rom);
        AssertEqual(AnimationProgramCursor<BoyonProgram>.Start(BoyonProgram.Idle), idleState.Program, "Boyon initializer starts idling");
        idle.Properties = idle.Properties.With(EnemyProperties.ProcessOffScreen);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BoyonAnimations.Programs, BoyonProgram.IdleLoop, BoyonFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(idle, EnemyInstructionRunner.LoopUpdates(BoyonAnimations.Programs, BoyonProgram.IdleLoop)), "Boyon idle loop");
        AssertTrue(!idle.Properties.HasAny(EnemyProperties.ProcessOffScreen), "Boyon idle stops off-screen processing");

        (RoomEnemySystem bounceSystem, RoomEnemySlot bounce, BoyonEnemyState bounceState) = Create(rom);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(bounce.Position.Whole) };
        BoyonBehavior.RunBoyonMain(bounce, bounceState, samus);
        BoyonBehavior.RunBoyonMain(bounce, bounceState, samus);
        AssertEqual(AnimationProgramCursor<BoyonProgram>.Start(BoyonProgram.Bouncing), bounceState.Program, "Samus nearby starts the bounce");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BoyonAnimations.Programs, BoyonProgram.BouncingLoop, BoyonFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(bounce, EnemyInstructionRunner.LoopUpdates(BoyonAnimations.Programs, BoyonProgram.BouncingLoop)), "Boyon bounce loop");
        AssertTrue(bounce.Properties.HasAny(EnemyProperties.ProcessOffScreen), "Boyon bounce processes off screen");
        AssertEqual((ushort?)BoyonNativeValues.BounceSound, bounceSystem.BoyonBehavior.LastBoyonSoundEffect, "Boyon bounce plays its sound");
        AssertTrue(!bounceState.BounceDisabled, "Boyon bounce permits the movement arc");
        Console.WriteLine("  Boyon behavior: the idle and bounce loops with their off-screen, sound and arc cues pass.");
    }

    /// <summary>A Boyon with a 64-pixel trigger range after its real initializer.</summary>
    private static (RoomEnemySystem, RoomEnemySlot, BoyonEnemyState) Create(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Boyon, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = BoyonFrameToolingDefinitions.Bank });
        // Parameter two is the horizontal trigger range Samus must be within to start a bounce.
        slot.Parameter2 = 64;
        slot.Position = slot.Position.WithWhole(new(256, 512));
        enemies.BoyonBehavior.InitializeBoyon(slot);
        return (enemies, slot, enemies.BoyonStates[0] ?? throw new InvalidOperationException("Boyon has no state."));
    }

    /// <summary>Runs the Boyon behavior check, its program oracle and the compiled selector check.</summary>
    [VerificationCommand("--lookup-boyon-programs")]
    internal static int LookupBoyonProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBoyonBehavior), () => VerifyBoyonBehavior(rom));
        Suite(nameof(BoyonAnimationsVerification.VerifyBoyonAnimations), () => BoyonAnimationsVerification.VerifyBoyonAnimations(rom));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }
}
