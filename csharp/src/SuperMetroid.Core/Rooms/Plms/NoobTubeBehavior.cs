using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The Noob tube species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class NoobTubeBehavior
{
    /// <summary>The program each native even shard spawn parameter ($00-$12) selects, by parameter / 2.</summary>
    private static readonly NoobTubeProjectileProgram[] ShardPrograms =
    [
        NoobTubeProjectileProgram.Shard0, NoobTubeProjectileProgram.Shard1, NoobTubeProjectileProgram.Shard2,
        NoobTubeProjectileProgram.Shard3, NoobTubeProjectileProgram.Shard4, NoobTubeProjectileProgram.Shard5,
        NoobTubeProjectileProgram.Shard6, NoobTubeProjectileProgram.Shard7, NoobTubeProjectileProgram.Shard8,
        NoobTubeProjectileProgram.Shard9,
    ];

    /// <summary>Provides room-load state and the native RNG for shard and bubble launch angles.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates and advances crack, glass shard, and released-air-bubble projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds projectile initialization to the loaded room and its projectile slots.</summary>
    internal NoobTubeBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Executes the initializer selected by definitions $D904/$D912/$D920. The request
    /// retains the executing PLM block because native bank $86 reads the global PLM index
    /// while constructing each actor.
    /// </summary>
    public void SpawnNoobTubeProjectile(
        NoobTubeProjectileRequest request,
        int roomWidthInBlocks)
    {
        _room.EnsureLoaded();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        if (request.PlmBlockIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(request));

        RoomEnemyProjectileKind kind = request.DefinitionPointer switch
        {
            NoobTubeProjectileDefinition.Crack => RoomEnemyProjectileKind.NoobTubeCrack,
            NoobTubeProjectileDefinition.Shard => RoomEnemyProjectileKind.NoobTubeShard,
            NoobTubeProjectileDefinition.ReleasedAirBubble =>
                RoomEnemyProjectileKind.NoobTubeReleasedAirBubble,
            _ => throw new InvalidOperationException(
                $"Undefined n00b-tube projectile definition {request.DefinitionPointer}."),
        };

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: 0);

        int blockX = request.PlmBlockIndex % roomWidthInBlocks;
        int blockY = request.PlmBlockIndex / roomWidthInBlocks;
        switch (kind)
        {
            case RoomEnemyProjectileKind.NoobTubeCrack:
                if (request.Parameter != 0)
                    throw new InvalidDataException("N00b-tube crack parameter must be zero.");
                projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(
                    blockX * 16 + NoobTubeProjectileRomData.CrackXOffset)), unchecked((ushort)(
                    blockY * 16 + NoobTubeProjectileRomData.CrackYOffset))));
                return;

            case RoomEnemyProjectileKind.NoobTubeShard:
            {
                int index = ValidateEvenNoobTubeParameter(
                    request.Parameter,
                    NoobTubeProjectileRomData.ShardOffsets.Length,
                    "shard");
                projectile.Variable1 = unchecked((ushort)(
                    blockX * 16 + NoobTubeProjectileRomData.CrackXOffset +
                    NoobTubeProjectileRomData.ShardOffsets[index].X));
                projectile.Variable0 = 0;
                projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                    blockY * 16 + NoobTubeProjectileRomData.CrackYOffset +
                    NoobTubeProjectileRomData.ShardOffsets[index].Y)));
                projectile.StartAnimation(new NoobTubeProjectileAnimation(ShardPrograms[index]));
                projectile.Velocity = NoobTubeProjectileRomData.ShardVelocities[index];
                return;
            }

            case RoomEnemyProjectileKind.NoobTubeReleasedAirBubble:
            {
                int index = ValidateEvenNoobTubeParameter(
                    request.Parameter,
                    NoobTubeProjectileRomData.BubbleOffsets.Length,
                    "released-air bubble");
                projectile.Variable1 = unchecked((ushort)(
                    blockX * 16 + NoobTubeProjectileRomData.BubbleOffsets[index].X));
                projectile.Variable0 = 0;
                projectile.Position = projectile.Position.WithWholeY(unchecked((ushort)(
                    blockY * 16 + NoobTubeProjectileRomData.BubbleOffsets[index].Y)));
                projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(NoobTubeProjectileRomData.BubbleInitialYVelocity));
                return;
            }

            default:
                throw new InvalidDataException(
                    $"N00b-tube initializer lost projectile kind {kind}.");
        }
    }

    /// <summary>Converts a native even-byte table offset to a shard or bubble entry index, rejecting odd and out-of-range operands.</summary>
    private static int ValidateEvenNoobTubeParameter(
        int parameter,
        int entryCount,
        string family)
    {
        if ((parameter & 1) != 0 || parameter / 2 >= entryCount)
        {
            throw new InvalidDataException(
                $"N00b-tube {family} parameter ${parameter:X4} is outside its even table.");
        }
        return parameter / 2;
    }

    /// <summary>Runs the six crack, shard, and bubble callbacks, including flicker timing and off-screen shard deletion.</summary>
    internal void RunNoobTubeProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraY)
    {
        EnemyProjectilePreInstruction preInstruction = projectile.PreInstruction;
        switch (preInstruction)
        {
            case EnemyProjectilePreInstruction.NoobTubeCrackFlickering:
                if (projectile.Position.X.Whole != NoobTubeProjectileRomData.HiddenXPosition)
                    projectile.Variable0 = projectile.Position.X.Whole;
                projectile.Position = projectile.Position.WithWholeX((_projectiles.CurrentFrame8 & 1) != 0
                    ? NoobTubeProjectileRomData.HiddenXPosition
                    : checked((ushort)projectile.Variable0));
                return;

            case EnemyProjectilePreInstruction.NoobTubeCrackFalling:
                projectile.Position = projectile.Position.AdvanceY(new SnesSignedEightEight(NoobTubeProjectileRomData.CrackFallVelocity).ToSixteenSixteenDelta());
                return;

            case EnemyProjectilePreInstruction.NoobTubeShardFlying:
                (projectile.Variable1, projectile.Variable0) = EnemyMath.AddEightBitVelocity(
                    checked((ushort)projectile.Variable1),
                    checked((ushort)projectile.Variable0),
                    projectile.Velocity.X.RawValue);
                projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
                EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
                return;

            case EnemyProjectilePreInstruction.NoobTubeShardFalling:
                MoveNoobTubeProjectileHorizontallyAlongArc(projectile, angleStep: 2);
                projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
                EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
                return;

            case EnemyProjectilePreInstruction.NoobTubeBubbleFalling:
                // `$86:D89F` performs only the curved horizontal update before falling
                // through to `$D8DF`, which applies the bubble's vertical velocity once.
                MoveNoobTubeProjectileHorizontallyAlongArc(projectile, angleStep: 4);
                MoveNoobTubeBubbleVertically(projectile);
                return;

            case EnemyProjectilePreInstruction.NoobTubeBubbleFlying:
                MoveNoobTubeBubbleVertically(projectile);
                return;

            default:
                throw new InvalidOperationException(
                    $"Undefined n00b-tube projectile pre-instruction {preInstruction}.");
        }
    }

    /// <summary>Uses the native sine table to advance the projectile's separate horizontal word and fraction, then steps its angle.</summary>
    internal static void MoveNoobTubeProjectileHorizontallyAlongArc(
        RoomEnemyProjectileSlot projectile,
        ushort angleStep)
    {
        // The native even table offset ORs in a quarter turn: the angle's bit 6.
        short sine = EnemyTrigonometryTables.SignedSine(unchecked((byte)((projectile.Velocity.X.RawValue >> 1) | 64)));
        ushort horizontalVelocity = unchecked((ushort)(sine >> 2));
        (projectile.Variable1, projectile.Variable0) = EnemyMath.AddEightBitVelocity(
            checked((ushort)projectile.Variable1),
            checked((ushort)projectile.Variable0),
            horizontalVelocity);
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.X.RawValue + angleStep))));
    }

    /// <summary>Advances the bubble by its vertical velocity and publishes its separately accumulated horizontal word.</summary>
    private static void MoveNoobTubeBubbleVertically(RoomEnemyProjectileSlot projectile)
    {
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta()).WithWholeX(checked((ushort)projectile.Variable1));
    }

    /// <summary>Connects all six native projectile pre-instruction identities to the shared room callback.</summary>
    internal void RegisterNoobTubeProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunNoobTubeProjectilePreInstruction(context.Projectile, context.Camera.Y);
        }, EnemyProjectilePreInstruction.NoobTubeCrackFlickering, EnemyProjectilePreInstruction.NoobTubeCrackFalling, EnemyProjectilePreInstruction.NoobTubeShardFlying, EnemyProjectilePreInstruction.NoobTubeShardFalling, EnemyProjectilePreInstruction.NoobTubeBubbleFalling, EnemyProjectilePreInstruction.NoobTubeBubbleFlying);
    }

    /// <summary>Carries out the cues of the crack, shard and bubble programs.</summary>
    internal void RegisterNoobTubeProjectileCues(EnemyProjectileHandlerTable table) =>
        table.RegisterCue<NoobTubeProjectileCue>((context, cue) => ApplyNoobTubeProjectileCue(context.Projectile, cue));

    /// <summary>Carries out one cue of the crack, shard and bubble programs.</summary>
    private void ApplyNoobTubeProjectileCue(RoomEnemyProjectileSlot projectile, NoobTubeProjectileCue cue)
    {
        bool oddFrame = (_projectiles.CurrentFrame8 & 1) != 0;
        switch (cue.Kind)
        {
            case NoobTubeProjectileCueKind.FlickerReflected:
                // Odd frames show the first pose at the shard's column; even frames the reflected pose, mirrored across the tube.
                projectile.Position = projectile.Position.WithWholeX(oddFrame
                    ? checked((ushort)projectile.Variable1)
                    : unchecked((ushort)(NoobTubeProjectileRomData.ShardMirrorSpan - projectile.Variable1)));
                ShardAnimation(projectile).ShowsReflectedPose = !oddFrame;
                break;
            case NoobTubeProjectileCueKind.FlickerHidden:
                projectile.Position = projectile.Position.WithWholeX(oddFrame ? checked((ushort)projectile.Variable1) : NoobTubeProjectileRomData.HiddenXPosition);
                break;
            case NoobTubeProjectileCueKind.AssignShardFallingAngle:
                projectile.Velocity = new(new SnesSignedEightEight(SnesLittleEndian.HighByte(_room.Random.Generate())), new SnesSignedEightEight(NoobTubeProjectileRomData.ShardFallYVelocity));
                break;
            case NoobTubeProjectileCueKind.AssignBubbleFallingAngle:
                projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(SnesLittleEndian.HighByte(_room.Random.Generate())));
                break;
            case NoobTubeProjectileCueKind.SetPreInstruction:
                projectile.PreInstruction = cue.PreInstruction;
                break;
            case NoobTubeProjectileCueKind.Delete:
                projectile.Clear();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(NoobTubeProjectileCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>The typed animation a shard's flicker cue runs in.</summary>
    private static NoobTubeProjectileAnimation ShardAnimation(RoomEnemyProjectileSlot projectile) =>
        projectile.Animation as NoobTubeProjectileAnimation
        ?? throw new InvalidOperationException($"A n00b-tube flicker cue ran outside its typed program; the slot runs {projectile.Animation}.");
}
