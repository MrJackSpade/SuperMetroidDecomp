using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Enemies.Polyp.PolypBehaviorVerification;
using static SuperMetroid.Verification.Enemies.Polyp.PolypRockAnimationsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.SingleFrameEnemyVisualsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.Kzan.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Enemies.Polyp;
namespace SuperMetroid.Verification.Enemies.Kzan;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that the Kzan top starts its idle pose and holds it.</summary>
internal static class KzanBehaviorVerification
{
    /// <summary>The initializer starts the idle program, which shows its one frame and then holds it.</summary>
    internal static void VerifyKzanBehavior(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.KzanTop, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = KzanFrameToolingDefinitions.Bank });
        slot.Position = slot.Position.WithWholeY(128);
        enemies.KzanBehavior.InitializeKzanTop(slot);
        AssertEqual(AnimationProgramCursor<KzanProgram>.Start(KzanProgram.Idle),
            ((enemies.SlotTable.Slots[0].Entity as KzanEntity)?.State ?? throw new InvalidOperationException("The Kzan has no state.")).Program,
            "Kzan initializer animation");
        // The update that reaches the native sleep yields with the new-frame flag still set, so it reads as shown twice.
        ushort frame = KzanAnimationsConstants.Spritemaps(KzanProgram.Idle).Single();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([frame, frame], [1, 1]),
            EnemyInstructionRunner.ShownFrames(slot, 4), "Kzan shows its idle frame once and holds it");
        Console.WriteLine("  Kzan behavior: the top shows its idle pose and holds it.");
    }

    /// <summary>Runs the Kzan, Polyp, and Polyp-rock single-frame program and artwork checks.</summary>
    [VerificationCommand("--single-frame-enemy-visuals")]
    internal static int SingleFrameEnemyVisualsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyKzanBehavior), () => VerifyKzanBehavior(rom));
        Suite(nameof(KzanAnimationsVerification.VerifyKzanAnimations), () => KzanAnimationsVerification.VerifyKzanAnimations(rom));
        Suite(nameof(VerifyPolypBehavior), () => VerifyPolypBehavior(rom));
        Suite(nameof(PolypAnimationsVerification.VerifyPolypAnimations), () => PolypAnimationsVerification.VerifyPolypAnimations(rom));
        Suite(nameof(VerifyPolypRockAnimations), () => VerifyPolypRockAnimations(rom));
        Suite(nameof(PolypRockBehaviorVerification.VerifyPolypRockBehavior), () => PolypRockBehaviorVerification.VerifyPolypRockBehavior(rom));
        Suite(nameof(VerifySingleFrameEnemyVisuals), () => VerifySingleFrameEnemyVisuals());
        return 0;
    }
}
