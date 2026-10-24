using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The Rio species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class RioBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Rio];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new RioEntity(this, slot, common);

    /// <summary>Supplies the global enemy RNG word consumed on every Rio AI frame.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves Rio's horizontal and vertical flight against current room terrain.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds Rio AI to shared room RNG and enemy terrain collision.</summary>
    internal RioBehavior(EnemyRoomBinding room, EnemyTerrain terrain)
    {
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Strict horizontal Samus proximity threshold that starts a dive.</summary>
    private const ushort RioTriggerDistance = 160;
    /// <summary>8.8 vertical velocity decrement applied during dive and rebound.</summary>
    private const ushort RioGravityStep = 24;

    /// <summary>Most recent library-two sound request produced by Rio during this frame.</summary>
    public SoundEffectId? LastRioSoundEffect { get; private set; }

    /// <summary>Ports <c>Rio_Init</c> at $A2:BBCD.</summary>
    internal static void InitializeRio(RoomEnemySlot slot)
    {
        var state = new RioEnemyState
        {
            AnimationFinished = false,
            InstalledProgram = null,
            Function = RioEnemyFunction.WaitingForSamus,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // Rio_Init writes the list directly rather than calling Rio_6. The installed animation must
        // therefore remain none until the first behavioral transition installs a new list.
        state.Program = AnimationProgramCursor<RioProgram>.Start(RioProgram.Idle);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Ports <c>Rio_Main</c> and Rio_1..Rio_5 at $A2:BBE3-$BD53.</summary>
    private void RunRioMain(
        RoomEnemySlot slot,
        RioEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        Position16<RoomPixels> camera)
    {
        // Rio_Main advances the global enemy RNG even though this particular family never
        // consumes the returned word. Omitting that apparently-useless call changes every
        // later random enemy in the room, so it is part of Rio's observable behavior.
        _room.Random.Generate();

        switch (state.Function)
        {
            case RioEnemyFunction.WaitingForSamus:
                if (samus is null || !EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.X.Whole,
                        slot.Position.X.Whole,
                        RioTriggerDistance))
                {
                    return;
                }

                state.Velocity = new EightEightVector<RoomPixels>(
                    new SnesSignedEightEight(RioLaunchDefinitions.RioXVelocity),
                    new SnesSignedEightEight(RioLaunchDefinitions.RioYVelocity));
                if (unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0)
                    state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
                InstallRioInstructionList(
                    slot,
                    state,
                    RioProgram.SwoopingPart1);
                state.Function = RioEnemyFunction.Diving;

                // CheckIfEnemyIsOnScreen returns zero for an on-screen origin. The native
                // branch consequently queues sound $65 only for the visible attack start.
                if (!EnemyProcessingWindow.EnemyWithNormalSpritesIsOffScreen(slot, camera))
                    LastRioSoundEffect = EnemySoundEffectDefinitions.RioDiveSound;
                return;

            case RioEnemyFunction.WaitingForLandingAnimation:
                if (!state.AnimationFinished)
                    return;
                state.AnimationFinished = false;
                InstallRioInstructionList(
                    slot,
                    state,
                    RioProgram.PostSwoopIdle);
                state.Function = RioEnemyFunction.WaitingForSamus;
                return;

            case RioEnemyFunction.Diving:
                RequireRioLevel(level);
                if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.X.RawValue)))
                {
                    state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue)))
                {
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }

                state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue - RioGravityStep))));
                if (state.Velocity.Y.SignedRawValue < 0)
                {
                    state.SavedHorizontalVelocity = state.Velocity.X;
                    state.Velocity = new EightEightVector<RoomPixels>(0, 0);
                    state.Function = RioEnemyFunction.HoveringTowardSamus;
                }
                else if (state.AnimationFinished)
                {
                    state.AnimationFinished = false;
                    InstallRioInstructionList(
                        slot,
                        state,
                        RioProgram.SwoopingPart2);
                }
                return;

            case RioEnemyFunction.BouncingBackToPerch:
                RequireRioLevel(level);
                if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.X.RawValue)))
                {
                    state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
                }
                if (_terrain.MoveEnemyVertically(
                        level,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue)))
                {
                    InstallRioInstructionList(
                        slot,
                        state,
                        RioProgram.SwoopCooldown);
                    state.Function = RioEnemyFunction.WaitingForLandingAnimation;
                }
                else
                {
                    state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue - RioGravityStep))));
                }
                return;

            case RioEnemyFunction.HoveringTowardSamus:
                if (samus is null)
                    throw new InvalidOperationException("Rio homing requires the active Samus actor.");
                RequireRioLevel(level);

                // Once Rio reaches Samus's Y coordinate from above, it restores the saved
                // horizontal dive speed and starts the upward/bounce phase at -1/256 px.
                if (unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)) >= 0)
                {
                    state.Velocity = new EightEightVector<RoomPixels>(
                        state.SavedHorizontalVelocity,
                        new SnesSignedEightEight(unchecked((ushort)-1)));
                    state.Function = RioEnemyFunction.BouncingBackToPerch;
                    return;
                }

                byte angle = EnemyMath.CalculateCartridgeAngle(
                    new(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole)))).ReinterpretAsTableAngle().RawValue;
                state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(ReadRioSignedSineCosineSample(
                    unchecked(new SnesByteAngle((byte)(angle + EnemyByteAngleDefinitions.QuarterTurn))))));
                state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(ReadRioSignedSineCosineSample(new SnesByteAngle(angle))));
                _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                    level,
                    slot,
                    EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.X.RawValue));
                _terrain.MoveEnemyVertically(
                    level,
                    slot,
                    EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue));
                return;

            default:
                throw new InvalidDataException(
                    $"Rio function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Carries out one cue of the Rio's animations: telling main AI the animation has finished.</summary>
    private static RioProgram? ApplyRioCue(RioEnemyState state, RioCue cue)
    {
        switch (cue.Kind)
        {
            case RioCueKind.AnimationFinished:
                state.AnimationFinished = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(RioCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Ports Rio_6: install an animation only when it actually changes.</summary>
    private static void InstallRioInstructionList(
        RoomEnemySlot slot,
        RioEnemyState state,
        RioProgram program) =>
        EnemyProgramAnimation.InstallOnce(slot, state.InstalledProgram, program, installed =>
        {
            state.InstalledProgram = installed;
            state.Program = AnimationProgramCursor<RioProgram>.Start(installed);
        });

    /// <summary>Reverses vertical velocity after collision and enters the rebound phase.</summary>
    private static void ReverseRioVerticalVelocityAndBounce(RioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.Y.SignedRawValue)));
        state.Function = RioEnemyFunction.BouncingBackToPerch;
    }

    /// <summary>
    /// Reads one sign-extended entry from kSinCosTable8bit_Sext at $A0:B443. Rio consumes
    /// the raw table sample as an 8.8 velocity; unlike Rinka, it performs no speed multiply.
    /// </summary>
    internal static ushort ReadRioSignedSineCosineSample(SnesByteAngle angle) =>
        unchecked((ushort)EnemyTrigonometryTables.SignedSine(angle));

    /// <summary>Rejects movement dispatch without active room collision data.</summary>
    private static void RequireRioLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Rio movement requires the current room level data.");
    }

    /// <summary>Ports Rio's main AI.</summary>
    internal void RunRioMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunRioMain(
            slot,
            slot.RequireSpeciesState<RioEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.Camera);
    }

    /// <summary>Advances Rio's typed animation.</summary>
    internal static void AdvanceRioTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, RioAnimations.Programs, RioFrameDefinitions.ShownFrames.Of,
            static (RioEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyRioCue(state, cue));

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastRioSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueEnemySoundIfAny(LastRioSoundEffect, maximumQueued: SoundQueueLimit.Max6);
    }
}
