using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Kzan.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>The Kzan species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class KzanBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.KzanBottom, EnemyDefinitionId.KzanTop];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new KzanEntity(this, slot, common);

    /// <summary>Lets the lower actor mirror the immediately preceding top actor's position.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Uses native slot adjacency to couple each Kzan bottom to its top.</summary>
    internal KzanBehavior(EnemySlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>NTSC pause at the bottom before the top begins to rise.</summary>
    private const ushort KzanNtscRiseWaitFrames = 64;
    /// <summary>Fractional pixels subtracted each rising actor update.</summary>
    private const ushort KzanNtscRisingSubspeed = unchecked((ushort)-32768);
    /// <summary>Last speed-table offset permitted during falling acceleration.</summary>
    private const int KzanMaximumFallingSpeedTableOffset = 512;

    /// <summary>Speed-table bytes per falling step: one logical speed's positive and negative pair.</summary>
    private const int KzanFallingSpeedTableStride = 8;

    /// <summary>The library-two landing sound published when the falling top reaches its target.</summary>
    private static readonly SoundEffectId KzanLandingSound =
        SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, EnemySoundEffectDefinitions.KzanLandingSoundEffect);

    /// <summary>The words each physical slot keeps across Kzan occupants for the current room.</summary>
    private readonly KzanExtensionWords[] _extensions = NewExtensions();

    /// <summary>Fresh, zeroed extension words for every physical slot.</summary>
    private static KzanExtensionWords[] NewExtensions() =>
        [.. Enumerable.Range(0, RoomEnemySystem.MaximumEnemyCount).Select(_ => new KzanExtensionWords())];

    /// <summary>Initializes the top's idle list, wrapped speed-table index, fall distance, and low-byte wait timer from its two parameters.</summary>
    internal void InitializeKzanTop(RoomEnemySlot slot)
    {
        var state = new KzanEnemyState(_extensions[slot.SlotIndex]);
        slot.SetSpeciesState(state, state.ClearWithRecord);

        state.Program = AnimationProgramCursor<KzanProgram>.Start(KzanProgram.Idle);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Function = KzanEnemyFunction.WaitingToFall;

        // init0's low byte is a logical speed-table index; the cartridge multiplies it by
        // eight (its byte offset, at most 2040) before reading the table. Retail values
        // include 64, selecting its final NTSC entry.
        int speedTableOffset = SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter1) * KzanFallingSpeedTableStride;
        state.FallingYSpeedTableIndex = speedTableOffset;
        (short initialWhole, ushort initialFraction) = EnemyMath.ReadLinearEnemySpeed(checked((ushort)speedTableOffset));
        state.InitialFallingSpeed = SnesSignedSixteenSixteen.FromParts(initialWhole, initialFraction);

        // init1 packs two unrelated bytes: the high byte is fall distance in pixels and the
        // low byte is the top wait countdown. A zero countdown deliberately wraps to $FFFF
        // on its first DEC rather than triggering immediately.
        var spawn = new Position16<RoomPixels>(0, slot.Position.Y.Whole);
        state.FallingTarget = spawn + new Offset16<RoomPixels>(0, SnesLittleEndian.HighByte(slot.Spawn.Population.Parameter2));
        state.RisingTarget = spawn;
        state.FallWaitTimerResetValue = SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter2);
        state.FallWaitTimer = NativeCountdown.Start(state.FallWaitTimerResetValue);
    }

    /// <summary>Places the lower actor twelve pixels below the preceding top during setup.</summary>
    private void InitializeKzanBottom(RoomEnemySlot slot) => FollowKzanTop(slot);

    /// <summary>Keeps the lower actor twelve pixels below the top on every enemy update.</summary>
    private void RunKzanBottomMain(RoomEnemySlot slot) => FollowKzanTop(slot);

    /// <summary>
    /// Reproduces the literal <c>Enemy[-1]</c> reads. Retail populations always place the
    /// bottom immediately after its top; only slot zero is rejected because there is no
    /// managed representation of the unrelated WRAM that malformed data would under-read.
    /// </summary>
    private void FollowKzanTop(RoomEnemySlot bottom)
    {
        if (bottom.SlotIndex == 0)
            throw new InvalidDataException("Kzan bottom cannot occupy enemy slot zero.");

        RoomEnemySlot precedingSlot = _slotTable._slots[bottom.SlotIndex - 1];
        bottom.Position = bottom.Position.WithWhole(new(precedingSlot.Position.X.Whole, unchecked((ushort)(precedingSlot.Position.Y.Whole + 12))));
    }

    /// <summary>Dispatches the top actor through fall wait, accelerated descent, rise wait, and half-pixel ascent.</summary>
    private void RunKzanTopMain(
        RoomEnemySlot slot,
        KzanEnemyState state,
        SamusState? samus)
    {
        switch (state.Function)
        {
            case KzanEnemyFunction.WaitingToFall:
                RunKzanWaitingToFall(slot, state);
                return;

            case KzanEnemyFunction.Falling:
                RunKzanFalling(slot, state, samus);
                return;

            case KzanEnemyFunction.WaitingToRise:
                RunKzanWaitingToRise(state);
                return;

            case KzanEnemyFunction.Rising:
                RunKzanRising(slot, state, samus);
                return;

            default:
                throw new InvalidDataException(
                    $"Kzan function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Decrements the wrapping top wait and resets the fall-speed index when descent begins.</summary>
    private static void RunKzanWaitingToFall(RoomEnemySlot slot, KzanEnemyState state)
    {
        state.FallWaitTimer = state.FallWaitTimer.Decrement().Remaining;
        if (!state.FallWaitTimer.IsZero)
            return;

        state.FallWaitTimer = NativeCountdown.Start(state.FallWaitTimerResetValue);
        state.FallingYSpeedTableIndex = SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter1) * KzanFallingSpeedTableStride;
        state.Function = KzanEnemyFunction.Falling;
    }

    /// <summary>Applies table-driven downward speed, clamps to the target using signed wrapped comparison, carries Samus, and advances the speed index.</summary>
    private void RunKzanFalling(
        RoomEnemySlot slot,
        KzanEnemyState state,
        SamusState? samus)
    {
        state.PreviousPosition = slot.Position.Whole;
        (short wholeSpeed, ushort fractionalSpeed) =
            EnemyMath.ReadLinearEnemySpeed(checked((ushort)state.FallingYSpeedTableIndex));
        AddKzanY(slot, wholeSpeed, fractionalSpeed);

        // CMP/BMI tests only the sign of the wrapped subtraction, which the spatial difference
        // keeps: coordinates near the room's edge retain the same cartridge behavior.
        if ((slot.Position.Whole - state.FallingTarget).Y >= 0)
        {
            state.RiseWaitTimer = NativeCountdown.Start(KzanNtscRiseWaitFrames);
            state.Function = KzanEnemyFunction.WaitingToRise;
            slot.Position = slot.Position.WithWholeY(state.FallingTarget.Y);
            LastKzanSoundEffect = KzanLandingSound;
        }

        CarrySamusWithKzanIfTouchingFromBelow(slot, state, samus);

        // The cartridge caps the offset by the sign of its word difference from the cap; both
        // stay far below the word's sign bit, so that is the ordinary minimum.
        state.FallingYSpeedTableIndex = Math.Min(
            state.FallingYSpeedTableIndex + KzanFallingSpeedTableStride,
            KzanMaximumFallingSpeedTableOffset);
    }

    /// <summary>Counts down the 64-frame NTSC landing hold before switching to ascent.</summary>
    private static void RunKzanWaitingToRise(KzanEnemyState state)
    {
        state.RiseWaitTimer = state.RiseWaitTimer.Decrement().Remaining;
        if (state.RiseWaitTimer.IsZero)
            state.Function = KzanEnemyFunction.Rising;
    }

    /// <summary>Subtracts one-half pixel per update, stops at the original height, and carries Samus by the whole-pixel step.</summary>
    private static void RunKzanRising(
        RoomEnemySlot slot,
        KzanEnemyState state,
        SamusState? samus)
    {
        state.PreviousPosition = slot.Position.Whole;

        // The NTSC build rises by exactly 0.8000h pixels per actor frame. Perform the same
        // split SEC/SBC so a fractional borrow decrements the whole position word.
        SubtractKzanY(slot, wholeSpeed: 0, KzanNtscRisingSubspeed);
        if ((slot.Position.Whole - state.RisingTarget).Y < 0)
        {
            state.Function = KzanEnemyFunction.WaitingToFall;
            slot.Position = slot.Position.WithWholeY(state.RisingTarget.Y);
        }

        CarrySamusWithKzanIfTouchingFromBelow(slot, state, samus);
    }

    /// <summary>Adds the native <c>[$14].[$12]</c> split pair to enemy Y.</summary>
    private static void AddKzanY(
        RoomEnemySlot slot,
        short wholeSpeed,
        ushort fractionalSpeed)
    {
        uint fractionalSum = (uint)slot.Position.Y.Fraction + fractionalSpeed;
        slot.Position = slot.Position.WithFractionY(unchecked((ushort)fractionalSum));
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(
            slot.Position.Y.Whole + wholeSpeed + (fractionalSum > ushort.MaxValue ? 1 : 0))));
    }

    /// <summary>Subtracts the native <c>[$14].[$12]</c> split pair from enemy Y.</summary>
    private static void SubtractKzanY(
        RoomEnemySlot slot,
        ushort wholeSpeed,
        ushort fractionalSpeed)
    {
        bool borrowed = slot.Position.Y.Fraction < fractionalSpeed;
        slot.Position = slot.Position.WithFractionY(unchecked((ushort)(slot.Position.Y.Fraction - fractionalSpeed)));
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(
            slot.Position.Y.Whole - wholeSpeed - (borrowed ? 1 : 0))));
    }

    /// <summary>
    /// Ports Kzan's private copy of <c>CheckIfEnemyIsTouchingSamusFromBelow</c>. Its only
    /// difference from the common helper is the five-pixel shifted Y sample. Horizontal
    /// overlap is strict; vertical tangency is accepted after the shifted center test.
    /// </summary>
    private static bool IsKzanTouchingSamusFromBelow(
        RoomEnemySlot slot,
        SamusState samus)
    {
        int xDistance = Math.Abs(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)));
        if (xDistance >= samus.Kinematics.Radius.X + slot.Radius.X)
            return false;

        short shiftedYDelta = unchecked((short)(
            unchecked((ushort)(samus.Position.Y.Whole + 5 - slot.Position.Y.Whole))));
        if (shiftedYDelta >= 0)
            return false;

        int yDistance = Math.Abs((int)shiftedYDelta);
        if (yDistance < samus.Kinematics.Radius.Y)
            return true;

        return yDistance - samus.Kinematics.Radius.Y <= slot.Radius.Y;
    }

    /// <summary>
    /// Adds only the accepted whole-pixel Kzan delta to WRAM-equivalent <c>$0B5C</c>.
    /// Native code deliberately leaves extra Y subdisplacement untouched.
    /// </summary>
    private static void CarrySamusWithKzanIfTouchingFromBelow(
        RoomEnemySlot slot,
        KzanEnemyState state,
        SamusState? samus)
    {
        if (samus is null || !IsKzanTouchingSamusFromBelow(slot, samus))
            return;

        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, (slot.Position.Whole - state.PreviousPosition).Y);
    }

    /// <summary>Rejects a top AI call before its matching per-slot state has been initialized.</summary>
    internal static KzanEnemyState RequireKzanState(RoomEnemySlot slot) =>
        slot.SpeciesState<KzanEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Kzan-top state.");

    /// <summary>Advances Kzan's typed animation.</summary>
    internal static void AdvanceKzanTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireKzanState(slot);
        AnimationProgramCursor<KzanProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, KzanAnimations.Programs, ref cursor, KzanFrameDefinitions.ShownFrames.Of);
        state.Program = cursor;
    }

    /// <summary>Ports KzanTop's initialization AI.</summary>
    internal void RunKzanTopInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeKzanTop(slot);
    }

    /// <summary>Ports KzanBottom's initialization AI.</summary>
    internal void RunKzanBottomInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeKzanBottom(slot);
    }

    /// <summary>Ports KzanTop's main AI.</summary>
    internal void RunKzanTopMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunKzanTopMain(slot, RequireKzanState(slot), frame.Samus);
    }

    /// <summary>Ports KzanBottom's main AI.</summary>
    internal void RunKzanBottomMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunKzanBottomMain(slot);
    }

    /// <summary>Last library-two Kzan landing sound this enemy pass, using the Max6 queue entry; cleared at the next enemy-frame boundary.</summary>
    public SoundEffectId? LastKzanSoundEffect { get; private set; }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastKzanSoundEffect = null;
        for (int slotIndex = 0; slotIndex < _extensions.Length; slotIndex++)
            _extensions[slotIndex] = new KzanExtensionWords();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastKzanSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastKzanSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: 6);
    }
}
