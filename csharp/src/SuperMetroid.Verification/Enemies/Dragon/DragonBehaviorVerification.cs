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
            Action animate = Animation(place, state);
            ushort[] poses = DragonAnimationsConstants.Spritemaps(program).ToArray();
            switch (selector)
            {
                case DragonSegment.IdleFacingLeft or DragonSegment.IdleFacingRight:
                    // A still picture shows once and holds (#2275).
                    AssertEqual(EnemyInstructionRunner.ExpectedFrames([poses[0]], [1]),
                        EnemyInstructionRunner.ShownFrames(place, 3, animate), $"Dragon {program} pose");
                    break;
                case DragonSegment.WingsFacingLeft or DragonSegment.WingsFacingRight:
                    AssertEqual(EnemyInstructionRunner.ExpectedLoop(DragonAnimations.Segments, program, DragonFrameToolingDefinitions.Spritemap),
                        EnemyInstructionRunner.ShownFrames(place, EnemyInstructionRunner.LoopUpdates(DragonAnimations.Segments, program), animate), $"Dragon {program} loop");
                    break;
                case DragonSegment.AttackingFacingLeft:
                case DragonSegment.AttackingFacingRight:
                default:
                    int[] holds = [32, 3, 7, 3, 1];
                    AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. poses, poses[^1]], [.. holds, 1]),
                        EnemyInstructionRunner.ShownFrames(place, holds.Sum() + 2, animate), $"Dragon {program} poses");
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
        // Two Dragons are set up as their room's population sets them up, each wing after the body that offers it.
        _ = enemies.LoadPopulation(PopulationPlacementAccess.Entry(EnemyDefinitionId.Dragon, new DragonSpawnSettings(DragonPart.Body)), PopulationPlacementAccess.Entry(EnemyDefinitionId.Dragon, new DragonSpawnSettings(DragonPart.Wing)),
            PopulationPlacementAccess.Entry(EnemyDefinitionId.Dragon, new DragonSpawnSettings(DragonPart.Body)), PopulationPlacementAccess.Entry(EnemyDefinitionId.Dragon, new DragonSpawnSettings(DragonPart.Wing)));
        foreach (int bodySlot in new[] { 0, 2 })
        {
            DragonEnemyState body = enemies.DragonStates[bodySlot] ?? throw new InvalidOperationException("Dragon has no state.");
            body.Action = DragonAction.AttackFinished;
            body.Facing = HorizontalDirection.Left;
            body.AttackCounter = 3;
            enemies.DragonBehavior.RunDragonMain(enemies.Places.InUpdateOrder[bodySlot].HeldOccupant(), enemies.Places.InUpdateOrder[bodySlot], body, body.WingState, RoomEnemyFrameFixtures.StandaloneSamus());
        }
        SoundEffectId fireball = EnemySoundEffectDefinitions.DragonFireballSound;
        AssertEqual(2, enemies.Audio.SoundRequests.Count(request => request.SoundEffect == fireball),
            "two Dragons attacking in one frame queue two fireball sounds");
        Console.WriteLine("  Dragon behavior: two Dragons attacking in one frame queue two fireball sounds.");
    }

    /// <summary>The animation step of the Dragon part whose state is <paramref name="state"/>, as its entity takes it.</summary>
    private static Action Animation(RoomEnemyPlace place, DragonPartState state) => state switch
    {
        DragonEnemyState body => () => DragonBehavior.AdvanceDragonBodyTypedAnimation(place, body),
        DragonWingState wing => () => DragonBehavior.AdvanceDragonWingTypedAnimation(place, wing),
        _ => throw new InvalidOperationException($"Undefined Dragon part state {state}."),
    };

    /// <summary>A Dragon body, or a wing for the wing selectors, with <paramref name="selector"/> freshly installed.</summary>
    private static InstallResult Install(DragonSegment selector)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        bool wing = selector is DragonSegment.WingsFacingLeft or DragonSegment.WingsFacingRight;
        // The body is set up as its room's population sets it up, and a wing after the body that offers it.
        int parts = wing ? 2 : 1;
        _ = enemies.LoadPopulation([.. new[] { PopulationPlacementAccess.Entry(EnemyDefinitionId.Dragon, new DragonSpawnSettings(DragonPart.Body)), PopulationPlacementAccess.Entry(EnemyDefinitionId.Dragon, new DragonSpawnSettings(DragonPart.Wing)) }.Take(parts)]);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[parts - 1];
        DragonEnemyState bodyState = enemies.DragonStates[0] ?? throw new InvalidOperationException("Dragon has no state.");
        // The wing plays on the state its body began for it, which the wing took as it was set up.
        DragonPartState state = wing ? bodyState.WingState : bodyState;
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
