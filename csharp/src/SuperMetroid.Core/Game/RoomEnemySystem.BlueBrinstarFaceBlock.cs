namespace SuperMetroid.Core.Game;

/// <summary>
/// The side from which Samus may wake a Blue Brinstar face block. This is derived from the
/// normalized high-bit marker that retail stores back into population parameter two; it is
/// not a new host-only behavior selector.
/// </summary>
public enum BlueBrinstarFaceBlockActivationSide
{
    Left,
    Right,
}

/// <summary>
/// Debugger-facing names for the two private words used by <c>$A8:E8AE</c>. The activation
/// latch and last horizontal delta remain in the ordinary enemy slot, so watches still map
/// directly to native variables A and B instead of drifting into a parallel object model.
/// </summary>
public sealed class BlueBrinstarFaceBlockEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal BlueBrinstarFaceBlockEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Native variable A. Once set, the face has selected its directional animation and the
    /// main AI never performs the proximity test again.
    /// </summary>
    public bool Activated
    {
        get => _slot.VariableA != 0;
        internal set => _slot.VariableA = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>
    /// Native variable B, written only after Samus passes the vertical range test. Retaining
    /// the wrapped 16-bit subtraction makes left-side deltas directly comparable to WRAM.
    /// </summary>
    public ushort LastHorizontalDelta
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>
    /// Parameter two after the initializer reduces its original bit zero to either $0000
    /// (Samus must be left) or $8000 (Samus must be right).
    /// </summary>
    public BlueBrinstarFaceBlockActivationSide ActivationSide =>
        _slot.Parameter2 == 0
            ? BlueBrinstarFaceBlockActivationSide.Left
            : BlueBrinstarFaceBlockActivationSide.Right;
}

/// <summary>
/// Literal translation of the Blue Brinstar face-block enemy <c>$EA7F</c> from
/// <c>$A8:E7AC-$E92B</c>. These actors never move or attack. After Morph Ball has been
/// collected, selected records wake once when Samus enters a strict square range from the
/// authored side. A room-global graphics-drawn hook cycles four OBJ-palette colors for all
/// instances, while touch is a literal no-op and shots pass through without damaging them.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort BlueBrinstarFaceBlockDefinition = 0xea7f;
    internal const ushort BlueBrinstarFaceBlockShotAi =
        EnemyAiCodePointers.BankA8.BlueBrinstarFaceBlockShot;

    private const ushort BlueBrinstarFaceBlockMorphBallItemMask = 0x0004;
    private const ushort BlueBrinstarFaceBlockInitialInstructionList = 0xe828;
    private const ushort BlueBrinstarFaceBlockSamusLeftInstructionList = 0xe80c;
    private const ushort BlueBrinstarFaceBlockSamusRightInstructionList = 0xe81a;
    private const int BlueBrinstarFaceBlockPaletteTable = 0xa8e7cc;
    private const ushort BlueBrinstarFaceBlockPalettePeriod = 16;
    private const int BlueBrinstarFaceBlockPaletteFrameCount = 8;
    private const int BlueBrinstarFaceBlockAnimatedColorCount = 4;
    private const int FirstObjPaletteColor = 128;
    private const int ColorsPerObjPalette = 16;
    private const int FirstAnimatedFaceBlockPaletteColor = 9;

    private readonly BlueBrinstarFaceBlockEnemyState?[] _blueBrinstarFaceBlockStates =
        new BlueBrinstarFaceBlockEnemyState?[MaximumEnemyCount];

    // The cartridge stores this animation in the shared enemy-graphics-drawn hook words,
    // not per enemy. Multiple blocks repeatedly select the same graphics-set palette during
    // main AI, and whichever eligible block runs last therefore owns this one shared base.
    private bool _blueBrinstarFaceBlockPaletteHookInstalled;
    private ushort _blueBrinstarFaceBlockPaletteTimer;
    private ushort _blueBrinstarFaceBlockPaletteFrame;
    private int _blueBrinstarFaceBlockPaletteDestination;

    /// <summary>Typed face-block state in fixed physical enemy-slot order.</summary>
    public IReadOnlyList<BlueBrinstarFaceBlockEnemyState?> BlueBrinstarFaceBlockStates =>
        _blueBrinstarFaceBlockStates;

    /// <summary>Clears actor and singleton graphics-hook state at room-load time.</summary>
    private void ResetBlueBrinstarFaceBlockRoomState()
    {
        Array.Clear(_blueBrinstarFaceBlockStates);
        _blueBrinstarFaceBlockPaletteHookInstalled = false;
        _blueBrinstarFaceBlockPaletteTimer = 0;
        _blueBrinstarFaceBlockPaletteFrame = 0;
        _blueBrinstarFaceBlockPaletteDestination = 0;
    }

    /// <summary>Ports <c>BlueBrinstarFaceBlock_Init</c> at <c>$A8:E82E</c>.</summary>
    private void InitializeBlueBrinstarFaceBlock(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new BlueBrinstarFaceBlockEnemyState(slot);
        _blueBrinstarFaceBlockStates[slot.SlotIndex] = state;

        // `$E828` is a one-frame neutral map followed by the common sleep instruction. All
        // authored directional lists use the same neutral map as their first frame.
        slot.CurrentInstruction = BlueBrinstarFaceBlockInitialInstructionList;
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
    private void RunBlueBrinstarFaceBlockMain(
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

        ushort verticalDelta = unchecked((ushort)(samus!.YPosition - slot.YPosition));
        if (AbsoluteFaceBlockDelta(verticalDelta) >= slot.Parameter1)
            return;

        // Variable B is written after the Y test but before the X test, including on frames
        // that fail horizontal range or side. That write order is visible to debuggers and
        // is preserved rather than collapsing the two comparisons into host geometry.
        ushort horizontalDelta = unchecked((ushort)(samus.XPosition - slot.XPosition));
        state.LastHorizontalDelta = horizontalDelta;
        if (AbsoluteFaceBlockDelta(horizontalDelta) >= slot.Parameter1)
            return;

        ushort horizontalSign = unchecked((ushort)(horizontalDelta & 0x8000));
        if (horizontalSign == slot.Parameter2)
            return;

        slot.CurrentInstruction = horizontalSign != 0
            ? BlueBrinstarFaceBlockSamusLeftInstructionList
            : BlueBrinstarFaceBlockSamusRightInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        state.Activated = true;

        // Activation restarts the singleton palette hook at the cartridge's sixteen-frame
        // cadence but deliberately does not reset its frame index.
        _blueBrinstarFaceBlockPaletteTimer = BlueBrinstarFaceBlockPalettePeriod;
    }

    private static bool HasCollectedMorphBall(SamusState? samus) =>
        samus is not null &&
        (samus.CollectedItems & BlueBrinstarFaceBlockMorphBallItemMask) != 0;

    /// <summary>
    /// Reproduces the native signed-magnitude helper over one wrapped 16-bit subtraction.
    /// In particular, $8000 remains $8000 just as the 65C816 negate sequence does.
    /// </summary>
    private static ushort AbsoluteFaceBlockDelta(ushort delta) =>
        unchecked((short)delta) < 0 ? unchecked((ushort)-delta) : delta;

    private void InstallBlueBrinstarFaceBlockPaletteHook(RoomEnemySlot slot)
    {
        _blueBrinstarFaceBlockPaletteHookInstalled = true;

        // `((palette_index * 16) & $FF00) >> 8`, followed by `(offset >> 1) + 137`,
        // simplifies to OBJ palette N color nine. Keep that PPU meaning explicit so a
        // debugger sees an actual CGRAM index rather than a mysterious WRAM byte offset.
        int objPalette = (slot.PaletteIndex >> 9) & 7;
        _blueBrinstarFaceBlockPaletteDestination =
            FirstObjPaletteColor + objPalette * ColorsPerObjPalette +
            FirstAnimatedFaceBlockPaletteColor;
    }

    /// <summary>
    /// Ports the graphics-drawn palette hook at <c>$A8:E86C</c>. RoomEnemySystem executes
    /// graphics hooks once per enemy frame after actor processing, the same ownership model
    /// already used by Magdollite and Work Robot palette hooks. WRAM $0795 pauses it during
    /// an enemy door transition; that translated word is exposed as ElevatorDoorTransitionActive.
    /// </summary>
    private void StepBlueBrinstarFaceBlockPaletteAnimation()
    {
        if (!_blueBrinstarFaceBlockPaletteHookInstalled || ElevatorDoorTransitionActive)
            return;

        _blueBrinstarFaceBlockPaletteTimer = unchecked((ushort)(
            _blueBrinstarFaceBlockPaletteTimer - 1));
        if (_blueBrinstarFaceBlockPaletteTimer != 0)
            return;

        _blueBrinstarFaceBlockPaletteTimer = BlueBrinstarFaceBlockPalettePeriod;
        int source = BlueBrinstarFaceBlockPaletteTable +
            (_blueBrinstarFaceBlockPaletteFrame & (BlueBrinstarFaceBlockPaletteFrameCount - 1)) *
            BlueBrinstarFaceBlockAnimatedColorCount * 2;
        for (int color = 0; color < BlueBrinstarFaceBlockAnimatedColorCount; color++)
        {
            _cgram!.SetColor(
                _blueBrinstarFaceBlockPaletteDestination + color,
                ReadWord(_bus!, source + color * 2));
        }

        // Native masks only the low byte after incrementing. Because the value is already
        // constrained to 0..7, this is exactly its `(index + 1) & 7` result.
        _blueBrinstarFaceBlockPaletteFrame = unchecked((ushort)(
            (_blueBrinstarFaceBlockPaletteFrame + 1) &
            (BlueBrinstarFaceBlockPaletteFrameCount - 1)));
    }

    private BlueBrinstarFaceBlockEnemyState RequireBlueBrinstarFaceBlockState(
        RoomEnemySlot slot) =>
        _blueBrinstarFaceBlockStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Blue Brinstar face-block state.");
}
