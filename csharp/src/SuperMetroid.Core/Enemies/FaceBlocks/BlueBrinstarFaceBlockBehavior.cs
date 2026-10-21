using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>The blue Brinstar face block species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class BlueBrinstarFaceBlockBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The pass-through shot reaction for this species' private shot callback.</summary>
    internal static EnemyShotReaction PassThroughShot { get; } =
        new(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.FaceBlock &&
                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.BlueBrinstarFaceBlockShot)
        {
            AfterSuperMissileQuake = ProjectilePassThrough.Shot,
        };

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.FaceBlock];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BlueBrinstarFaceBlockEntity(this, slot, common);

    /// <summary>Supplies installed auxiliary colors for the face-block animation.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Provides the live OBJ palette rows written by the graphics hook.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Exposes the door-transition pause condition for the shared graphics hook.</summary>
    private readonly ElevatorBehavior _elevatorBehavior;

    /// <summary>Binds the species to room artwork, palette memory, and door-transition state.</summary>
    internal BlueBrinstarFaceBlockBehavior(EnemyRoomBinding room, EnemyPresentation presentation, ElevatorBehavior elevatorBehavior)
    {
        _elevatorBehavior = elevatorBehavior;
        _room = room;
        _presentation = presentation;
    }

    /// <summary>Enemy frames between shared face-block palette advances.</summary>
    private const ushort BlueBrinstarFaceBlockPalettePeriod = 16;
    /// <summary>Number of frames in the installed auxiliary-color sequence.</summary>
    private const int BlueBrinstarFaceBlockPaletteFrameCount = 8;
    /// <summary>Consecutive OBJ colors replaced on each palette advance.</summary>
    private const int BlueBrinstarFaceBlockAnimatedColorCount = 4;
    /// <summary>First OBJ palette color affected by the graphics-drawn hook.</summary>
    private const int FirstAnimatedFaceBlockPaletteColor = 9;

    /// <summary>Whether an eligible block installed the room-wide graphics hook.</summary>
    /// <remarks>The cartridge stores this animation in the shared enemy-graphics-drawn hook words, not per enemy.
    /// Multiple blocks repeatedly select the same graphics-set palette during main AI, and whichever eligible block
    /// runs last therefore owns this one shared base.</remarks>
    private bool _blueBrinstarFaceBlockPaletteHookInstalled;
    /// <summary>Shared countdown, reset to sixteen after each color update or actor activation.</summary>
    private NativeCountdown _blueBrinstarFaceBlockPaletteTimer;
    /// <summary>Shared eight-frame color index, 0 through 7, intentionally retained when an actor activates.</summary>
    private int _blueBrinstarFaceBlockPaletteFrame;
    /// <summary>OBJ palette row selected by the last eligible block processed by main AI.</summary>
    private int _blueBrinstarFaceBlockPaletteRow;

    /// <summary>Ports <c>BlueBrinstarFaceBlock_Init</c> at <c>$A8:E82E</c>.</summary>
    internal void InitializeBlueBrinstarFaceBlock(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new BlueBrinstarFaceBlockEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // `$E828` is a one-frame neutral map followed by the common sleep instruction. All
        // authored directional lists use the same neutral map as their first frame.
        state.Program = AnimationProgramCursor<BlueBrinstarFaceBlockProgram>.Start(BlueBrinstarFaceBlockProgram.Initial);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        // The initializer always primes the shared countdown, even before Morph Ball exists.
        // It installs the hook only when collected-items bit two is already present. Main AI
        // performs the same item test every frame, allowing an actor in a persistent/custom
        // room to become active immediately after the item is acquired.
        if (HasCollectedMorphBall(samus))
            InstallBlueBrinstarFaceBlockPaletteHook(slot);
        _blueBrinstarFaceBlockPaletteTimer = NativeCountdown.Start(BlueBrinstarFaceBlockPalettePeriod);

        // Native keeps only parameter two bit zero, moved to the sign bit. This looks
        // backwards until main AI is read: activation requires the Samus delta sign to be
        // *different* from this marker.
        state.FacesRight = (slot.Spawn.Population.Parameter2 & 1) != 0;
        state.Activated = false;
        state.LastHorizontalDelta = 0;
    }

    /// <summary>Ports <c>BlueBrinstarFaceBlock_Main</c> at <c>$A8:E8AE</c>.</summary>
    internal void RunBlueBrinstarFaceBlockMain(
        RoomEnemySlot slot,
        BlueBrinstarFaceBlockEnemyState state,
        SamusState? samus)
    {
        // Collected-items, not equipped-items, gates both palette animation and waking. A
        // null Samus owner means the caller has not supplied that native global, so leaving
        // the actor dormant is the only non-invented behavior until a gameplay frame does.
        if (!HasCollectedMorphBall(samus))
            return;

        InstallBlueBrinstarFaceBlockPaletteHook(slot);
        if (state.Activated)
            return;

        ushort verticalDelta = unchecked((ushort)(samus.Position.Y.Whole - slot.Position.Y.Whole));
        if (AbsoluteFaceBlockDelta(verticalDelta) >= slot.Spawn.Population.Parameter1)
            return;

        // Variable B is written after the Y test but before the X test, including on frames
        // that fail horizontal range or side. That write order is visible to debuggers and
        // is preserved rather than collapsing the two comparisons into host geometry.
        ushort horizontalDelta = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        state.LastHorizontalDelta = horizontalDelta;
        if (AbsoluteFaceBlockDelta(horizontalDelta) >= slot.Spawn.Population.Parameter1)
            return;

        bool samusToTheLeft = (horizontalDelta & EnemyWordMasks.SignBit) != 0;
        if (samusToTheLeft == state.FacesRight)
            return;

        state.Program = AnimationProgramCursor<BlueBrinstarFaceBlockProgram>.Start(samusToTheLeft
            ? BlueBrinstarFaceBlockProgram.SamusLeft
            : BlueBrinstarFaceBlockProgram.SamusRight);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Activated = true;

        // Activation restarts the singleton palette hook at the cartridge's sixteen-frame
        // cadence but deliberately does not reset its frame index.
        _blueBrinstarFaceBlockPaletteTimer = NativeCountdown.Start(BlueBrinstarFaceBlockPalettePeriod);
    }

    /// <summary>Uses the collected-item word so unequipping Morph Ball does not put active blocks back to sleep.</summary>
    private static bool HasCollectedMorphBall([NotNullWhen(true)] SamusState? samus) =>
        samus is not null &&
        (samus.CollectedItems & SamusEquipmentFlags.MorphBall) != 0;

    /// <summary>
    /// Reproduces the native signed-magnitude helper over one wrapped 16-bit subtraction.
    /// In particular, $8000 remains $8000 just as the 65C816 negate sequence does.
    /// </summary>
    private static ushort AbsoluteFaceBlockDelta(ushort delta) =>
        unchecked((short)delta) < 0 ? unchecked((ushort)-delta) : delta;

    /// <summary>Selects the block's OBJ palette row as the singleton graphics-hook target.</summary>
    private void InstallBlueBrinstarFaceBlockPaletteHook(RoomEnemySlot slot)
    {
        _blueBrinstarFaceBlockPaletteHookInstalled = true;

        // `((palette_index * 16) & $FF00) >> 8`, followed by `(offset >> 1) + 137`,
        // simplifies to OBJ palette N color nine. Keep that PPU meaning explicit so a
        // debugger sees an actual palette row rather than a mysterious WRAM byte offset.
        _blueBrinstarFaceBlockPaletteRow = (slot.PaletteIndex >> 9) & 7;
    }

    /// <summary>
    /// Ports the graphics-drawn palette hook at <c>$A8:E86C</c>. RoomEnemySystem executes
    /// graphics hooks once per enemy frame after actor processing, the same ownership model
    /// already used by Magdollite and Work Robot palette hooks. WRAM $0795 pauses it during
    /// an enemy door transition; that translated word is exposed as ElevatorDoorTransitionActive.
    /// </summary>
    internal void StepBlueBrinstarFaceBlockPaletteAnimation()
    {
        if (!_blueBrinstarFaceBlockPaletteHookInstalled || _elevatorBehavior.ElevatorDoorTransitionActive)
            return;

        _blueBrinstarFaceBlockPaletteTimer = _blueBrinstarFaceBlockPaletteTimer.Decrement().Remaining;
        if (!_blueBrinstarFaceBlockPaletteTimer.IsZero)
            return;

        _blueBrinstarFaceBlockPaletteTimer = NativeCountdown.Start(BlueBrinstarFaceBlockPalettePeriod);
        int frame = _blueBrinstarFaceBlockPaletteFrame & (BlueBrinstarFaceBlockPaletteFrameCount - 1);
        var colors = _room.Artwork?.AuxiliaryColors ?? throw new InvalidDataException(
            "Face-block palette animation requires installed artwork.");
        CgramPalette animated = _presentation.CgramRows.ObjectRow(_blueBrinstarFaceBlockPaletteRow)
            .Slice(FirstAnimatedFaceBlockPaletteColor, BlueBrinstarFaceBlockAnimatedColorCount);
        for (int color = 0; color < BlueBrinstarFaceBlockAnimatedColorCount; color++)
        {
            animated.Write(
                color,
                colors.Resolve(SuperMetroid.Core.Enemies.Artwork.EnemyAuxiliaryPalette.FaceBlock, frame, color));
        }

        // The index cycles through the eight colors, as the cartridge's increment and low-bits mask do.
        _blueBrinstarFaceBlockPaletteFrame = (_blueBrinstarFaceBlockPaletteFrame + 1) % BlueBrinstarFaceBlockPaletteFrameCount;
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
