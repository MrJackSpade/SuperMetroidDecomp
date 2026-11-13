using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.LavaJumpers;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks the Norfair lava jumper parent's hidden and jump animations and the follower's loop.</summary>
internal static class NorfairLavaJumperBehaviorVerification
{
    /// <summary>
    /// The parent starts hidden; the real rise handoff starts the jump animation, whose finishing cue reaches main AI.
    /// The follower initializer starts the follower loop, which plays its counted inner loop once and starts over.
    /// One-update frames are still flagged new on the update that reaches a native sleep.
    /// </summary>
    internal static void VerifyNorfairLavaJumperBehavior()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.Bind(EnemyDefinitionId.Squeept, parent.Definition);
        parent.Bind(parent.DefinitionId, default(RoomEnemyDefinition));
        parent.Position = parent.Position.WithWhole(new(256, 480));
        parent.SetSpawnSettings(new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        enemies.NorfairLavaJumpingEnemyBehavior.InitializeNorfairLavaJumpingEnemy(parent);
        NorfairLavaJumpingEnemyState parentState = enemies.NorfairLavaJumpingEnemyStates[0]
            ?? throw new InvalidOperationException("Verification requires enemies.NorfairLavaJumpingEnemyStates[0].");
        AssertEqual(new AnimationPlayback<NorfairLavaJumperSegment>(NorfairLavaJumperAnimations.StartingWith(NorfairLavaJumperSegment.Hidden), AnimationPlace.StartOfAnimation), parentState.Playback,
            "lava-jumper parent starts hidden");
        // The initializer leaves the population's instruction timer; start the hidden pose on the next update.
        parent.AnimationClock.StepOnNextUpdate();
        ushort hidden = NorfairLavaJumperAnimationsConstants.Spritemaps(NorfairLavaJumperSegment.Hidden).Single();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([hidden, hidden], [1, 1]), EnemyInstructionRunner.ShownFrames(parent, 3),
            "lava-jumper hidden pose");

        enemies.NorfairLavaJumpingEnemyBehavior.RunNorfairLavaJumpingEnemyMain(parent, parentState);
        parentState.YVelocity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-1080); // Past the native switch velocity, still rising.
        enemies.NorfairLavaJumpingEnemyBehavior.RunNorfairLavaJumpingEnemyMain(parent, parentState);
        AssertEqual(new AnimationPlayback<NorfairLavaJumperSegment>(NorfairLavaJumperAnimations.StartingWith(NorfairLavaJumperSegment.Jump), AnimationPlace.StartOfAnimation), parentState.Playback,
            "real lava-jumper rise handoff starts the jump animation");
        ushort[] jump = NorfairLavaJumperAnimationsConstants.Spritemaps(NorfairLavaJumperSegment.Jump).ToArray();
        int[] holds = [1, 5, 9, 7, 3, 10, 1];
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. jump, jump[^1]], [.. holds, 1]),
            EnemyInstructionRunner.ShownFrames(parent, holds.Sum() + 2), "lava-jumper jump animation");
        AssertTrue(parentState.AnimationFinished, "jump animation publishes its finishing handshake");

        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.Bind(EnemyDefinitionId.Squeept, follower.Definition);
        follower.Bind(follower.DefinitionId, default(RoomEnemyDefinition));
        follower.SetSpawnParameter1(EnemyDefinitionId.Squeept, 32768);
        enemies.NorfairLavaJumpingEnemyBehavior.InitializeNorfairLavaJumpingEnemy(follower);
        NorfairLavaJumpingEnemyState followerState = enemies.NorfairLavaJumpingEnemyStates[1]
            ?? throw new InvalidOperationException("Verification requires enemies.NorfairLavaJumpingEnemyStates[1].");
        AssertEqual(new AnimationPlayback<NorfairLavaJumperSegment>(NorfairLavaJumperAnimations.StartingWith(NorfairLavaJumperSegment.Follower), AnimationPlace.StartOfAnimation), followerState.Playback,
            "real follower initializer starts the follower loop");
        follower.AnimationClock.StepOnNextUpdate();
        ushort[] startup = NorfairLavaJumperAnimationsConstants.Spritemaps(NorfairLavaJumperSegment.Follower).ToArray();
        ushort[] loop = NorfairLavaJumperAnimationsConstants.Spritemaps(NorfairLavaJumperSegment.FollowerLoop).ToArray();
        ushort[] expected = [.. startup, .. loop, startup[0]];
        AssertEqual(EnemyInstructionRunner.ExpectedFrames(expected, expected.Select(_ => 1)),
            EnemyInstructionRunner.ShownFrames(follower, expected.Length), "follower loop runs once and starts over");
        Console.WriteLine("  Norfair lava jumper behavior: hidden, jump with its finishing cue, and the follower loop run as the cartridge does.");
    }

    /// <summary>Runs the lava-jumper behavior checks and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--norfair-lava-jumper-instruction-mechanics")]
    internal static int NorfairLavaJumperBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyNorfairLavaJumperBehavior), () => VerifyNorfairLavaJumperBehavior());
        Suite(nameof(NorfairLavaJumperAnimationsVerification.VerifyNorfairLavaJumperAnimations), () => NorfairLavaJumperAnimationsVerification.VerifyNorfairLavaJumperAnimations(rom));
        return 0;
    }
}
