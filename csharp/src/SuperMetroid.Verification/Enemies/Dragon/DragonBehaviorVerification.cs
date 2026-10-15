using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Dragon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Dragon's idle, wing and attack animations through the real install path.</summary>
internal static class DragonBehaviorVerification
{
    /// <summary>
    /// Each selector installed through <see cref="DragonBehavior.InstallDragonInstructionList"/> runs its program: the idle
    /// pose shows once and holds, the wings loop on time, and the attack shows its five poses for 32, 3, 7, 3 and 1
    /// updates before its cue marks the attack complete for main AI.
    /// </summary>
    internal static void VerifyDragonBehavior(SuperMetroidAddressSpace rom)
    {
        foreach (DragonAnimationSelector selector in Enum.GetValues<DragonAnimationSelector>().Where(selector => selector != DragonAnimationSelector.ForceReinstall))
        {
            DragonProgram program = DragonAnimationDefinitions.Program(selector);
            (RoomEnemySystem enemies, RoomEnemySlot slot, DragonEnemyState state) = Install(rom, selector);
            ushort[] poses = DragonAnimationsConstants.Spritemaps(program).ToArray();
            switch (selector)
            {
                case DragonAnimationSelector.IdleFacingLeft or DragonAnimationSelector.IdleFacingRight:
                    AssertEqual(EnemyInstructionRunner.ExpectedFrames([poses[0], poses[0]], [1, 1]),
                        EnemyInstructionRunner.ShownFrames(enemies, slot, 3), $"Dragon {program} pose");
                    break;
                case DragonAnimationSelector.WingsFacingLeft or DragonAnimationSelector.WingsFacingRight:
                    AssertEqual(EnemyInstructionRunner.ExpectedLoop(DragonAnimations.Programs, program, DragonFrameDefinitions.Spritemap),
                        EnemyInstructionRunner.ShownFrames(enemies, slot, EnemyInstructionRunner.LoopUpdates(DragonAnimations.Programs, program)), $"Dragon {program} loop");
                    break;
                default:
                    int[] holds = [32, 3, 7, 3, 1];
                    AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. poses, poses[^1]], [.. holds, 1]),
                        EnemyInstructionRunner.ShownFrames(enemies, slot, holds.Sum() + 2), $"Dragon {program} poses");
                    break;
            }
            bool attacks = selector is DragonAnimationSelector.AttackingFacingLeft or DragonAnimationSelector.AttackingFacingRight;
            AssertEqual(attacks, state.AnimationFinished, $"Dragon {program} attack-completion cue");
        }
        Console.WriteLine("  Dragon behavior: both idles, both wing loops and both attacks with their completion cue pass.");
    }

    /// <summary>A Dragon body, or a wing for the wing selectors, with <paramref name="selector"/> freshly installed.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot, DragonEnemyState State) Install(SuperMetroidAddressSpace rom, DragonAnimationSelector selector)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.Dragon;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = DragonFrameToolingDefinitions.Bank };
        slot.Parameter1 = selector is DragonAnimationSelector.WingsFacingLeft or DragonAnimationSelector.WingsFacingRight ? (ushort)1 : (ushort)0;
        enemies.DragonBehavior.InitializeDragon(slot);
        DragonEnemyState state = enemies.DragonStates[0] ?? throw new InvalidOperationException("Dragon has no state.");
        state.InstalledInstructionListIndex = DragonAnimationSelector.ForceReinstall;
        state.RequestedInstructionListIndex = selector;
        state.AnimationFinished = false;
        DragonBehavior.InstallDragonInstructionList(slot, state);
        return (enemies, slot, state);
    }

    /// <summary>Runs the Dragon behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--dragon-instruction-mechanics")]
    internal static int DragonBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyDragonBehavior), () => VerifyDragonBehavior(rom));
        Suite(nameof(DragonAnimationsVerification.VerifyDragonAnimations), () => DragonAnimationsVerification.VerifyDragonAnimations(rom));
        return 0;
    }
}
