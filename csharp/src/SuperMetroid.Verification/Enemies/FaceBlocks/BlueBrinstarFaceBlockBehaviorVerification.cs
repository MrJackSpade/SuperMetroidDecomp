using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyVisualSelectorInventoryVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.FaceBlocks;
using SuperMetroid.Verification.Harness;

/// <summary>Checks which face-block animation each activation side starts and how long each pose holds.</summary>
internal static class BlueBrinstarFaceBlockBehaviorVerification
{
    /// <summary>
    /// The initializer starts the neutral pose, which shows once and holds; Morph Ball-equipped Samus approaching from
    /// the left or right activates the block, which then holds the neutral pose for 48 updates and its two turning poses
    /// for 16 each before holding the last. The exported frames are the poses those programs show.
    /// </summary>
    internal static void VerifyBlueBrinstarFaceBlockBehavior(SuperMetroidAddressSpace rom)
    {
        (RoomEnemySystem enemies, RoomEnemySlot slot, BlueBrinstarFaceBlockEnemyState state) = Create(rom, parameter2: 0);
        AssertEqual(AnimationProgramCursor<BlueBrinstarFaceBlockProgram>.Start(BlueBrinstarFaceBlockProgram.Initial), state.Program, "face block starts neutral");
        ushort neutral = BlueBrinstarFaceBlockAnimations.Spritemaps(BlueBrinstarFaceBlockProgram.Initial).Single();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([neutral, neutral], [1, 1]), EnemyInstructionRunner.ShownFrames(enemies, slot, 3), "face block neutral pose");

        foreach ((ushort parameter2, int samusX, BlueBrinstarFaceBlockProgram program) in new[]
                 {
                     ((ushort)0, -32, BlueBrinstarFaceBlockProgram.SamusLeft),
                     ((ushort)1, 32, BlueBrinstarFaceBlockProgram.SamusRight),
                 })
        {
            (enemies, slot, state) = Create(rom, parameter2);
            var samus = new SamusState
            {
                CollectedItems = BlueBrinstarFaceBlockNativeValues.MorphBallItem,
                Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(slot.Position.X.Whole + samusX)), slot.Position.Y.Whole)),
            };
            enemies.BlueBrinstarFaceBlockBehavior.RunBlueBrinstarFaceBlockMain(slot, state, samus);
            AssertTrue(state.Activated, $"face block {program} activates");
            AssertEqual(AnimationProgramCursor<BlueBrinstarFaceBlockProgram>.Start(program), state.Program, $"face block {program} selection");
            int[] holds = [48, 16, 16];
            AssertEqual(EnemyInstructionRunner.ExpectedFrames(BlueBrinstarFaceBlockAnimations.Spritemaps(program), holds),
                EnemyInstructionRunner.ShownFrames(enemies, slot, holds.Sum() + 1), $"face block {program} poses");
        }

        AssertEqual(5, BlueBrinstarFaceBlockVisualDefinitions.Frames().Select(frame => frame.Pointer).Distinct().Count(), "five distinct face-block poses");
        AssertTrue(BlueBrinstarFaceBlockVisualDefinitions.Frames().Select(frame => frame.Pointer).ToHashSet().SetEquals(
                BlueBrinstarFaceBlockAnimations.Spritemaps(Enum.GetValues<BlueBrinstarFaceBlockProgram>())),
            "the exported face-block frames are exactly the poses the programs show");
        Console.WriteLine("  Face block behavior: neutral start, both activation sides and their 48/16/16 turning poses pass.");
    }

    /// <summary>A face block initialized at (256, 256) with a 64-pixel trigger range.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot, BlueBrinstarFaceBlockEnemyState State) Create(SuperMetroidAddressSpace rom, ushort parameter2)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.FaceBlock;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = BlueBrinstarFaceBlockFrameDefinitions.Bank };
        slot.Parameter1 = 64;
        slot.Parameter2 = parameter2;
        slot.Position = slot.Position.WithWhole(new(256, 256));
        enemies.BlueBrinstarFaceBlockBehavior.InitializeBlueBrinstarFaceBlock(slot, null);
        return (enemies, slot, enemies.BlueBrinstarFaceBlockStates[0] ?? throw new InvalidOperationException("The face block has no state."));
    }

    /// <summary>Runs the face-block behavior check, its program oracle and the compiled selector check.</summary>
    [VerificationCommand("--lookup-face-block-programs")]
    internal static int LookupFaceBlockProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBlueBrinstarFaceBlockBehavior), () => VerifyBlueBrinstarFaceBlockBehavior(rom));
        Suite(nameof(BlueBrinstarFaceBlockAnimationsVerification.VerifyBlueBrinstarFaceBlockAnimations), () => BlueBrinstarFaceBlockAnimationsVerification.VerifyBlueBrinstarFaceBlockAnimations(rom));
        Suite(nameof(VerifyCompiledEnemyVisualSelectors), () => VerifyCompiledEnemyVisualSelectors());
        return 0;
    }
}
