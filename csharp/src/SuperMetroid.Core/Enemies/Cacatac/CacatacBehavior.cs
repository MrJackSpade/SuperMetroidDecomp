using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>The Cacatac species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class CacatacBehavior : IEnemySpecies
{
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
                SpawnCacatacSpike(slot, cue.Value);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CacatacCueKind)} {cue.Kind}.");
        }
        return null;
    }


    /// <summary>Typed patrol and attack state indexed by native enemy slot.</summary>
    internal readonly CacatacEnemyState?[] _cacatacStates =
        new CacatacEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Last library-two spike-release sound requested in this enemy frame.</summary>
    public ushort? LastCacatacSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Cacatac</c> at $A2:9F48.</summary>
    internal void InitializeCacatac(RoomEnemySlot slot)
    {
        var direction = (CacatacDirection)SnesLittleEndian.LowByte(slot.Parameter1);
        int distanceIndex = SnesLittleEndian.LowByte(slot.Parameter2);
        int speedIndex = SnesLittleEndian.HighByte(slot.Parameter2);
        if (!Enum.IsDefined(direction))
        {
            throw new InvalidDataException(
                $"Cacatac parameter one ${slot.Parameter1:X4} selects direction " +
                $"{(ushort)direction}, outside its three-word function table.");
        }
        if (distanceIndex >= 6)
        {
            throw new InvalidDataException(
                $"Cacatac parameter two ${slot.Parameter2:X4} selects travel-distance " +
                $"index {distanceIndex}, outside its six-word table.");
        }
        if (speedIndex >= EnemyLinearSpeedDefinitions.RecordCount)
        {
            throw new InvalidDataException(
                $"Cacatac parameter two ${slot.Parameter2:X4} selects linear-speed " +
                $"index {speedIndex}, outside the NTSC retail table.");
        }

        bool upsideUp = (SnesLittleEndian.HighByteInPlace(slot.Parameter1)) != 0;
        slot.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;

        ushort distance = CacatacMovementDefinitions.TravelDistance((byte)distanceIndex);
        int speedRecord = speedIndex * EnemyLinearSpeedDefinitions.RecordSize;
        var right = EnemyLinearSpeedDefinitions.Read(speedRecord);
        var left = EnemyLinearSpeedDefinitions.Read(speedRecord + 4);
        var state = new CacatacEnemyState(slot)
        {
            RightVelocity = unchecked((ushort)right.Whole),
            RightSubvelocity = right.Fraction,
            LeftVelocity = unchecked((ushort)left.Whole),
            LeftSubvelocity = left.Fraction,
            Direction = direction,
            Function = direction switch
            {
                CacatacDirection.Left => CacatacEnemyFunction.MovingLeft,
                CacatacDirection.Right => CacatacEnemyFunction.MovingRight,
                CacatacDirection.InitialStop => CacatacEnemyFunction.Stopped,
                _ => throw new InvalidOperationException($"Undefined Cacatac direction {direction}."),
            },
            MaximumXPosition = unchecked((ushort)(slot.Position.X.Whole + distance)),
            MinimumXPosition = unchecked((ushort)(slot.Position.X.Whole - distance)),
            UpsideUp = upsideUp,
        };
        _cacatacStates[slot.SlotIndex] = state;
        SetCacatacProgram(slot, state, upsideUp ? CacatacProgram.UpsideUpIdle : CacatacProgram.UpsideDownIdle);
    }

    /// <summary>Ports <c>MainAI_Cacatac</c> and its three indirect targets.</summary>
    private void RunCacatacMain(RoomEnemySlot slot, CacatacEnemyState state)
    {
        switch (state.Function)
        {
            case CacatacEnemyFunction.MovingLeft:
                MoveCacatac(slot, state, movingRight: false);
                MaybeStartCacatacAttack(slot, state);
                return;
            case CacatacEnemyFunction.MovingRight:
                MoveCacatac(slot, state, movingRight: true);
                MaybeStartCacatacAttack(slot, state);
                return;
            case CacatacEnemyFunction.Stopped:
                return;
            default:
                throw new InvalidDataException(
                    $"Cacatac function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the split-word additions at $A2:9FBA and $A2:9FEC.</summary>
    private static void MoveCacatac(
        RoomEnemySlot slot,
        CacatacEnemyState state,
        bool movingRight)
    {
        ushort subvelocity = movingRight
            ? state.RightSubvelocity
            : state.LeftSubvelocity;
        ushort velocity = movingRight
            ? state.RightVelocity
            : state.LeftVelocity;

        uint fractionalSum = (uint)slot.Position.X.Fraction + subvelocity;
        slot.Position = slot.Position.WithFractionX(unchecked((ushort)fractionalSum));
        if (fractionalSum > ushort.MaxValue)
            slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + 1)));
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + velocity)));

        if (!movingRight)
        {
            if (!EnemyMath.IsNegative16(unchecked((ushort)(slot.Position.X.Whole - state.MinimumXPosition))))
                return;
            state.Function = CacatacEnemyFunction.MovingRight;
            state.Direction = CacatacDirection.Right;
            return;
        }

        if (EnemyMath.IsNegative16(unchecked((ushort)(slot.Position.X.Whole - state.MaximumXPosition))))
            return;
        state.Function = CacatacEnemyFunction.MovingLeft;
        state.Direction = CacatacDirection.Left;
    }

    /// <summary>Ports the 3/256 random attack gate at $A2:A01C.</summary>
    private void MaybeStartCacatacAttack(RoomEnemySlot slot, CacatacEnemyState state)
    {
        ushort decision = unchecked((byte)(_room.Random.Generate() + slot.FrameCounter));
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
    private void PlayCacatacSpikeSound() => LastCacatacSoundEffect = EnemySoundEffectDefinitions.CacatacSpikeSound;

    /// <summary>Starts a patrol or attack animation on the next instruction tick.</summary>
    internal static void SetCacatacProgram(RoomEnemySlot slot, CacatacEnemyState state, CacatacProgram program)
    {
        state.Program = AnimationProgramCursor<CacatacProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Requires an initialized typed state before a Cacatac AI or list callback runs.</summary>
    private CacatacEnemyState RequireCacatacState(RoomEnemySlot slot) =>
        _cacatacStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Cacatac state.");

    /// <summary>Registers the spike's ten direction-table movers and viewport cull.</summary>
    internal static void RegisterCacatacProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            // Cacatac spike: ten direction-table movers.
            RunCacatacSpikePreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.CacatacSpike);
    }

    /// <summary>Routes bank-$A2 initialization only to Cacatac definitions.</summary>
    internal void RegisterCacatacInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Cacatac, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Cacatac, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeCacatac(slot);
        });
    }

    /// <summary>Routes Cacatac's three-state patrol and attack dispatcher through the enemy table.</summary>
    internal void RegisterCacatacMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Cacatac, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Cacatac, (slot, frame, nmiFrameCounter8) =>
        {
            RunCacatacMain(slot, RequireCacatacState(slot));
        });
    }

    /// <summary>Runs Cacatac's idle and spike-attack animations as typed programs (#641).</summary>
    internal void RegisterCacatacTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Cacatac, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireCacatacState(slot);
            AnimationProgramCursor<CacatacProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, CacatacAnimations.Programs, ref cursor, CacatacFrameDefinitions.Spritemap,
                    (cue, _) => ApplyCacatacCue(slot, state, cue));
            state.Program = cursor;
        });


    /// <summary>
    /// Ports <c>SpawnEnemyProjectileY_ParameterA_XGraphics</c> and initializer $86:D992.
    /// The source actor supplies both origin subpixels and its combined palette/tile index.
    /// </summary>
    internal void SpawnCacatacSpike(RoomEnemySlot source, ushort rawDirection)
    {
        if ((rawDirection & 1) != 0 || rawDirection > (ushort)CacatacSpikeDirection.DownRight)
        {
            throw new InvalidDataException(
                $"Cacatac spike direction ${rawDirection:X4} is outside ten even table offsets.");
        }

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CacatacSpike,
            unchecked((ushort)(source.PaletteIndex | source.VramTilesIndex)));
        projectile.DirectionParameter = rawDirection;
        projectile.Variable0 = rawDirection;
        var direction = (CacatacSpikeDirection)rawDirection;
        projectile.InstallAnimation(new CacatacProjectileAnimation(CacatacProjectileDefinitions.Program(direction)));
        projectile.Position = source.Position;
        CacatacSpikeSpeedPair speeds = CacatacProjectileDefinitions.SpeedPair(direction);
        projectile.Velocity = new(new SnesSignedEightEight(speeds.Positive), new SnesSignedEightEight(speeds.Negative));
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
        CacatacSpikeDirection direction = (CacatacSpikeDirection)projectile.Variable0;
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
        ushort velocity)
    {
        projectile.Position = projectile.Position.AdvanceX(new SnesSignedEightEight(velocity).ToSixteenSixteenDelta());
    }

    /// <summary>Advances one spike vertically using its signed 8.8 velocity as a 16.16 delta.</summary>
    private static void MoveCacatacSpikeY(
        RoomEnemyProjectileSlot projectile,
        ushort velocity)
    {
        projectile.Position = projectile.Position.AdvanceY(new SnesSignedEightEight(velocity).ToSixteenSixteenDelta());
    }

    /// <summary>
    /// Shared clone of bank-$86's signed four-CMP viewport cull. Equality with the right or
    /// bottom edge remains alive; only strictly outside coordinates delete the projectile.
    /// </summary>
    internal static void DeleteEnemyProjectileIfOutsideInclusiveViewport(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        ushort right = unchecked((ushort)(camera.X + 256));
        ushort bottom = unchecked((ushort)(camera.Y + 256));
        bool outside = unchecked((short)(projectile.Position.X.Whole - camera.X)) < 0 ||
            unchecked((short)(right - projectile.Position.X.Whole)) < 0 ||
            unchecked((short)(projectile.Position.Y.Whole - camera.Y)) < 0 ||
            unchecked((short)(bottom - projectile.Position.Y.Whole)) < 0;
        if (outside)
            projectile.Clear();
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_cacatacStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastCacatacSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastCacatacSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
