namespace SuperMetroid.Core.Game;

/// <summary>The blue Brinstar face block species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class BlueBrinstarFaceBlockBehavior : IEnemySpecies
{
    /// <summary>Stores room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Stores presentation.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Stores elevator behavior.</summary>
    private readonly ElevatorBehavior _elevatorBehavior;

    /// <summary>Initializes a new blue brinstar face block behavior instance.</summary>
    internal BlueBrinstarFaceBlockBehavior(EnemyRoomBinding room, EnemyPresentation presentation, ElevatorBehavior elevatorBehavior)
    {
        _elevatorBehavior = elevatorBehavior;
        _room = room;
        _presentation = presentation;
    }

    /// <summary>Stores blue brinstar face block morph ball item mask.</summary>
    private const ushort BlueBrinstarFaceBlockMorphBallItemMask = 0x0004;
    /// <summary>Stores blue brinstar face block palette period.</summary>
    private const ushort BlueBrinstarFaceBlockPalettePeriod = 16;
    /// <summary>Stores blue brinstar face block palette frame count.</summary>
    private const int BlueBrinstarFaceBlockPaletteFrameCount = 8;
    /// <summary>Stores blue brinstar face block animated color count.</summary>
    private const int BlueBrinstarFaceBlockAnimatedColorCount = 4;
    /// <summary>Stores first animated face block palette color.</summary>
    private const int FirstAnimatedFaceBlockPaletteColor = 9;

    /// <summary>Stores blue brinstar face block states.</summary>
    internal readonly BlueBrinstarFaceBlockEnemyState?[] _blueBrinstarFaceBlockStates =
        new BlueBrinstarFaceBlockEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    // The cartridge stores this animation in the shared enemy-graphics-drawn hook words,
    // not per enemy. Multiple blocks repeatedly select the same graphics-set palette during
    // main AI, and whichever eligible block runs last therefore owns this one shared base.
    /// <summary>Stores blue brinstar face block palette hook installed.</summary>
    private bool _blueBrinstarFaceBlockPaletteHookInstalled;
    /// <summary>Stores blue brinstar face block palette timer.</summary>
    private ushort _blueBrinstarFaceBlockPaletteTimer;
    /// <summary>Stores blue brinstar face block palette frame.</summary>
    private ushort _blueBrinstarFaceBlockPaletteFrame;
    /// <summary>Stores blue brinstar face block palette row.</summary>
    private int _blueBrinstarFaceBlockPaletteRow;

    /// <summary>Clears actor and singleton graphics-hook state at room-load time.</summary>
    internal void ResetBlueBrinstarFaceBlockRoomState()
    {
        Array.Clear(_blueBrinstarFaceBlockStates);
        _blueBrinstarFaceBlockPaletteHookInstalled = false;
        _blueBrinstarFaceBlockPaletteTimer = 0;
        _blueBrinstarFaceBlockPaletteFrame = 0;
        _blueBrinstarFaceBlockPaletteRow = 0;
    }

    /// <summary>Ports <c>BlueBrinstarFaceBlock_Init</c> at <c>$A8:E82E</c>.</summary>
    internal void InitializeBlueBrinstarFaceBlock(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new BlueBrinstarFaceBlockEnemyState(slot);
        _blueBrinstarFaceBlockStates[slot.SlotIndex] = state;

        // `$E828` is a one-frame neutral map followed by the common sleep instruction. All
        // authored directional lists use the same neutral map as their first frame.
        slot.CurrentInstruction = BlueBrinstarFaceBlockInstructionProgramDefinitions.Initial;
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        // The initializer always primes the shared countdown, even before Morph Ball exists.
        // It installs the hook only when collected-items bit two is already present. Main AI
        // performs the same item test every frame, allowing an actor in a persistent/custom
        // room to become active immediately after the item is acquired.
        if (HasCollectedMorphBall(samus))
            InstallBlueBrinstarFaceBlockPaletteHook(slot);
        _blueBrinstarFaceBlockPaletteTimer = BlueBrinstarFaceBlockPalettePeriod;

        // Native keeps only parameter two bit zero and moves it to the sign bit. This looks
        // backwards until main AI is read: activation requires the Samus delta sign to be
        // *different* from this marker, so zero means left and $8000 means right.
        slot.Parameter2 = (slot.Parameter2 & 1) != 0 ? (ushort)0x8000 : (ushort)0;
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

        ushort verticalDelta = unchecked((ushort)(samus!.Position.Y.Whole - slot.Position.Y.Whole));
        if (AbsoluteFaceBlockDelta(verticalDelta) >= slot.Parameter1)
            return;

        // Variable B is written after the Y test but before the X test, including on frames
        // that fail horizontal range or side. That write order is visible to debuggers and
        // is preserved rather than collapsing the two comparisons into host geometry.
        ushort horizontalDelta = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        state.LastHorizontalDelta = horizontalDelta;
        if (AbsoluteFaceBlockDelta(horizontalDelta) >= slot.Parameter1)
            return;

        ushort horizontalSign = unchecked((ushort)(horizontalDelta & 0x8000));
        if (horizontalSign == slot.Parameter2)
            return;

        slot.CurrentInstruction = horizontalSign != 0
            ? BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusLeft
            : BlueBrinstarFaceBlockInstructionProgramDefinitions.SamusRight;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        state.Activated = true;

        // Activation restarts the singleton palette hook at the cartridge's sixteen-frame
        // cadence but deliberately does not reset its frame index.
        _blueBrinstarFaceBlockPaletteTimer = BlueBrinstarFaceBlockPalettePeriod;
    }

    /// <summary>Has collected morph ball.</summary>
    private static bool HasCollectedMorphBall(SamusState? samus) =>
        samus is not null &&
        (samus.CollectedItems & BlueBrinstarFaceBlockMorphBallItemMask) != 0;

    /// <summary>
    /// Reproduces the native signed-magnitude helper over one wrapped 16-bit subtraction.
    /// In particular, $8000 remains $8000 just as the 65C816 negate sequence does.
    /// </summary>
    private static ushort AbsoluteFaceBlockDelta(ushort delta) =>
        unchecked((short)delta) < 0 ? unchecked((ushort)-delta) : delta;

    /// <summary>Installs blue brinstar face block palette hook.</summary>
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

        _blueBrinstarFaceBlockPaletteTimer = unchecked((ushort)(
            _blueBrinstarFaceBlockPaletteTimer - 1));
        if (_blueBrinstarFaceBlockPaletteTimer != 0)
            return;

        _blueBrinstarFaceBlockPaletteTimer = BlueBrinstarFaceBlockPalettePeriod;
        int frame = _blueBrinstarFaceBlockPaletteFrame & (BlueBrinstarFaceBlockPaletteFrameCount - 1);
        var colors = _room.Artwork?.AuxiliaryColors ?? throw new InvalidDataException(
            "Face-block palette animation requires installed artwork.");
        CgramPalette animated = _presentation.CgramRows.ObjectRow(_blueBrinstarFaceBlockPaletteRow)
            .Slice(FirstAnimatedFaceBlockPaletteColor, BlueBrinstarFaceBlockAnimatedColorCount);
        for (int color = 0; color < BlueBrinstarFaceBlockAnimatedColorCount; color++)
        {
            animated.Write(
                color,
                colors.Resolve(SuperMetroid.Core.Assets.EnemyAuxiliaryPalette.FaceBlock, frame, color));
        }

        // Native masks only the low byte after incrementing. Because the value is already
        // constrained to 0..7, this is exactly its `(index + 1) & 7` result.
        _blueBrinstarFaceBlockPaletteFrame = unchecked((ushort)(
            (_blueBrinstarFaceBlockPaletteFrame + 1) &
            (BlueBrinstarFaceBlockPaletteFrameCount - 1)));
    }

    /// <summary>Requires blue brinstar face block state.</summary>
    private BlueBrinstarFaceBlockEnemyState RequireBlueBrinstarFaceBlockState(
        RoomEnemySlot slot) =>
        _blueBrinstarFaceBlockStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Blue Brinstar face-block state.");

    /// <summary>Registers blue brinstar face block initialization ai.</summary>
    internal void RegisterBlueBrinstarFaceBlockInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_FaceBlock, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.FaceBlock, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeBlueBrinstarFaceBlock(slot, samus);
        });
    }

    /// <summary>Registers blue brinstar face block main ai.</summary>
    internal void RegisterBlueBrinstarFaceBlockMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_FaceBlock, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.FaceBlock, (slot, frame, nmiFrameCounter8) =>
        {
            RunBlueBrinstarFaceBlockMain(
                slot,
                RequireBlueBrinstarFaceBlockState(slot),
                frame.Samus);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetBlueBrinstarFaceBlockRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
