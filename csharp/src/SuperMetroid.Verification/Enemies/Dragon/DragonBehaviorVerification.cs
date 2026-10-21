using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Dragon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Dragon's idle, wing and attack animations through the real install path.</summary>
internal static class DragonBehaviorVerification
{
    /// <summary>
    /// Each selector installed through <see cref="DragonBehavior.InstallDragonInstructionList"/> runs its program: the idle
    /// pose shows once and holds, the wings loop on time, and the attack shows its five poses for 32, 3, 7, 3 and 1
    /// updates before its cue marks the attack complete for main AI.
    /// </summary>
    internal static void VerifyDragonBehavior()
    {
        foreach (DragonAnimationSelector selector in Enum.GetValues<DragonAnimationSelector>().Where(selector => selector != DragonAnimationSelector.ForceReinstall))
        {
            DragonProgram program = DragonAnimationDefinitions.Program(selector);
            (RoomEnemySystem enemies, RoomEnemySlot slot, DragonEnemyState state) = Install(selector);
            ushort[] poses = DragonAnimationsConstants.Spritemaps(program).ToArray();
            switch (selector)
            {
                case DragonAnimationSelector.IdleFacingLeft or DragonAnimationSelector.IdleFacingRight:
                    AssertEqual(EnemyInstructionRunner.ExpectedFrames([poses[0], poses[0]], [1, 1]),
                        EnemyInstructionRunner.ShownFrames(slot, 3), $"Dragon {program} pose");
                    break;
                case DragonAnimationSelector.WingsFacingLeft or DragonAnimationSelector.WingsFacingRight:
                    AssertEqual(EnemyInstructionRunner.ExpectedLoop(DragonAnimations.Programs, program, DragonFrameToolingDefinitions.Spritemap),
                        EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(DragonAnimations.Programs, program)), $"Dragon {program} loop");
                    break;
                case DragonAnimationSelector.AttackingFacingLeft:
                case DragonAnimationSelector.AttackingFacingRight:
                case DragonAnimationSelector.ForceReinstall:
                default:
                    int[] holds = [32, 3, 7, 3, 1];
                    AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. poses, poses[^1]], [.. holds, 1]),
                        EnemyInstructionRunner.ShownFrames(slot, holds.Sum() + 2), $"Dragon {program} poses");
                    break;
            }
            bool attacks = selector is DragonAnimationSelector.AttackingFacingLeft or DragonAnimationSelector.AttackingFacingRight;
            AssertEqual(attacks, state.AnimationFinished, $"Dragon {program} attack-completion cue");
        }
        Console.WriteLine("  Dragon behavior: both idles, both wing loops and both attacks with their completion cue pass.");
        VerifyDragonsAttackingTogether();
    }

    /// <summary>
    /// `$A2:E70F` queues the fireball sound at every attack. Two Dragons firing in one frame queue it twice;
    /// the contributed lowc movie's second queued sound makes the next door's sound wait (update 118,158).
    /// </summary>
    private static void VerifyDragonsAttackingTogether()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        foreach (int bodySlot in new[] { 0, 2 })
        {
            for (int part = 0; part < 2; part++)
            {
                RoomEnemySlot slot = enemies.SlotTable.Slots[bodySlot + part];
                slot.Bind(EnemyDefinitionId.Dragon, slot.Definition);
                slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = DragonFrameToolingDefinitions.Bank });
                slot.SetSpawnParameter1((ushort)part);
                DragonBehavior.InitializeDragon(slot);
            }
        }
        foreach (int bodySlot in new[] { 0, 2 })
        {
            DragonEnemyState body = enemies.DragonStates[bodySlot] ?? throw new InvalidOperationException("Dragon has no state.");
            body.Function = DragonEnemyFunction.Attacking;
            body.RequestedInstructionListIndex = DragonAnimationSelector.AttackingFacingLeft;
            body.AttackCounter = WrappingCountdown.Start(3);
            body.AnimationFinished = true;
            enemies.DragonBehavior.RunDragonAttacking(enemies.SlotTable.Slots[bodySlot], body);
        }
        SoundEffectId fireball = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, EnemySoundEffectDefinitions.DragonFireballSound);
        AssertEqual(2, enemies.Audio.SoundRequests.Count(request => request.SoundEffect == fireball),
            "two Dragons attacking in one frame queue two fireball sounds");
        Console.WriteLine("  Dragon behavior: two Dragons attacking in one frame queue two fireball sounds.");
    }

    /// <summary>A Dragon body, or a wing for the wing selectors, with <paramref name="selector"/> freshly installed.</summary>
    private static InstallResult Install(DragonAnimationSelector selector)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Dragon, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = DragonFrameToolingDefinitions.Bank });
        slot.SetSpawnParameter1(selector is DragonAnimationSelector.WingsFacingLeft or DragonAnimationSelector.WingsFacingRight ? (ushort)1 : (ushort)0);
        DragonBehavior.InitializeDragon(slot);
        DragonEnemyState state = enemies.DragonStates[0] ?? throw new InvalidOperationException("Dragon has no state.");
        state.InstalledInstructionListIndex = DragonAnimationSelector.ForceReinstall;
        state.RequestedInstructionListIndex = selector;
        state.AnimationFinished = false;
        DragonBehavior.InstallDragonInstructionList(slot, state);
        return new InstallResult(enemies, slot, state);
    }

    /// <summary>Runs the Dragon behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--dragon-instruction-mechanics")]
    internal static int DragonBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyDragonBehavior), () => VerifyDragonBehavior());
        Suite(nameof(DragonAnimationsVerification.VerifyDragonAnimations), () => DragonAnimationsVerification.VerifyDragonAnimations(rom));
        return 0;
    }
}
