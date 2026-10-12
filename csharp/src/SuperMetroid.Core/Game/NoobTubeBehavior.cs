using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Noob tube species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class NoobTubeBehavior
{
    /// <summary>Stores room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Stores projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Initializes a new noob tube behavior instance.</summary>
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
                projectile.InstructionPointer =
                    NoobTubeProjectileRomData.ShardInstructionLists[index];
                projectile.InstructionTimer = 1;
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

    /// <summary>Validates even noob tube parameter.</summary>
    private static int ValidateEvenNoobTubeParameter(
        ushort parameter,
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

    /// <summary>Runs the six movement callbacks referenced by the three projectile lists.</summary>
    internal void RunNoobTubeProjectilePreInstruction(
        RoomEnemyProjectileSlot projectile,
        ushort cameraY)
    {
        NoobTubeProjectilePreInstruction preInstruction =
            ClosedNativeWords.Decode<NoobTubeProjectilePreInstruction>(
                projectile.PreInstruction, "n00b-tube projectile pre-instruction");
        switch (preInstruction)
        {
            case NoobTubeProjectilePreInstruction.CrackFlickering:
                if (projectile.Position.X.Whole != NoobTubeProjectileRomData.HiddenXPosition)
                    projectile.Variable0 = projectile.Position.X.Whole;
                projectile.Position = projectile.Position.WithWholeX((_projectiles.CurrentFrame8 & 1) != 0
                    ? NoobTubeProjectileRomData.HiddenXPosition
                    : projectile.Variable0);
                return;

            case NoobTubeProjectilePreInstruction.CrackFalling:
                projectile.Position = projectile.Position.AdvanceY(new SnesSignedEightEight(NoobTubeProjectileRomData.CrackFallVelocity).ToSixteenSixteenDelta());
                return;

            case NoobTubeProjectilePreInstruction.ShardFlying:
                (projectile.Variable1, projectile.Variable0) = EnemyMath.AddEightBitVelocity(
                    projectile.Variable1,
                    projectile.Variable0,
                    projectile.Velocity.X.RawValue);
                projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
                EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
                return;

            case NoobTubeProjectilePreInstruction.ShardFalling:
                MoveNoobTubeProjectileHorizontallyAlongArc(projectile, angleStep: 2);
                projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
                EnemyProjectileEngine.DeleteEnemyProjectileIfVerticallyOffScreen(projectile, cameraY);
                return;

            case NoobTubeProjectilePreInstruction.BubbleFalling:
                // `$86:D89F` performs only the curved horizontal update before falling
                // through to `$D8DF`, which applies the bubble's vertical velocity once.
                MoveNoobTubeProjectileHorizontallyAlongArc(projectile, angleStep: 4);
                MoveNoobTubeBubbleVertically(projectile);
                return;

            case NoobTubeProjectilePreInstruction.BubbleFlying:
                MoveNoobTubeBubbleVertically(projectile);
                return;

            default:
                throw new InvalidOperationException(
                    $"Undefined n00b-tube projectile pre-instruction {preInstruction}.");
        }
    }

    /// <summary>Moves noob tube projectile horizontally along arc.</summary>
    internal static void MoveNoobTubeProjectileHorizontallyAlongArc(
        RoomEnemyProjectileSlot projectile,
        ushort angleStep)
    {
        ushort tableOffset = unchecked((ushort)((projectile.Velocity.X.RawValue & 0x01fe) | 0x0080));
        short sine = EnemyTrigonometryTables.SignedSine((byte)(tableOffset >> 1));
        ushort horizontalVelocity = unchecked((ushort)(sine >> 2));
        (projectile.Variable1, projectile.Variable0) = EnemyMath.AddEightBitVelocity(
            projectile.Variable1,
            projectile.Variable0,
            horizontalVelocity);
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.X.RawValue + angleStep))));
    }

    /// <summary>Moves noob tube bubble vertically.</summary>
    private static void MoveNoobTubeBubbleVertically(RoomEnemyProjectileSlot projectile)
    {
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta()).WithWholeX(projectile.Variable1);
    }

    /// <summary>Register noob tube projectiles.</summary>
    internal void RegisterNoobTubeProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunNoobTubeProjectilePreInstruction(context.Projectile, context.Camera.Y);
        }, EnemyProjectilePreInstruction.NoobTubeCrackFlickering, EnemyProjectilePreInstruction.NoobTubeCrackFalling, EnemyProjectilePreInstruction.NoobTubeShardFlying, EnemyProjectilePreInstruction.NoobTubeShardFalling, EnemyProjectilePreInstruction.NoobTubeBubbleFalling, EnemyProjectilePreInstruction.NoobTubeBubbleFlying);
    }


    /// <summary>Register falling angle instructions.</summary>
    internal void RegisterFallingAngleInstructions(EnemyProjectileHandlerTable table)
    {
        table.RegisterInstruction(EnemyProjectileInstruction.NoobTubeShardAssignFallingAngle, null,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                projectile.Velocity = new(new SnesSignedEightEight(SnesLittleEndian.HighByte(_room.Random.Generate())), new SnesSignedEightEight(NoobTubeProjectileRomData.ShardFallYVelocity));
                cursor = unchecked((ushort)(cursor + 2));
                return EnemyProjectileInstructionOutcome.Continue;
            });

        table.RegisterInstruction(EnemyProjectileInstruction.NoobTubeBubbleAssignFallingAngle, null,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(SnesLittleEndian.HighByte(_room.Random.Generate())));
                cursor = unchecked((ushort)(cursor + 2));
                return EnemyProjectileInstructionOutcome.Continue;
            });
    }
}
