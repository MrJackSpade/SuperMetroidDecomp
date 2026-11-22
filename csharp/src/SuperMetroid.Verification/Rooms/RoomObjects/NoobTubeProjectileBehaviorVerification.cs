using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
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
    /// <summary>The random sample whose sway-start field is the last of its range: a falling shard's sway starts there.</summary>
    private const int LastSwayStartSample = byte.MaxValue << 8;

    /// <summary>
    /// The tube's block column in the Glass Tunnel, two blocks from the room's left edge, two rows from the room's top so
    /// the fixture camera at the room's corner keeps a shard above its deletion depth.
    /// </summary>
    private static Position16<RoomBlocks> GlassTunnelTubeBlockNearTop =>
        new(RoomObjectPopulationDefinitions.GlassTunnel.Placements.ToArray().Single(placement => placement.Kind == RoomObjectKind.NoobTube).Block.X, 2);

    /// <summary>The Glass Tunnel's blocks: one screen wide and three tall.</summary>
    private static Extent16<RoomBlocks> GlassTunnelSize => new(16, 48);

    /// <summary>
    /// The real burst producers start each crack, shard and bubble on its typed program; each lives as long as its native
    /// list, every shown frame draws as its native spritemap does, each shard flicker picks the pose and column its frame
    /// parity selects, a shard swaying past the room's left edge mirrors past its right edge (#2537), and all sixty-nine
    /// pictures are shown from all ninety of the lists' operands, some of which store a picture again (#2273), with the
    /// lists' bytes forbidden.
    /// </summary>
    internal static void VerifyNoobTubeProjectileBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = rom;
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

        // #2537: the Glass Tunnel's tube block lies two blocks from the room's left edge, and the shard that flies up and
        // out, its fall's sway drawn from the end of its range, sways past that edge as the projectile pass moves it: its
        // column lies at the end of the room's axis, and its mirror image as far past the room's right edge. Two rows from
        // the room's top keep it above the fixture camera's deletion depth for its whole life.
        RoomEnemySystem edgeSystem = NewSystem();
        edgeSystem.VerificationServices().NextRandom = () => LastSwayStartSample;
        NoobTubePiece upOutward = NoobTubeDebrisDefinitions.Shards[Array.IndexOf(shards, NoobTubeShard.UpOutward)];
        EnemyProjectile edgeShard = Spawn(edgeSystem, RoomEnemyProjectileKind.NoobTubeShard, upOutward, GlassTunnelTubeBlockNearTop);
        int tunnelBlocks = GlassTunnelSize.X * GlassTunnelSize.Y;
        var tunnel = new RoomLevelData(GlassTunnelSize,
            new RoomLevelPlanes(new RoomTerrainBlock[tunnelBlocks], new RoomBlockBehavior[tunnelBlocks], new RoomBlockAppearance[tunnelBlocks]),
            RoomLevelData.NothingBeyondGrid(), []);
        SamusState standingSamus = RoomEnemyFrameFixtures.StandaloneSamus();
        bool passedLeftEdge = false;
        bool mirroredPastRightEdge = false;
        for (int step = 1; step <= 320 && edgeShard.IsActive; step++)
        {
            edgeSystem.StepEnemyProjectiles(tunnel, standingSamus, frameCounterByte: step & byte.MaxValue);
            if (!edgeShard.IsActive)
                break;
            int column = edgeShard.NoobTubeDebris.X.Whole;
            passedLeftEdge |= column > CoordinateAxis.LastNonNegative;
            if (edgeShard.Animation is not NoobTubeProjectileAnimation shardAnimation ||
                !shardAnimation.ShowsMirroredFrame)
                continue;
            AssertEqual(unchecked((ushort)(NoobTubeDebrisDefinitions.ShardMirrorSpan - column)), edgeShard.Position.X.Whole,
                $"step {step}: the shard's mirror image lies across the tube from its column {column}");
            mirroredPastRightEdge |= edgeShard.Position.X.Whole > NoobTubeDebrisDefinitions.ShardMirrorSpan;
        }
        AssertTrue(passedLeftEdge, "the up-and-outward shard with the latest sway start sways past the room's left edge");
        AssertTrue(mirroredPastRightEdge, "the up-and-outward shard past the room's left edge mirrors past its right edge");

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
        AssertEqual(69, frames.Length, "the pieces show sixty-nine frames");
        ushort[] operands = [.. frames.Select(EnemyProjectileProgramFrameSources.Operand)
            .SelectMany(own => EnemyProjectileProgramFrameOperands.ShowingAgain(own).Prepend(own))];
        AssertEqual(90, operands.Length, "the pieces' frames are stored at ninety native operands");
        // A flicker names the native operand it shows; a frame shown from an operand that stores its picture again names
        // its own (#2273).
        AssertTrue(executedOperands.IsSubsetOf(operands), "every shown operand is one of the pieces' native operands");
        HashSet<ushort> shown = [.. executedOperands.Select(EnemyProjectileProgramFrameOperands.OwnOperand)];
        AssertEqual(frames.Length, shown.Count, "every frame is shown");
        foreach (EnemyProjectileFrameId frame in frames)
            AssertTrue(shown.Contains(EnemyProjectileProgramFrameSources.Operand(frame)),
                $"production shows n00b-tube presentation $86:{EnemyProjectileProgramFrameSources.Operand(frame):X4}");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the n00b tube's native list bytes");
        Console.WriteLine("  N00b-tube behavior: the crack, all ten shards and six bubbles live their native lifetimes, " +
            "every shard flicker picks its parity's frame, column and visibility, a shard past the room's left edge mirrors past its right edge, " +
            "and all ninety native poses draw as their native spritemaps.");

        RoomEnemySystem NewSystem()
        {
            var system = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            system.VerificationServices().NextRandom = () => 16448;
            return system;
        }

        EnemyProjectile Spawn(RoomEnemySystem system, RoomEnemyProjectileKind kind, NoobTubePiece piece, Position16<RoomBlocks>? block = null)
        {
            system.RoomObjectSpawns.SpawnNoobTubeProjectile(
                new NoobTubeProjectileRequest(piece, RoomObjectBlock: block ?? new(10, 2)));
            return system.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == kind);
        }

        int RunThroughDeletion(RoomEnemySystem system, EnemyProjectile projectile, int maximumTicks)
        {
            for (int tick = 1; tick <= maximumTicks; tick++)
            {
                projectile.FrameHold = WrappingCountdown.Start(1);
                Process(system, projectile, new SuperMetroid.Core.Compatibility.ShortFrameCount(unchecked((byte)tick)));
                if (!projectile.IsActive)
                    return tick;
            }
            throw new InvalidOperationException($"N00b-tube projectile {projectile.Kind} exceeded {maximumTicks} forced ticks.");
        }

        void Process(RoomEnemySystem system, EnemyProjectile projectile, SuperMetroid.Core.Compatibility.ShortFrameCount shortCounter)
        {
            system.Projectiles.AdvanceEnemyProjectileAnimations(projectile, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()), Position16<RoomPixels>.Zero, shortCounter);
            if (!projectile.IsActive)
                return;
            ushort expectedOperand = projectile.PresentationOperandAddress;
            if (projectile.Animation is NoobTubeProjectileAnimation animation)
            {
                // A flicker program's list begins with its flicker command: odd frames show its first operand at the
                // shard's column; even frames show the reflected operand mirrored across the tube, or hide the shard. A
                // shard's flicker is read at the list its flight plays, or its fall's, as the phase playing it is (#2376).
                ushort entry = NoobTubeProjectileNativeListAddresses.Family.ListEntryOf(cartridge, animation.Playback);
                var command = (NativeEnemyProjectileInstruction)cartridge.ReadCartridgeWordFixedBank(NativeEnemyProjectileListDefinitions.ProjectileBank | entry);
                bool oddFrame = (shortCounter & 1) != 0;
                if (command == NativeEnemyProjectileInstruction.NoobTubeShardReflectFlicker)
                {
                    expectedOperand = unchecked((ushort)(entry + (oddFrame ? 2 : 4)));
                    ushort column = projectile.NoobTubeDebris.X.Whole;
                    AssertEqual(oddFrame ? column : unchecked((ushort)(256 - column)), projectile.Position.X.Whole,
                        $"reflected flicker at $86:{entry:X4} column");
                }
                else if (command == NativeEnemyProjectileInstruction.NoobTubeShardFlicker)
                {
                    expectedOperand = unchecked((ushort)(entry + 2));
                    AssertEqual(projectile.NoobTubeDebris.X.Whole, projectile.Position.X.Whole,
                        $"hiding flicker at $86:{entry:X4} column");
                    AssertEqual(!oddFrame, projectile.NoobTubeDebris.IsHidden, $"hiding flicker at $86:{entry:X4} hides the shard on even frames");
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
