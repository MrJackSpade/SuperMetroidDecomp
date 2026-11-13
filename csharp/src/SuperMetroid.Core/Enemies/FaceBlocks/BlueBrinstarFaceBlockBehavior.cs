using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>The blue Brinstar face block species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BlueBrinstarFaceBlockBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The pass-through shot reaction for this species' private shot callback.</summary>
    internal static EnemyShotReaction PassThroughShot { get; } =
        new(enemy => enemy.DefinitionId == EnemyDefinitionId.FaceBlock &&
                     EnemyCollision.ShotAi(enemy) == EnemyReaction.BlueBrinstarFaceBlockShot)
        {
            AfterSuperMissileQuake = ProjectilePassThrough.Shot,
        };

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.FaceBlock];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BlueBrinstarFaceBlockEntity(this, slot, common);

    /// <summary>The room's enemy slots, whose face blocks all show the room-wide glow.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Exposes the door transition that pauses the face blocks' glow.</summary>
    private readonly ElevatorBehavior _elevatorBehavior;

    /// <summary>Binds the species to the room's slots and door-transition state.</summary>
    internal BlueBrinstarFaceBlockBehavior(EnemySlotTable slotTable, ElevatorBehavior elevatorBehavior)
    {
        _elevatorBehavior = elevatorBehavior;
        _slotTable = slotTable;
    }

    /// <summary>The updates each frame of the face blocks' glow holds for.</summary>
    private const int GlowFrameUpdates = 16;

    /// <summary>
    /// Whether a face block has started the room's glow, which it does once Samus has collected the Morph Ball. The room
    /// has one glow, not one per block.
    /// </summary>
    private bool _glowStarted;
    /// <summary>Updates left before the glow steps, restarted at sixteen by each step and by a block waking.</summary>
    private int _glowStepCountdown;
    /// <summary>
    /// The frame of the glow every face block shows, 0 through <see cref="FaceBlockShades.GlowFrames"/> minus one, kept
    /// when a block wakes. It starts at the last frame, so the glow's first step shows its first.
    /// </summary>
    private int _glowFrame = FaceBlockShades.GlowFrames - 1;

    /// <summary>The glow every face block in the room shows once it has started (#1512).</summary>
    private PictureShading Glow => PictureShading.Shaded(FaceBlockShades.Glow(_glowFrame));

    /// <summary>Initializes the Blue Brinstar face block.</summary>
    internal void InitializeBlueBrinstarFaceBlock(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new BlueBrinstarFaceBlockEnemyState();
        slot.SetSpeciesState(state);

        // The neutral animation is a one-frame neutral map followed by the common sleep. All
        // authored directional lists use the same neutral map as their first frame.
        state.Playback = new(BlueBrinstarFaceBlockAnimations.StartingWith(BlueBrinstarFaceBlockProgram.Initial), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        // The initializer always restarts the glow's countdown, even before Samus has the Morph Ball, and starts the glow
        // only once she has collected it. Main AI makes the same test every update, so a block in the room where she
        // collects it starts the glow at once.
        if (HasCollectedMorphBall(samus))
            StartGlow(slot);
        _glowStepCountdown = GlowFrameUpdates;

        // Keeps only parameter two bit zero, moved to the sign bit. This looks
        // backwards until main AI is read: activation requires the Samus delta sign to be
        // *different* from this marker.
        state.FacesRight = slot.SpawnSettings<FaceBlockSpawnSettings>().Facing == HorizontalDirection.Right;
        state.Activated = false;
        state.LastHorizontalDelta = 0;
    }

    /// <summary>Runs the Blue Brinstar face block's main AI.</summary>
    internal void RunBlueBrinstarFaceBlockMain(
        RoomEnemySlot slot,
        BlueBrinstarFaceBlockEnemyState state,
        SamusState? samus)
    {
        // The collected items, not the equipped ones, gate both the glow and waking. A null Samus owner means the caller
        // has not supplied her, so leaving the block dormant is the only non-invented behaviour until a gameplay update does.
        if (!HasCollectedMorphBall(samus))
            return;

        StartGlow(slot);
        if (state.Activated)
            return;

        if (CoordinateAxis.Distance(samus.Position.Y.Whole, slot.Position.Y.Whole) >=
            slot.SpawnSettings<FaceBlockSpawnSettings>().TriggerDistance)
            return;

        // The horizontal distance is recorded after the Y test but before the X test, including on frames
        // that fail horizontal range or side, so a frame that fails the X test still
        // records the vertical distance.
        int horizontalDelta = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole);
        state.LastHorizontalDelta = horizontalDelta;
        if (Math.Abs(horizontalDelta) >= slot.SpawnSettings<FaceBlockSpawnSettings>().TriggerDistance)
            return;

        bool samusToTheLeft = horizontalDelta < 0;
        if (samusToTheLeft == state.FacesRight)
            return;

        state.Playback = new(BlueBrinstarFaceBlockAnimations.StartingWith(samusToTheLeft
            ? BlueBrinstarFaceBlockProgram.SamusLeft
            : BlueBrinstarFaceBlockProgram.SamusRight), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.Activated = true;

        // Waking restarts the room's glow countdown at sixteen updates but keeps the glow's frame.
        _glowStepCountdown = GlowFrameUpdates;
    }

    /// <summary>Uses the collected items so unequipping Morph Ball does not put active blocks back to sleep.</summary>
    private static bool HasCollectedMorphBall([NotNullWhen(true)] SamusState? samus) =>
        samus is not null &&
        samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall);

    /// <summary>
    /// Starts the room's glow and shows it on <paramref name="slot"/>; every face block, and only a face block, shows the
    /// glow (#1512).
    /// </summary>
    private void StartGlow(RoomEnemySlot slot)
    {
        _glowStarted = true;
        slot.Shading = Glow;
    }

    /// <summary>
    /// Counts down the room's glow once a block has started it, after the enemies' update; when the frame's time is up
    /// the glow steps to its next frame on every face block. A door transition pauses it.
    /// </summary>
    internal void StepGlow()
    {
        if (!_glowStarted || _elevatorBehavior.ElevatorDoorTransitionActive)
            return;

        if (--_glowStepCountdown != 0)
            return;

        _glowStepCountdown = GlowFrameUpdates;
        _glowFrame = (_glowFrame + 1) % FaceBlockShades.GlowFrames;
        PictureShading glow = Glow;
        foreach (RoomEnemySlot block in _slotTable.Slots)
        {
            if (block.DefinitionId == EnemyDefinitionId.FaceBlock)
                block.Shading = glow;
        }
    }

    /// <summary>Rejects AI dispatch for a face-block slot that was not initialized on room load.</summary>
    internal static BlueBrinstarFaceBlockEnemyState RequireBlueBrinstarFaceBlockState(
        RoomEnemySlot slot) =>
        slot.SpeciesState<BlueBrinstarFaceBlockEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Blue Brinstar face-block state.");

    /// <summary>Advances the blue Brinstar face block's typed animation.</summary>
    internal static void AdvanceBlueBrinstarFaceBlockTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireBlueBrinstarFaceBlockState(slot);
        AnimationPlayback<BlueBrinstarFaceBlockProgram> playback = state.Playback;
        EnemyProgramAnimation.Play(slot, BlueBrinstarFaceBlockAnimations.Segments, ref playback, BlueBrinstarFaceBlockFrameDefinitions.ShownFrames.Of,
            static cue => throw new InvalidOperationException($"A cue-free blue Brinstar face block animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Runs the blue Brinstar face block's initialization AI.</summary>
    internal void RunBlueBrinstarFaceBlockInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeBlueBrinstarFaceBlock(slot, surroundings.Samus);
    }

    /// <summary>Runs the blue Brinstar face block's main AI.</summary>
    internal void RunBlueBrinstarFaceBlockMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBlueBrinstarFaceBlockMain(
            slot,
            RequireBlueBrinstarFaceBlockState(slot),
            frame.Samus);
    }
}
