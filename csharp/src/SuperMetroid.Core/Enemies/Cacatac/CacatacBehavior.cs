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
        CacatacSpawnSettings settings = slot.SpawnSettings<CacatacSpawnSettings>();
        CacatacDirection direction = settings.Direction;
        int distanceIndex = settings.DistanceIndex;
        int speedIndex = settings.SpeedIndex;
        if (distanceIndex >= 6)
        {
            throw new InvalidDataException(
                $"Cacatac travel-distance index {distanceIndex} is outside its six-word table.");
        }
        if (speedIndex >= EnemyLinearSpeedDefinitions.RowCount)
        {
            throw new InvalidDataException(
                $"Cacatac linear-speed index {speedIndex} is outside the NTSC retail table.");
        }

        bool upsideUp = settings.UpsideUp;
        slot.Frame = EnemyFrameId.Empty;

        var travel = new Offset16<RoomPixels>(checked((short)CacatacPatrol.TravelDistance((byte)distanceIndex)), 0);
        Position16<RoomPixels> spawn = slot.Position.Whole;
        EnemyLinearSpeedRow speeds = EnemyLinearSpeeds.Row(speedIndex);
        var state = new CacatacEnemyState
        {
            RightVelocity = speeds.Positive,
            LeftVelocity = speeds.Negative,
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
        CacatacProjectileProgram program = CacatacSpikes.Program(direction);

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CacatacSpike,
            source.Graphics);
        projectile.InstallAnimation(new CacatacProjectileAnimation(program));
        projectile.Position = source.Position;
        projectile.Velocity = CacatacSpikes.Velocity(direction);
    }

    /// <summary>
    /// Ports pre-instruction $86:D9DB: the spike moves along its velocity, horizontally then vertically, then is culled
    /// once outside the view.
    /// </summary>
    internal static void RunCacatacSpikePreInstruction(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
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
