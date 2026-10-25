using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>The Cacatac species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class CacatacBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Cacatac];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new CacatacEntity(this, slot, common);

    /// <summary>Supplies shared room RNG for the three-in-256 spike attack gate.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates spike projectiles with source position and OBJ attributes.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds Cacatac patrol and spike behavior to room RNG and projectile allocation.</summary>
    internal CacatacBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>Carries out one cue of Cacatac's animations.</summary>
    private CacatacProgram? ApplyCacatacCue(RoomEnemySlot slot, CacatacEnemyState state, CacatacCue cue)
    {
        switch (cue.Kind)
        {
            case CacatacCueKind.RestorePatrol:
                RestoreCacatacPatrol(state);
                break;
            case CacatacCueKind.PlaySpikeSound:
                PlayCacatacSpikeSound();
                break;
            case CacatacCueKind.SpawnSpike:
                SpawnCacatacSpike(slot, cue.Direction);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CacatacCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Last library-two spike-release sound requested in this enemy frame.</summary>
    public SoundEffectId? LastCacatacSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Cacatac</c> at $A2:9F48.</summary>
    internal static void InitializeCacatac(RoomEnemySlot slot)
    {
        int directionParameter = SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter1);
        int distanceIndex = SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter2);
        int speedIndex = SnesLittleEndian.HighByte(slot.Spawn.Population.Parameter2);
        if (!CacatacProjectileDefinitions.Directions.TryDecode(directionParameter, out CacatacDirection direction))
        {
            throw new InvalidDataException(
                $"Cacatac parameter one ${slot.Spawn.Population.Parameter1:X4} selects direction " +
                $"{directionParameter}, outside its three-word function table.");
        }
        if (distanceIndex >= 6)
        {
            throw new InvalidDataException(
                $"Cacatac parameter two ${slot.Spawn.Population.Parameter2:X4} selects travel-distance " +
                $"index {distanceIndex}, outside its six-word table.");
        }
        if (speedIndex >= EnemyLinearSpeedDefinitions.RecordCount)
        {
            throw new InvalidDataException(
                $"Cacatac parameter two ${slot.Spawn.Population.Parameter2:X4} selects linear-speed " +
                $"index {speedIndex}, outside the NTSC retail table.");
        }

        bool upsideUp = (SnesLittleEndian.HighByteInPlace(slot.Spawn.Population.Parameter1)) != 0;
        slot.Frame = EnemyFrameId.Empty;

        var travel = new Offset16<RoomPixels>(checked((short)CacatacMovementDefinitions.TravelDistance((byte)distanceIndex)), 0);
        Position16<RoomPixels> spawn = slot.Position.Whole;
        int speedRecord = speedIndex * EnemyLinearSpeedDefinitions.RecordSize;
        var right = EnemyLinearSpeedDefinitions.Read(speedRecord);
        var left = EnemyLinearSpeedDefinitions.Read(speedRecord + 4);
        var state = new CacatacEnemyState
        {
            RightVelocity = right,
            LeftVelocity = left,
            Direction = direction,
            Function = direction switch
            {
                CacatacDirection.Left => CacatacEnemyFunction.MovingLeft,
                CacatacDirection.Right => CacatacEnemyFunction.MovingRight,
                CacatacDirection.InitialStop => CacatacEnemyFunction.Stopped,
                _ => throw new InvalidOperationException($"Undefined Cacatac direction {direction}."),
            },
            RightPatrolLimit = spawn + travel,
            LeftPatrolLimit = spawn - travel,
            UpsideUp = upsideUp,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);
        SetCacatacProgram(slot, state, upsideUp ? CacatacProgram.UpsideUpIdle : CacatacProgram.UpsideDownIdle);
    }

    /// <summary>Ports <c>MainAI_Cacatac</c> and its three indirect targets.</summary>
    private void RunCacatacMain(RoomEnemySlot slot, CacatacEnemyState state)
    {
        switch (state.Function)
        {
            case CacatacEnemyFunction.MovingLeft:
                MoveCacatac(slot, state, direction: HorizontalDirection.Left);
                MaybeStartCacatacAttack(slot, state);
                return;
            case CacatacEnemyFunction.MovingRight:
                MoveCacatac(slot, state, direction: HorizontalDirection.Right);
                MaybeStartCacatacAttack(slot, state);
                return;
            case CacatacEnemyFunction.Stopped:
                return;
            default:
                throw new InvalidDataException(
                    $"Cacatac function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>
    /// Ports the split-word additions at $A2:9FBA and $A2:9FEC: the 16.16 velocity is added to X with the
    /// fraction's carry into the whole, then the wrapped signed distance to the patrol limit decides the turn.
    /// </summary>
    private static void MoveCacatac(
        RoomEnemySlot slot,
        CacatacEnemyState state,
        HorizontalDirection direction)
    {
        slot.Position = slot.Position.AdvanceX(direction == HorizontalDirection.Right ? state.RightVelocity : state.LeftVelocity);

        if (direction != HorizontalDirection.Right)
        {
            if ((slot.Position.Whole - state.LeftPatrolLimit).X >= 0)
                return;
            state.Function = CacatacEnemyFunction.MovingRight;
            state.Direction = CacatacDirection.Right;
            return;
        }

        if ((slot.Position.Whole - state.RightPatrolLimit).X < 0)
            return;
        state.Function = CacatacEnemyFunction.MovingLeft;
        state.Direction = CacatacDirection.Left;
    }

    /// <summary>Ports the 3/256 random attack gate at $A2:A01C.</summary>
    private void MaybeStartCacatacAttack(RoomEnemySlot slot, CacatacEnemyState state)
    {
        int decision = unchecked((byte)(_room.Random.Generate() + slot.FrameCounter));
        if (decision >= 3)
            return;

        state.Function = CacatacEnemyFunction.Stopped;
        SetCacatacProgram(slot, state, state.UpsideUp ? CacatacProgram.UpsideUpAttack : CacatacProgram.UpsideDownAttack);
    }

    /// <summary>Animation instruction $A2:A095: restore patrol from the direction word.</summary>
    private static void RestoreCacatacPatrol(CacatacEnemyState state)
    {
        // The assembly uses BEQ, not CMP #1. Its special initial direction two therefore
        // becomes moving-right once the first idle instruction executes.
        state.Function = state.Direction == CacatacDirection.Left
            ? CacatacEnemyFunction.MovingLeft
            : CacatacEnemyFunction.MovingRight;
    }

    /// <summary>Animation instruction $A2:9F2A: publish library-two sound $34.</summary>
    private void PlayCacatacSpikeSound() =>
        LastCacatacSoundEffect = EnemySoundEffectDefinitions.CacatacSpikeSound;

    /// <summary>Starts a patrol or attack animation on the next instruction tick.</summary>
    internal static void SetCacatacProgram(RoomEnemySlot slot, CacatacEnemyState state, CacatacProgram program)
    {
        state.Program = AnimationProgramCursor<CacatacProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Registers the spike's ten direction-table movers and viewport cull.</summary>
    internal static void AddCacatacProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            // Cacatac spike: ten direction-table movers.
            RunCacatacSpikePreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.CacatacSpike);
    }

    /// <summary>Ports Cacatac's main AI.</summary>
    internal void RunCacatacMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunCacatacMain(slot, slot.RequireSpeciesState<CacatacEnemyState>());
    }

    /// <summary>Advances Cacatac's typed animation.</summary>
    internal void AdvanceCacatacTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, CacatacAnimations.Programs, CacatacFrameDefinitions.ShownFrames.Of,
            static (CacatacEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyCacatacCue(slot, state, cue));

    /// <summary>
    /// Ports <c>SpawnEnemyProjectileY_ParameterA_XGraphics</c> and initializer $86:D992.
    /// The source actor supplies both origin subpixels and its combined palette/tile index.
    /// </summary>
    internal void SpawnCacatacSpike(RoomEnemySlot source, CacatacSpikeDirection direction)
    {
        // Rejects an odd or out-of-table direction before allocating, as the native selector check does.
        CacatacProjectileProgram program = CacatacProjectileDefinitions.Program(direction);

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CacatacSpike,
            source.Graphics);
        int selector = CacatacProjectileDefinitions.SpikeSelectors.Parameter(direction);
        projectile.DirectionParameter = selector;
        projectile.Variable0 = selector;
        projectile.InstallAnimation(new CacatacProjectileAnimation(program));
        projectile.Position = source.Position;
        CacatacSpikeSpeedPair speeds = CacatacProjectileDefinitions.SpeedPair(direction);
        projectile.Velocity = new(speeds.Positive, speeds.Negative);
    }

    /// <summary>
    /// Ports pre-instruction $86:D9DB and the ten callback pointers at
    /// $86:D97E-$86:D991, selected by even direction bytes $00..$12.
    /// In selector order the native callbacks are $DA8E, $DA98, $DA93,
    /// $DA8E, $DA9D, $DA93, $DAA2, $DAB2, $DAAA, $DABA. The two left-facing
    /// and two right-facing variants share callbacks; diagonals compose one
    /// horizontal and one vertical signed-speed move. The native
    /// pre-instruction culls the projectile after its selected movement.
    /// These authored callback addresses do not form a uniform pointer stride.
    /// </summary>
    internal static void RunCacatacSpikePreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        CacatacSpikeDirection direction = CacatacProjectileDefinitions.SpikeSelectors.Decode(projectile.Variable0);
        switch (direction)
        {
            case CacatacSpikeDirection.LeftFacingUp:
            case CacatacSpikeDirection.LeftFacingDown:
                MoveCacatacSpikeX(projectile, projectile.Velocity.Y.RawValue);
                break;
            case CacatacSpikeDirection.Up:
                MoveCacatacSpikeY(projectile, projectile.Velocity.Y.RawValue);
                break;
            case CacatacSpikeDirection.RightFacingUp:
            case CacatacSpikeDirection.RightFacingDown:
                MoveCacatacSpikeX(projectile, projectile.Velocity.X.RawValue);
                break;
            case CacatacSpikeDirection.Down:
                MoveCacatacSpikeY(projectile, projectile.Velocity.X.RawValue);
                break;
            case CacatacSpikeDirection.UpLeft:
                MoveCacatacSpikeX(projectile, projectile.Velocity.Y.RawValue);
                MoveCacatacSpikeY(projectile, projectile.Velocity.Y.RawValue);
                break;
            case CacatacSpikeDirection.UpRight:
                MoveCacatacSpikeX(projectile, projectile.Velocity.X.RawValue);
                MoveCacatacSpikeY(projectile, projectile.Velocity.Y.RawValue);
                break;
            case CacatacSpikeDirection.DownLeft:
                MoveCacatacSpikeX(projectile, projectile.Velocity.Y.RawValue);
                MoveCacatacSpikeY(projectile, projectile.Velocity.X.RawValue);
                break;
            case CacatacSpikeDirection.DownRight:
                MoveCacatacSpikeX(projectile, projectile.Velocity.X.RawValue);
                MoveCacatacSpikeY(projectile, projectile.Velocity.X.RawValue);
                break;
            default:
                throw new InvalidDataException(
                    $"Cacatac spike direction ${projectile.Variable0:X4} is not translated.");
        }

        DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }

    /// <summary>Advances one spike horizontally using its signed 8.8 velocity as a 16.16 delta.</summary>
    private static void MoveCacatacSpikeX(
        RoomEnemyProjectileSlot projectile,
        int velocity)
    {
        projectile.Position = projectile.Position.AdvanceX(new SignedEightEight(checked((ushort)velocity)).ToSixteenSixteenDelta());
    }

    /// <summary>Advances one spike vertically using its signed 8.8 velocity as a 16.16 delta.</summary>
    private static void MoveCacatacSpikeY(
        RoomEnemyProjectileSlot projectile,
        int velocity)
    {
        projectile.Position = projectile.Position.AdvanceY(new SignedEightEight(checked((ushort)velocity)).ToSixteenSixteenDelta());
    }

    /// <summary>
    /// Shared clone of bank-$86's signed four-CMP viewport cull. Equality with the right or
    /// bottom edge remains alive; only strictly outside coordinates delete the projectile.
    /// </summary>
    internal static void DeleteEnemyProjectileIfOutsideInclusiveViewport(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        int right = unchecked((ushort)(camera.X + 256));
        int bottom = unchecked((ushort)(camera.Y + 256));
        bool outside = unchecked((short)(projectile.Position.X.Whole - camera.X)) < 0 ||
            unchecked((short)(right - projectile.Position.X.Whole)) < 0 ||
            unchecked((short)(projectile.Position.Y.Whole - camera.Y)) < 0 ||
            unchecked((short)(bottom - projectile.Position.Y.Whole)) < 0;
        if (outside)
            projectile.Clear();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastCacatacSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastCacatacSoundEffect is { } sound)
            audio.QueueEnemySound(sound, maximumQueued: SoundQueueLimit.Max6);
    }
}
