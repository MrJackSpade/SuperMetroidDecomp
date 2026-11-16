using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Enemies.Polyp.PolypBehaviorVerification;
using static SuperMetroid.Verification.Enemies.Polyp.PolypRockAnimationsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.SingleFrameEnemyVisualsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Enemies.Polyp;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Kzan;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that the Kzan top starts its idle pose and holds it.</summary>
internal static class KzanBehaviorVerification
{
    /// <summary>The initializer starts the idle program, which shows its one frame and then holds it.</summary>
    internal static void VerifyKzanBehavior()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.KzanTop, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        place.Position = place.Position.WithWholeY(128);
        place.SetSpawnSettings(new KzanSpawnSettings(FallSpeed: SignedSixteenSixteen.FromParts(4, 0), FallWait: 112, FallDistance: 80));
        KzanBehavior.InitializeKzanTop(place);
        AssertEqual(new AnimationPlayback<KzanSegment>(KzanAnimations.StartingWith(KzanSegment.Idle), AnimationPlace.StartOfAnimation),
            (enemies.Places.InUpdateOrder[0].EntityState<KzanTopEntity, KzanEnemyState>() ?? throw new InvalidOperationException("The Kzan has no state.")).Playback,
            "Kzan initializer animation");
        // The update that reaches the native sleep yields with the new-frame flag still set, so it reads as shown twice.
        ushort frame = KzanAnimationsConstants.Spritemaps(KzanSegment.Idle).Single();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([frame, frame], [1, 1]),
            EnemyInstructionRunner.ShownFrames(place, 4), "Kzan shows its idle frame once and holds it");
        Console.WriteLine("  Kzan behavior: the top shows its idle pose and holds it.");
    }

    /// <summary>
    /// A top spawned sixteen pixels above the room's bottom edge with a 32-pixel fall has its falling target wrap to Y 16.
    /// The production main AI compares the position with both targets by the wrapped spatial difference: at Y 65520 the
    /// top is still above the wrapped falling target and keeps falling; at Y 16 it lands there with its landing sound.
    /// Rising from Y 16 toward the target at Y 65520 it keeps rising, and passing above it at Y 65519 stops it there.
    /// </summary>
    internal static void VerifyKzanTargetsWrap()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.KzanTop, place.Definition);
        place.SetSpawnParameter1(EnemyDefinitionId.KzanTop, 0);
        place.SetSpawnParameter2(EnemyDefinitionId.KzanTop, 32 * 256);
        place.Position = place.Position.WithWhole(new(128, 65520));
        KzanBehavior.InitializeKzanTop(place);
        KzanEnemyState state = place.RequireSpeciesState<KzanEnemyState>();
        AssertEqual(16, state.FallingTargetY, "the falling target wraps past the room's bottom edge");
        AssertEqual(65520, state.RisingTargetY, "the rising target is the spawn position");
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(0));

        state.Action = KzanAction.Falling;
        enemies.KzanBehavior.RunKzanTopMainAi(place, frame);
        AssertEqual(KzanAction.Falling, state.Action, "above the wrapped falling target the top keeps falling");
        AssertEqual((ushort)65520, place.Position.Y.Whole, "the first falling step uses the zero speed entry");
        AssertEqual(SignedSixteenSixteen.FromSixteenthsOfPixel(1), state.FallingSpeed, "each falling step accelerates the fall by a sixteenth of a pixel per update");

        place.Position = place.Position.WithWholeY(16);
        enemies.KzanBehavior.RunKzanTopMainAi(place, frame);
        AssertEqual(KzanAction.WaitingToRise, state.Action, "reaching the wrapped falling target lands the top");
        AssertEqual((ushort)16, place.Position.Y.Whole, "the top lands on the falling target");
        AssertEqual((SoundEffectId?)EnemySoundEffectDefinitions.KzanLandingSoundEffect,
            enemies.Audio.LatestSound, "landing queues the library-two landing sound");

        state.Action = KzanAction.Rising;
        enemies.KzanBehavior.RunKzanTopMainAi(place, frame);
        AssertEqual(KzanAction.Rising, state.Action, "below the wrapped rising target the top keeps rising");
        AssertEqual((ushort)15, place.Position.Y.Whole, "half a pixel of rise borrows from the whole position");

        place.Position = place.Position.WithWholeY(65519);
        enemies.KzanBehavior.RunKzanTopMainAi(place, frame);
        AssertEqual(KzanAction.WaitingToFall, state.Action, "passing above the rising target stops the rise");
        AssertEqual((ushort)65520, place.Position.Y.Whole, "the top stops on the rising target");
        Console.WriteLine("  Kzan targets: the top's fall and rise compare with wrapped targets by the wrapped spatial difference.");
    }

    /// <summary>Runs the Kzan, Polyp, and Polyp-rock single-frame program and artwork checks.</summary>
    [VerificationCommand("--single-frame-enemy-visuals")]
    internal static int SingleFrameEnemyVisualsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyKzanBehavior), () => VerifyKzanBehavior());
        Suite(nameof(VerifyKzanTargetsWrap), () => VerifyKzanTargetsWrap());
        Suite(nameof(KzanAnimationsVerification.VerifyKzanAnimations), () => KzanAnimationsVerification.VerifyKzanAnimations(rom));
        Suite(nameof(VerifyPolypBehavior), () => VerifyPolypBehavior());
        Suite(nameof(PolypAnimationsVerification.VerifyPolypAnimations), () => PolypAnimationsVerification.VerifyPolypAnimations(rom));
        Suite(nameof(VerifyPolypRockAnimations), () => VerifyPolypRockAnimations(rom));
        Suite(nameof(PolypRockBehaviorVerification.VerifyPolypRockBehavior), () => PolypRockBehaviorVerification.VerifyPolypRockBehavior(rom));
        Suite(nameof(VerifySingleFrameEnemyVisuals), () => VerifySingleFrameEnemyVisuals());
        return 0;
    }
}
