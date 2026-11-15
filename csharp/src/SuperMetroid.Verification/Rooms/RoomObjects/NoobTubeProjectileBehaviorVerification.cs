using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.NoobTube;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>#1292: the n00b tube's crack, glass shards and released-air bubbles run typed programs through their lifetimes.</summary>
internal static class NoobTubeProjectileBehaviorVerification
{
    /// <summary>
    /// The real burst producers start each crack, shard and bubble on its typed program; each lives as long as its native
    /// list, every shown frame draws as its native spritemap does, each shard flicker picks the pose and column its frame
    /// parity selects, and all ninety poses are shown with the lists' bytes forbidden.
    /// </summary>
    internal static void VerifyNoobTubeProjectileBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var spriteArtwork = InstalledFixtures.EnemyTiles.ProjectileSpritemaps
            ?? throw new InvalidDataException("Projectile fixture requires installed sprites.");
        var executedOperands = new HashSet<ushort>();
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, NoobTubeProjectileNativeListAddresses.Extent, "n00b tube projectile");

        RoomEnemySystem crackSystem = NewSystem();
        EnemyProjectile crack = Spawn(crackSystem, RoomEnemyProjectileKind.NoobTubeCrack, NoobTubeDebrisDefinitions.Crack);
        AssertTrue(crack.Animation is NoobTubeProjectileAnimation { PlayingSegment: NoobTubeProjectileSegment.Crack },
            "real n00b-tube crack producer starts the crack program");
        AssertEqual(48, RunThroughDeletion(crackSystem, crack, maximumTicks: 64),
            "n00b-tube crack completes 47 frames and its terminal deletion tick");

        // The shards burst in the order their identities are declared (#2044).
        NoobTubeShard[] shards = Enum.GetValues<NoobTubeShard>();
        AssertEqual(shards.Length, NoobTubeDebrisDefinitions.Shards.Length, "the tube bursts into each shard once");
        for (int piece = 0; piece < shards.Length; piece++)
        {
            RoomEnemySystem shardSystem = NewSystem();
            EnemyProjectile shard = Spawn(shardSystem, RoomEnemyProjectileKind.NoobTubeShard, NoobTubeDebrisDefinitions.Shards[piece]);
            AssertTrue(shard.Animation is NoobTubeProjectileAnimation animation
                    && ReferenceEquals(animation.Playback.Animation, NoobTubeProjectileAnimations.ShardAnimations[shards[piece]]),
                $"real n00b-tube shard producer starts shard {shards[piece]}'s animation");
            AssertEqual(305, RunThroughDeletion(shardSystem, shard, maximumTicks: 320),
                $"n00b-tube shard {piece} completes both counted flicker phases");
        }

        for (int piece = 0; piece < NoobTubeDebrisDefinitions.Bubbles.Length; piece++)
        {
            RoomEnemySystem bubbleSystem = NewSystem();
            EnemyProjectile bubble = Spawn(bubbleSystem, RoomEnemyProjectileKind.NoobTubeReleasedAirBubble, NoobTubeDebrisDefinitions.Bubbles[piece]);
            AssertTrue(bubble.Animation is NoobTubeProjectileAnimation { PlayingSegment: NoobTubeProjectileSegment.ReleasedAirBubble },
                $"real released-air-bubble producer for bubble {piece} starts the bubble program");
            AssertEqual(16, RunThroughDeletion(bubbleSystem, bubble, maximumTicks: 24),
                $"released-air bubble {piece} completes fifteen frames and deletion");
        }

        EnemyProjectileFrameId[] frames = [.. NoobTubeProjectileAnimations.ShownFrames];
        AssertEqual(90, frames.Length, "the pieces show ninety frames");
        AssertEqual(frames.Length, executedOperands.Count, "every native visual operand is shown");
        foreach (EnemyProjectileFrameId frame in frames)
        {
            ushort key = EnemyProjectileProgramFrameSources.Operand(frame);
            AssertTrue(executedOperands.Contains(key), $"production shows n00b-tube presentation $86:{key:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the n00b tube's native list bytes");
        Console.WriteLine("  N00b-tube behavior: the crack, all ten shards and six bubbles live their native lifetimes, " +
            "every shard flicker picks its parity's frame, column and visibility, and all ninety poses draw as their native spritemaps.");

        RoomEnemySystem NewSystem()
        {
            var system = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            system.VerificationServices().NextRandom = () => 16448;
            return system;
        }

        EnemyProjectile Spawn(RoomEnemySystem system, RoomEnemyProjectileKind kind, NoobTubePiece piece)
        {
            system.NoobTubeBehavior.SpawnNoobTubeProjectile(
                new NoobTubeProjectileRequest(piece, RoomObjectBlock: new(10, 2)));
            return system.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == kind);
        }

        int RunThroughDeletion(RoomEnemySystem system, EnemyProjectile projectile, int maximumTicks)
        {
            for (int tick = 1; tick <= maximumTicks; tick++)
            {
                projectile.FrameHold = WrappingCountdown.Start(1);
                Process(system, projectile, unchecked((byte)tick));
                if (!projectile.IsActive)
                    return tick;
            }
            throw new InvalidOperationException($"N00b-tube projectile {projectile.Kind} exceeded {maximumTicks} forced ticks.");
        }

        void Process(RoomEnemySystem system, EnemyProjectile projectile, int shortCounter)
        {
            system.Projectiles.AdvanceEnemyProjectileAnimations(projectile, new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), new SuperMetroid.Core.Samus.Artwork.SamusArtworkBinding()), Position16<RoomPixels>.Zero, shortCounter);
            if (!projectile.IsActive)
                return;
            ushort expectedOperand = projectile.PresentationOperandAddress;
            if (projectile.Animation is NoobTubeProjectileAnimation animation)
            {
                // A flicker program's list begins with its flicker command: odd frames show its first operand at the
                // shard's column; even frames show the reflected operand mirrored across the tube, or hide the shard.
                ushort entry = NoobTubeProjectileNativeListAddresses.Entry(animation.PlayingSegment);
                var command = (NativeEnemyProjectileInstruction)cartridge.ReadCartridgeWordFixedBank(NativeEnemyProjectileListDefinitions.ProjectileBank | entry);
                bool oddFrame = (shortCounter & 1) != 0;
                if (command == NativeEnemyProjectileInstruction.NoobTubeShardReflectFlicker)
                {
                    expectedOperand = unchecked((ushort)(entry + (oddFrame ? 2 : 4)));
                    ushort column = projectile.StateOf<NoobTubeDebrisState>().X.Whole;
                    AssertEqual(oddFrame ? column : unchecked((ushort)(256 - column)), projectile.Position.X.Whole,
                        $"reflected flicker at $86:{entry:X4} column");
                }
                else if (command == NativeEnemyProjectileInstruction.NoobTubeShardFlicker)
                {
                    expectedOperand = unchecked((ushort)(entry + 2));
                    AssertEqual(projectile.StateOf<NoobTubeDebrisState>().X.Whole, projectile.Position.X.Whole,
                        $"hiding flicker at $86:{entry:X4} column");
                    AssertEqual(!oddFrame, projectile.IsHidden, $"hiding flicker at $86:{entry:X4} hides the shard on even frames");
                }
            }
            VerifyExecutedProjectileFrame(rom, projectile, spriteArtwork, executedOperands, expectedOperand);
        }
    }

    /// <summary>Runs the n00b tube's program oracle and behavior suite.</summary>
    [VerificationCommand("--noob-tube-projectile-programs")]
    internal static int NoobTubeProjectileProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(NoobTubeProjectileAnimationsVerification.VerifyNoobTubeProjectileAnimations), () => NoobTubeProjectileAnimationsVerification.VerifyNoobTubeProjectileAnimations(rom));
        Suite(nameof(VerifyNoobTubeProjectileBehavior), () => VerifyNoobTubeProjectileBehavior(rom));
        return 0;
    }
}
