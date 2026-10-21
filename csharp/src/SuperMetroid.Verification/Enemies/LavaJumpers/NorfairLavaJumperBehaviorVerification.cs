using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.LavaJumpers.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
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
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        RoomEnemySlot parent = enemies.SlotTable.Slots[0];
        parent.Bind(EnemyDefinitionId.Squeept, parent.Definition);
        parent.Bind(parent.DefinitionId, default(RoomEnemyDefinition) with { Bank = NorfairLavaJumperFrameToolingDefinitions.Bank });
        parent.Position = parent.Position.WithWhole(new(256, 480));
        enemies.NorfairLavaJumpingEnemyBehavior.InitializeNorfairLavaJumpingEnemy(parent);
        NorfairLavaJumpingEnemyState parentState = enemies.NorfairLavaJumpingEnemyStates[0]
            ?? throw new InvalidOperationException("Verification requires enemies.NorfairLavaJumpingEnemyStates[0].");
        AssertEqual(AnimationProgramCursor<NorfairLavaJumperProgram>.Start(NorfairLavaJumperProgram.Hidden), parentState.Program,
            "lava-jumper parent starts hidden");
        // The initializer leaves the population's instruction timer; start the hidden pose on the next update.
        parent.InstructionTimer = WrappingCountdown.Start(1);
        ushort hidden = NorfairLavaJumperAnimationsConstants.Spritemaps(NorfairLavaJumperProgram.Hidden).Single();
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([hidden, hidden], [1, 1]), EnemyInstructionRunner.ShownFrames(parent, 3),
            "lava-jumper hidden pose");

        enemies.NorfairLavaJumpingEnemyBehavior.RunNorfairLavaJumpingEnemyMain(parent, parentState);
        parentState.YVelocity = SnesSignedEightEight.FromSignedRaw(-1080); // Past the native switch velocity, still rising.
        enemies.NorfairLavaJumpingEnemyBehavior.RunNorfairLavaJumpingEnemyMain(parent, parentState);
        AssertEqual(AnimationProgramCursor<NorfairLavaJumperProgram>.Start(NorfairLavaJumperProgram.Jump), parentState.Program,
            "real lava-jumper rise handoff starts the jump animation");
        ushort[] jump = NorfairLavaJumperAnimationsConstants.Spritemaps(NorfairLavaJumperProgram.Jump).ToArray();
        int[] holds = [1, 5, 9, 7, 3, 10, 1];
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. jump, jump[^1]], [.. holds, 1]),
            EnemyInstructionRunner.ShownFrames(parent, holds.Sum() + 2), "lava-jumper jump animation");
        AssertTrue(parentState.AnimationFinished, "jump animation publishes its finishing handshake");

        RoomEnemySlot follower = enemies.SlotTable.Slots[1];
        follower.Bind(EnemyDefinitionId.Squeept, follower.Definition);
        follower.Bind(follower.DefinitionId, default(RoomEnemyDefinition) with { Bank = NorfairLavaJumperFrameToolingDefinitions.Bank });
        follower.SetSpawnParameter1(32768);
        enemies.NorfairLavaJumpingEnemyBehavior.InitializeNorfairLavaJumpingEnemy(follower);
        NorfairLavaJumpingEnemyState followerState = enemies.NorfairLavaJumpingEnemyStates[1]
            ?? throw new InvalidOperationException("Verification requires enemies.NorfairLavaJumpingEnemyStates[1].");
        AssertEqual(AnimationProgramCursor<NorfairLavaJumperProgram>.Start(NorfairLavaJumperProgram.Follower), followerState.Program,
            "real follower initializer starts the follower loop");
        follower.InstructionTimer = WrappingCountdown.Start(1);
        ushort[] startup = NorfairLavaJumperAnimationsConstants.Spritemaps(NorfairLavaJumperProgram.Follower).ToArray();
        ushort[] loop = NorfairLavaJumperAnimationsConstants.Spritemaps(NorfairLavaJumperProgram.FollowerLoop).ToArray();
        ushort[] expected = [.. startup, .. loop, startup[0]];
        AssertEqual(EnemyInstructionRunner.ExpectedFrames(expected, expected.Select(_ => 1)),
            EnemyInstructionRunner.ShownFrames(follower, expected.Length), "follower loop runs once and starts over");
        Console.WriteLine("  Norfair lava jumper behavior: hidden, jump with its finishing cue, and the follower loop run as the cartridge does.");
    }

    /// <summary>Runs the lava-jumper behavior checks and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--norfair-lava-jumper-instruction-mechanics")]
    internal static int NorfairLavaJumperBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyNorfairLavaJumperBehavior), () => VerifyNorfairLavaJumperBehavior());
        Suite(nameof(NorfairLavaJumperAnimationsVerification.VerifyNorfairLavaJumperAnimations), () => NorfairLavaJumperAnimationsVerification.VerifyNorfairLavaJumperAnimations(rom));
        return 0;
    }
}
