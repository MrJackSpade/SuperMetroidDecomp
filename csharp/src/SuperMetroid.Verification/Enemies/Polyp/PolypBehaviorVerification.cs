using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Polyp;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks that Polyp starts its stationary pose and holds it.</summary>
internal static class PolypBehaviorVerification
{
    /// <summary>The real initializer starts the stationary program, which shows its one frame and then holds it.</summary>
    internal static void VerifyPolypBehavior()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().SeedRandom = _ => { };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.LavaRocks, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        enemies.PolypBehavior.InitializePolyp(slot);
        AssertEqual(AnimationProgramCursor<PolypProgram>.Start(PolypProgram.Stationary),
            (enemies.SlotTable.Slots[0].EntityState<PolypEntity, PolypEnemyState>() ?? throw new InvalidOperationException("The Polyp has no state.")).Program,
            "real Polyp initializer starts the stationary program");
        // The update that reaches the native sleep yields with the new-frame flag still set, so it reads as shown twice.
        ushort frame = PolypAnimationsConstants.Spritemaps(PolypProgram.Stationary).Single();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([frame, frame], [1, 1]),
            EnemyInstructionRunner.ShownFrames(slot, 4), "Polyp shows its stationary frame once and holds it");
        Console.WriteLine("  Polyp behavior: the stationary pose shows and holds.");
    }

    /// <summary>Runs the focused Polyp behavior check and program oracle.</summary>
    [VerificationCommand("--polyp-instruction-mechanics")]
    internal static int PolypInstructionMechanicsCommand()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyPolypBehavior), () => VerifyPolypBehavior());
        Suite(nameof(PolypAnimationsVerification.VerifyPolypAnimations), () => PolypAnimationsVerification.VerifyPolypAnimations(rom));
        return 0;
    }
}
