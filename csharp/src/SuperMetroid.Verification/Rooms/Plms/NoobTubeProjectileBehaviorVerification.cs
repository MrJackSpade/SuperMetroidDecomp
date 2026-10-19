using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Plms.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>#1292: the n00b tube's crack, glass shards and released-air bubbles run typed programs through their lifetimes.</summary>
internal static class NoobTubeProjectileBehaviorVerification
{
    /// <summary>
    /// The real burst producers start each crack, shard and bubble on its typed program; each lives as long as its native
    /// list, every shown frame draws as its native spritemap does, each shard flicker picks the pose and column its frame
    /// parity selects, and all ninety poses are shown with the lists' bytes forbidden.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the n00b tube's flicker commands")]
    internal static void VerifyNoobTubeProjectileBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, NoobTubeProjectileNativeListAddresses.Extent, "n00b tube projectile");

        RoomEnemySystem crackSystem = NewSystem();
        RoomEnemyProjectileSlot crack = Spawn(crackSystem, RoomEnemyProjectileKind.NoobTubeCrack, parameter: 0);
        AssertTrue(crack.Animation is NoobTubeProjectileAnimation { Cursor.Program: NoobTubeProjectileProgram.Crack },
            "real n00b-tube crack producer starts the crack program");
        AssertEqual(48, RunThroughDeletion(crackSystem, crack, maximumTicks: 64),
            "n00b-tube crack completes 47 frames and its terminal deletion tick");

        NoobTubeProjectileProgram[] shards =
        [
            NoobTubeProjectileProgram.Shard0, NoobTubeProjectileProgram.Shard1, NoobTubeProjectileProgram.Shard2,
            NoobTubeProjectileProgram.Shard3, NoobTubeProjectileProgram.Shard4, NoobTubeProjectileProgram.Shard5,
            NoobTubeProjectileProgram.Shard6, NoobTubeProjectileProgram.Shard7, NoobTubeProjectileProgram.Shard8,
            NoobTubeProjectileProgram.Shard9,
        ];
        for (ushort parameter = 0; parameter <= 18; parameter += 2)
        {
            RoomEnemySystem shardSystem = NewSystem();
            RoomEnemyProjectileSlot shard = Spawn(shardSystem, RoomEnemyProjectileKind.NoobTubeShard, parameter);
            AssertTrue(shard.Animation is NoobTubeProjectileAnimation animation && animation.Cursor.Program == shards[parameter / 2],
                $"real n00b-tube shard producer selects parameter ${parameter:X2} program");
            AssertEqual(305, RunThroughDeletion(shardSystem, shard, maximumTicks: 320),
                $"n00b-tube shard ${parameter:X2} completes both counted flicker phases");
        }

        for (ushort parameter = 0; parameter <= 10; parameter += 2)
        {
            RoomEnemySystem bubbleSystem = NewSystem();
            RoomEnemyProjectileSlot bubble = Spawn(bubbleSystem, RoomEnemyProjectileKind.NoobTubeReleasedAirBubble, parameter);
            AssertTrue(bubble.Animation is NoobTubeProjectileAnimation { Cursor.Program: NoobTubeProjectileProgram.ReleasedAirBubble },
                $"real released-air-bubble producer parameter ${parameter:X2} starts the bubble program");
            AssertEqual(16, RunThroughDeletion(bubbleSystem, bubble, maximumTicks: 24),
                $"released-air bubble ${parameter:X2} completes fifteen frames and deletion");
        }

        AssertEqual(NoobTubeProjectileFrameDefinitions.PoseCount, executedOperands.Count, "every native visual operand is shown");
        for (int pose = 0; pose < NoobTubeProjectileFrameDefinitions.PoseCount; pose++)
        {
            ushort key = EnemyProjectileFrames.ProgramFrameKey(NoobTubeProjectileFrameDefinitions.ShownProjectileFrames[new(pose)]);
            AssertTrue(executedOperands.Contains(key), $"production shows n00b-tube presentation $86:{key:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the n00b tube's native list bytes");
        Console.WriteLine("  N00b-tube behavior: the crack, all ten shards and six bubbles live their native lifetimes, " +
            "every shard flicker picks its parity's pose and column, and all ninety poses draw as their native spritemaps.");

        RoomEnemySystem NewSystem()
        {
            var system = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            system.VerificationServices().NextRandom = (Func<ushort>)(() => 16448);
            return system;
        }

        RoomEnemyProjectileSlot Spawn(RoomEnemySystem system, RoomEnemyProjectileKind kind, ushort parameter)
        {
            NoobTubeProjectileDefinition definition = kind switch
            {
                RoomEnemyProjectileKind.NoobTubeCrack => NoobTubeProjectileDefinition.Crack,
                RoomEnemyProjectileKind.NoobTubeShard => NoobTubeProjectileDefinition.Shard,
                RoomEnemyProjectileKind.NoobTubeReleasedAirBubble => NoobTubeProjectileDefinition.ReleasedAirBubble,
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            };
            system.NoobTubeBehavior.SpawnNoobTubeProjectile(
                new NoobTubeProjectileRequest(definition, parameter, PlmBlockIndex: 42),
                roomWidthInBlocks: 16);
            return system.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == kind);
        }

        int RunThroughDeletion(RoomEnemySystem system, RoomEnemyProjectileSlot projectile, int maximumTicks)
        {
            for (int tick = 1; tick <= maximumTicks; tick++)
            {
                system.Projectiles.CurrentFrame8 = unchecked((byte)tick);
                projectile.InstructionTimer = NativeCountdown.Start(1);
                Process(system, projectile);
                if (!projectile.IsActive)
                    return tick;
            }
            throw new InvalidOperationException($"N00b-tube projectile {projectile.Kind} exceeded {maximumTicks} forced ticks.");
        }

        void Process(RoomEnemySystem system, RoomEnemyProjectileSlot projectile)
        {
            system.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
            if (!projectile.IsActive)
                return;
            ushort expectedOperand = projectile.PresentationOperandAddress;
            if (projectile.Animation is NoobTubeProjectileAnimation animation)
            {
                // A flicker program's list begins with its flicker command: odd frames show its first operand at the
                // shard's column; even frames show the reflected operand mirrored across the tube, or hide the shard.
                ushort entry = NoobTubeProjectileNativeListAddresses.Entry(animation.Cursor.Program);
                var command = (NativeEnemyProjectileInstruction)cartridge.ReadCartridgeWordFixedBank(NativeEnemyProjectileListDefinitions.ProjectileBank | entry);
                bool oddFrame = (system.Projectiles.CurrentFrame8 & 1) != 0;
                if (command == NativeEnemyProjectileInstruction.NoobTubeShardReflectFlicker)
                {
                    expectedOperand = unchecked((ushort)(entry + (oddFrame ? 2 : 4)));
                    AssertEqual(oddFrame ? projectile.Variable1 : unchecked((ushort)(256 - projectile.Variable1)), projectile.Position.X.Whole,
                        $"reflected flicker at $86:{entry:X4} column");
                }
                else if (command == NativeEnemyProjectileInstruction.NoobTubeShardFlicker)
                {
                    expectedOperand = unchecked((ushort)(entry + 2));
                    AssertEqual(oddFrame ? projectile.Variable1 : NoobTubeProjectileRomData.HiddenXPosition, projectile.Position.X.Whole,
                        $"hiding flicker at $86:{entry:X4} column");
                }
            }
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands, expectedOperand);
        }
    }

    /// <summary>Runs the n00b tube's program oracle and behavior suite.</summary>
    [VerificationCommand("--noob-tube-projectile-programs")]
    internal static int NoobTubeProjectileProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(NoobTubeProjectileAnimationsVerification.VerifyNoobTubeProjectileAnimations), () => NoobTubeProjectileAnimationsVerification.VerifyNoobTubeProjectileAnimations(rom));
        Suite(nameof(VerifyNoobTubeProjectileBehavior), () => VerifyNoobTubeProjectileBehavior(rom));
        return 0;
    }
}
