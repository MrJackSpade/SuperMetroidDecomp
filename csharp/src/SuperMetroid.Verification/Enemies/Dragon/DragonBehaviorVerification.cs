using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Dragon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Dragon's idle, wing and attack animations through the real install path.</summary>
internal static class DragonBehaviorVerification
{
    /// <summary>
    /// Each animation started on a Dragon part runs its program: the idle
    /// pose shows once and holds, the wings loop on time, and the attack shows its five poses for 32, 3, 7, 3 and 1
    /// updates before its cue finishes the attack, which main AI answers on the next update (#2476).
    /// </summary>
    internal static void VerifyDragonBehavior()
    {
        foreach (DragonSegment selector in Enum.GetValues<DragonSegment>())
        {
            DragonSegment program = selector;
            (RoomEnemySystem enemies, RoomEnemyPlace place, DragonPartState state) = Install(selector);
            ushort[] poses = DragonAnimationsConstants.Spritemaps(program).ToArray();
            switch (selector)
            {
                case DragonSegment.IdleFacingLeft or DragonSegment.IdleFacingRight:
                    // A still picture shows once and holds (#2275).
                    AssertEqual(EnemyInstructionRunner.ExpectedFrames([poses[0]], [1]),
                        EnemyInstructionRunner.ShownFrames(place, 3), $"Dragon {program} pose");
                    break;
                case DragonSegment.WingsFacingLeft or DragonSegment.WingsFacingRight:
                    AssertEqual(EnemyInstructionRunner.ExpectedLoop(DragonAnimations.Segments, program, DragonFrameToolingDefinitions.Spritemap),
                        EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(DragonAnimations.Segments, program)), $"Dragon {program} loop");
                    break;
                case DragonSegment.AttackingFacingLeft:
                case DragonSegment.AttackingFacingRight:
                default:
                    int[] holds = [32, 3, 7, 3, 1];
                    AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. poses, poses[^1]], [.. holds, 1]),
                        EnemyInstructionRunner.ShownFrames(place, holds.Sum() + 2), $"Dragon {program} poses");
                    break;
            }
            bool attacks = selector is DragonSegment.AttackingFacingLeft or DragonSegment.AttackingFacingRight;
            AssertEqual(attacks, state is DragonEnemyState { Action: DragonAction.AttackFinished }, $"Dragon {program} attack-completion cue");
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
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        foreach (int bodySlot in new[] { 0, 2 })
        {
            for (int part = 0; part < 2; part++)
            {
                RoomEnemyPlace place = enemies.Places.InUpdateOrder[bodySlot + part];
                place.SetSpawnParameter1(EnemyDefinitionId.Dragon, (ushort)part);
                place.Bind(EnemyDefinitionId.Dragon, place.Definition);
                place.Bind(place.HeldOccupant().DefinitionId, default(RoomEnemyDefinition));
                if (part == 0)
                    enemies.DragonBehavior.InitializeDragonBody(place);
                else
                    enemies.DragonBehavior.InitializeDragonWing(place);
            }
        }
        foreach (int bodySlot in new[] { 0, 2 })
        {
            DragonEnemyState body = enemies.DragonStates[bodySlot] ?? throw new InvalidOperationException("Dragon has no state.");
            body.Action = DragonAction.AttackFinished;
            body.Facing = HorizontalDirection.Left;
            body.AttackCounter = 3;
            enemies.DragonBehavior.RunDragonAttacking(enemies.Places.InUpdateOrder[bodySlot].HeldOccupant(), enemies.Places.InUpdateOrder[bodySlot], body);
        }
        SoundEffectId fireball = EnemySoundEffectDefinitions.DragonFireballSound;
        AssertEqual(2, enemies.Audio.SoundRequests.Count(request => request.SoundEffect == fireball),
            "two Dragons attacking in one frame queue two fireball sounds");
        Console.WriteLine("  Dragon behavior: two Dragons attacking in one frame queue two fireball sounds.");
    }

    /// <summary>A Dragon body, or a wing for the wing selectors, with <paramref name="selector"/> freshly installed.</summary>
    private static InstallResult Install(DragonSegment selector)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        bool wing = selector is DragonSegment.WingsFacingLeft or DragonSegment.WingsFacingRight;
        place.SetSpawnParameter1(EnemyDefinitionId.Dragon, wing ? (ushort)1 : (ushort)0);
        place.Bind(EnemyDefinitionId.Dragon, place.Definition);
        place.Bind(place.HeldOccupant().DefinitionId, default(RoomEnemyDefinition));
        if (wing)
            enemies.DragonBehavior.InitializeDragonWing(place);
        else
            enemies.DragonBehavior.InitializeDragonBody(place);
        DragonPartState state = enemies.DragonPartStates[0] ?? throw new InvalidOperationException("Dragon has no state.");
        // An attack plays while the body attacks, whose finishing cue it answers (#2476).
        if (state is DragonEnemyState body && selector is DragonSegment.AttackingFacingLeft or DragonSegment.AttackingFacingRight)
            body.Action = DragonAction.Attacking;
        state.Playback = EnemyAnimationPlayback.Start(place, DragonAnimations.StartingWith(selector));
        return new InstallResult(enemies, place, state);
    }

    /// <summary>Runs the Dragon behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--dragon-instruction-mechanics")]
    internal static int DragonBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyDragonBehavior), () => VerifyDragonBehavior());
        Suite(nameof(DragonAnimationsVerification.VerifyDragonAnimations), () => DragonAnimationsVerification.VerifyDragonAnimations(rom));
        return 0;
    }
}
