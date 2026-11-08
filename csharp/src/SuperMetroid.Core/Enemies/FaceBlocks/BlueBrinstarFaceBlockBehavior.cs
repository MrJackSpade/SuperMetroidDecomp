using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>The blue Brinstar face block species owner: its per-room state, AI routines and the interactions it registers.</summary>
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

    /// <summary>Exposes the door-transition pause condition for the shared graphics hook.</summary>
    private readonly ElevatorBehavior _elevatorBehavior;

    /// <summary>Binds the species to the room's slots and door-transition state.</summary>
    internal BlueBrinstarFaceBlockBehavior(EnemySlotTable slotTable, ElevatorBehavior elevatorBehavior)
    {
        _elevatorBehavior = elevatorBehavior;
        _slotTable = slotTable;
    }

    /// <summary>Enemy frames between shared face-block palette advances.</summary>
    private const int BlueBrinstarFaceBlockPalettePeriod = 16;

    /// <summary>Whether an eligible block installed the room-wide graphics hook.</summary>
    /// <remarks>The game stores this animation in the shared enemy-graphics-drawn hook words, not per enemy.
    /// Multiple blocks repeatedly select the same graphics-set palette during main AI, and whichever eligible block
    /// runs last therefore owns this one shared base.</remarks>
    private bool _blueBrinstarFaceBlockPaletteHookInstalled;
    /// <summary>Shared countdown, reset to sixteen after each color update or actor activation.</summary>
    private int _blueBrinstarFaceBlockPaletteTimer;
    /// <summary>Shared eight-frame color index, 0 through 7, intentionally retained when an actor activates.</summary>
    private int _blueBrinstarFaceBlockPaletteFrame;
    /// <summary>
    /// The glow every face block in the room shows (#1512): its standard colours until the hook first advances, then
    /// the glow frame it advanced to last.
    /// </summary>
    private PictureShading _blueBrinstarFaceBlockGlow = PictureShading.Standard;

    /// <summary>Initializes the Blue Brinstar face block.</summary>
    internal void InitializeBlueBrinstarFaceBlock(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new BlueBrinstarFaceBlockEnemyState();
        slot.SetSpeciesState(state);

        // The neutral animation is a one-frame neutral map followed by the common sleep. All
        // authored directional lists use the same neutral map as their first frame.
        state.Program = AnimationProgramCursor<BlueBrinstarFaceBlockProgram>.Start(BlueBrinstarFaceBlockProgram.Initial);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        // The initializer always primes the shared countdown, even before Morph Ball exists.
        // It installs the hook only when collected-items bit two is already present. Main AI
        // performs the same item test every frame, allowing an actor in a persistent/custom
        // room to become active immediately after the item is acquired.
        if (HasCollectedMorphBall(samus))
            InstallBlueBrinstarFaceBlockPaletteHook(slot);
        _blueBrinstarFaceBlockPaletteTimer = BlueBrinstarFaceBlockPalettePeriod;

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
        // Collected-items, not equipped-items, gates both palette animation and waking. A
        // null Samus owner means the caller has not supplied that global, so leaving
        // the actor dormant is the only non-invented behavior until a gameplay frame does.
        if (!HasCollectedMorphBall(samus))
            return;

        InstallBlueBrinstarFaceBlockPaletteHook(slot);
        if (state.Activated)
            return;

        int verticalDelta = CoordinateAxis.Wrap(samus.Position.Y.Whole - slot.Position.Y.Whole);
        if (AbsoluteFaceBlockDelta(verticalDelta) >= slot.SpawnSettings<FaceBlockSpawnSettings>().TriggerDistance)
            return;

        // Variable B is written after the Y test but before the X test, including on frames
        // that fail horizontal range or side, so a frame that fails the X test still
        // records the vertical distance.
        int horizontalDelta = CoordinateAxis.Wrap(samus.Position.X.Whole - slot.Position.X.Whole);
        state.LastHorizontalDelta = horizontalDelta;
        if (AbsoluteFaceBlockDelta(horizontalDelta) >= slot.SpawnSettings<FaceBlockSpawnSettings>().TriggerDistance)
            return;

        bool samusToTheLeft = EnemyMath.IsNegative16(horizontalDelta);
        if (samusToTheLeft == state.FacesRight)
            return;

        state.Program = AnimationProgramCursor<BlueBrinstarFaceBlockProgram>.Start(samusToTheLeft
            ? BlueBrinstarFaceBlockProgram.SamusLeft
            : BlueBrinstarFaceBlockProgram.SamusRight);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.Activated = true;

        // Activation restarts the singleton palette hook at the game's sixteen-frame
        // cadence but deliberately does not reset its frame index.
        _blueBrinstarFaceBlockPaletteTimer = BlueBrinstarFaceBlockPalettePeriod;
    }

    /// <summary>Uses the collected-item word so unequipping Morph Ball does not put active blocks back to sleep.</summary>
    private static bool HasCollectedMorphBall([NotNullWhen(true)] SamusState? samus) =>
        samus is not null &&
        samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall);

    /// <summary>
    /// Reproduces the signed-magnitude helper over one wrapped 16-bit subtraction.
    /// In particular, -32768 remains -32768 just as the game's negation does.
    /// </summary>
    private static int AbsoluteFaceBlockDelta(int delta) =>
        Math.Abs(CoordinateAxis.AsSigned(Ensure.BetweenInclusive(0, ushort.MaxValue, delta)));

    /// <summary>
    /// Installs the singleton graphics hook and shows the room's glow on the block. The game's hook recolours the palette
    /// of the last eligible block, which every block sharing it, and Stoke where it shares the face blocks' palette, shows;
    /// here every face block, and only a face block, shows the glow (#1512).
    /// </summary>
    private void InstallBlueBrinstarFaceBlockPaletteHook(RoomEnemySlot slot)
    {
        _blueBrinstarFaceBlockPaletteHookInstalled = true;
        slot.Shading = _blueBrinstarFaceBlockGlow;
    }

    /// <summary>
    /// Runs the graphics-drawn palette hook. RoomEnemySystem executes
    /// graphics hooks once per enemy frame after actor processing, the same ownership model
    /// already used by Magdollite and Work Robot palette hooks. The door-transition gate pauses it during
    /// an enemy door transition; that translated word is exposed as ElevatorDoorTransitionActive.
    /// </summary>
    internal void StepBlueBrinstarFaceBlockPaletteAnimation()
    {
        if (!_blueBrinstarFaceBlockPaletteHookInstalled || _elevatorBehavior.ElevatorDoorTransitionActive)
            return;

        if (--_blueBrinstarFaceBlockPaletteTimer != 0)
            return;

        _blueBrinstarFaceBlockPaletteTimer = BlueBrinstarFaceBlockPalettePeriod;
        int frame = _blueBrinstarFaceBlockPaletteFrame % FaceBlockShades.GlowFrames;
        _blueBrinstarFaceBlockGlow = PictureShading.Shaded(FaceBlockShades.Glow(frame));
        foreach (RoomEnemySlot block in _slotTable.Slots)
        {
            if (block.DefinitionId == EnemyDefinitionId.FaceBlock)
                block.Shading = _blueBrinstarFaceBlockGlow;
        }

        // The index cycles through the eight frames, as the game's increment and low-bits mask do.
        _blueBrinstarFaceBlockPaletteFrame = (_blueBrinstarFaceBlockPaletteFrame + 1) % FaceBlockShades.GlowFrames;
    }

    /// <summary>Rejects AI dispatch for a face-block slot that was not initialized on room load.</summary>
    internal static BlueBrinstarFaceBlockEnemyState RequireBlueBrinstarFaceBlockState(
        RoomEnemySlot slot) =>
        slot.SpeciesState<BlueBrinstarFaceBlockEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Blue Brinstar face-block state.");

    /// <summary>Advances BlueBrinstarFaceBlock's typed animation.</summary>
    internal static void AdvanceBlueBrinstarFaceBlockTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireBlueBrinstarFaceBlockState(slot);
        AnimationProgramCursor<BlueBrinstarFaceBlockProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, BlueBrinstarFaceBlockAnimations.Programs, ref cursor, BlueBrinstarFaceBlockFrameDefinitions.ShownFrames.Of);
        state.Program = cursor;
    }

    /// <summary>Ports BlueBrinstarFaceBlock's initialization AI.</summary>
    internal void RunBlueBrinstarFaceBlockInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeBlueBrinstarFaceBlock(slot, surroundings.Samus);
    }

    /// <summary>Ports BlueBrinstarFaceBlock's main AI.</summary>
    internal void RunBlueBrinstarFaceBlockMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBlueBrinstarFaceBlockMain(
            slot,
            RequireBlueBrinstarFaceBlockState(slot),
            frame.Samus);
    }
}
