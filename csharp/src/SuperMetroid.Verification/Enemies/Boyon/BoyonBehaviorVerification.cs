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
    internal static void VerifyBoyonBehavior()
    {
        (RoomEnemySystem idleSystem, RoomEnemySlot idle, BoyonEnemyState idleState) = Create();
        AssertEqual(AnimationProgramCursor<BoyonProgram>.Start(BoyonProgram.Idle), idleState.Program, "Boyon initializer starts idling");
        idle.Properties = idle.Properties.With(EnemyProperties.ProcessOffScreen);
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BoyonAnimations.Programs, BoyonProgram.IdleLoop, BoyonFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(idle, EnemyInstructionRunner.LoopUpdates(BoyonAnimations.Programs, BoyonProgram.IdleLoop)), "Boyon idle loop");
        AssertTrue(!idle.Properties.HasAny(EnemyProperties.ProcessOffScreen), "Boyon idle stops off-screen processing");

        (RoomEnemySystem bounceSystem, RoomEnemySlot bounce, BoyonEnemyState bounceState) = Create();
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(bounce.Position.Whole) };
        BoyonBehavior.RunBoyonMain(bounce, bounceState, samus);
        BoyonBehavior.RunBoyonMain(bounce, bounceState, samus);
        AssertEqual(AnimationProgramCursor<BoyonProgram>.Start(BoyonProgram.Bouncing), bounceState.Program, "Samus nearby starts the bounce");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(BoyonAnimations.Programs, BoyonProgram.BouncingLoop, BoyonFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(bounce, EnemyInstructionRunner.LoopUpdates(BoyonAnimations.Programs, BoyonProgram.BouncingLoop)), "Boyon bounce loop");
        AssertTrue(bounce.Properties.HasAny(EnemyProperties.ProcessOffScreen), "Boyon bounce processes off screen");
        AssertEqual((SoundEffectId?)SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, BoyonNativeValues.BounceSound),
            bounceSystem.BoyonBehavior.LastBoyonSoundEffect, "Boyon bounce plays its library-two bounce sound");
        AssertTrue(!bounceState.BounceDisabled, "Boyon bounce permits the movement arc");
        Console.WriteLine("  Boyon behavior: the idle and bounce loops with their off-screen, sound and arc cues pass.");
    }

    /// <summary>
    /// Rising past the top of the arc retreats the curve index from zero to 65535 and selects falling; the next falling
    /// update advances it from 65535 back to zero. Both steps run through the production main AI, and the index's slot
    /// variable holds the same word.
    /// </summary>
    internal static void VerifyBoyonCurveIndexWrapsAtTheTop()
    {
        (_, RoomEnemySlot slot, BoyonEnemyState state) = Create();
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(slot.Position.Whole) };
        BoyonBehavior.RunBoyonMain(slot, state, samus);
        state.BounceMovement = BoyonBounceMovement.Rising;
        state.SpeedTableIndex = 0;

        BoyonBehavior.RunBoyonMain(slot, state, samus);
        AssertEqual(BoyonEnemyState.LargestSpeedTableIndex, state.SpeedTableIndex, "rising past entry zero wraps the curve index");
        AssertEqual(BoyonBounceMovement.Falling, state.BounceMovement, "rising past entry zero selects falling");

        BoyonBehavior.RunBoyonMain(slot, state, samus);
        AssertEqual(0, state.SpeedTableIndex, "the first falling update advances the wrapped index to entry zero");
        AssertEqual(BoyonBounceMovement.Falling, state.BounceMovement, "entry zero is above the baseline, so falling continues");
        Console.WriteLine("  Boyon curve index: rising wraps it from zero to 65535 and falling advances it back to zero.");
    }

    /// <summary>
    /// The first main-AI update derives the baseline curve index by accumulating speed until the distance reaches the jump
    /// height, compared by the sign of the word difference. With multiplier one, height 32768 needs 144 entries, while
    /// height 36864 already counts as reached after the first entry, because zero minus 36864 is a positive word.
    /// </summary>
    internal static void VerifyBoyonJumpHeightComparesByWordSign()
    {
        foreach ((int heightSelector, int expected) in new (int, int)[] { (5, 144), (6, 1) })
        {
            var enemies = new RoomEnemySystem();
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Boyon, slot.Definition);
            slot.Parameter1 = checked((ushort)(heightSelector * 256));
            slot.Parameter2 = 64;
            slot.Position = slot.Position.WithWhole(new(256, 512));
            BoyonBehavior.InitializeBoyon(slot);
            BoyonEnemyState state = BoyonBehavior.RequireBoyonState(slot);
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(slot.Position.Whole) };

            BoyonBehavior.RunBoyonMain(slot, state, samus);

            AssertEqual(expected, state.InitialBounceSpeedTableIndex, $"Boyon baseline curve index for height selector {heightSelector}");
        }
        Console.WriteLine("  Boyon jump height: the derivation compares distance and height by the sign of their word difference.");
    }

    /// <summary>A Boyon with a 64-pixel trigger range after its real initializer.</summary>
    private static (RoomEnemySystem, RoomEnemySlot, BoyonEnemyState) Create()
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Boyon, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = BoyonFrameToolingDefinitions.Bank });
        // Parameter two is the horizontal trigger range Samus must be within to start a bounce.
        slot.Parameter2 = 64;
        slot.Position = slot.Position.WithWhole(new(256, 512));
        BoyonBehavior.InitializeBoyon(slot);
        return (enemies, slot, enemies.BoyonStates[0] ?? throw new InvalidOperationException("Boyon has no state."));
    }

    /// <summary>Runs the Boyon behavior check, its program oracle and the compiled selector check.</summary>
    [VerificationCommand("--lookup-boyon-programs")]
    internal static int LookupBoyonProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBoyonBehavior), () => VerifyBoyonBehavior());
        Suite(nameof(VerifyBoyonCurveIndexWrapsAtTheTop), () => VerifyBoyonCurveIndexWrapsAtTheTop());
        Suite(nameof(VerifyBoyonJumpHeightComparesByWordSign), () => VerifyBoyonJumpHeightComparesByWordSign());
        Suite(nameof(BoyonAnimationsVerification.VerifyBoyonAnimations), () => BoyonAnimationsVerification.VerifyBoyonAnimations(rom));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }
}
