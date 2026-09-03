using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The one-frame movement selector stored in Mochtroid variable F. Main AI clears this word
/// before dispatch, so touch collision must republish <see cref="TouchingSamus"/> every frame
/// for the creature to remain latched on.
/// </summary>
public enum MochtroidMovementMode : ushort
{
    NotTouchingSamus = 0,
    TouchingSamus = 1,
    Shaking = 2,
}

/// <summary>
/// Named view of the common enemy words and the three parallel extension-array words used by
/// $A3:A77D-$A9A8. Exposing these values makes the attachment clock and 16.16 steering easy to
/// inspect in C# without pretending the native arrays were part of the common $40-byte slot.
/// </summary>
public sealed class MochtroidEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal MochtroidEnemyState(RoomEnemySlot slot) => _slot = slot;

    public ushort XSubvelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    public short XVelocity
    {
        get => unchecked((short)_slot.VariableB);
        internal set => _slot.VariableB = unchecked((ushort)value);
    }

    public ushort YSubvelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    public short YVelocity
    {
        get => unchecked((short)_slot.VariableD);
        internal set => _slot.VariableD = unchecked((ushort)value);
    }

    /// <summary>Variable E; used only by the shipped-but-unreferenced shake mode.</summary>
    public ushort ShakeTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public MochtroidMovementMode MovementMode
    {
        get => (MochtroidMovementMode)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>$7E:7802 parallel word used by the instruction-list change detector.</summary>
    public ushort InstalledInstructionList { get; internal set; }

    /// <summary>$7E:8800 parallel word; contact damage occurs when this reaches 80.</summary>
    public ushort AttachmentDamageTimer { get; internal set; }

    public int SignedXVelocity => unchecked((XVelocity << 16) | XSubvelocity);
    public int SignedYVelocity => unchecked((YVelocity << 16) | YSubvelocity);
}

/// <summary>
/// Literal translation of Mochtroid enemy AI $A3:A77D-$A9A8. “Mocktroid” appears in one
/// disassembly table label, but the cartridge header and player-facing name use Mochtroid.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort MochtroidDefinition = 0xd8ff;

    private const ushort MochtroidIdleInstructionList = 0xa745;
    private const ushort MochtroidAttachedInstructionList = 0xa759;
    private const ushort MochtroidAttachmentDamagePeriod = 0x0050;
    private const int MochtroidMaximumVelocity = 3;
    private const int MochtroidAttachedVelocity = 1 << 16;

    private readonly MochtroidEnemyState?[] _mochtroidStates =
        new MochtroidEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for every physical slot currently owned by a Mochtroid.</summary>
    public IReadOnlyList<MochtroidEnemyState?> MochtroidStates => _mochtroidStates;

    /// <summary>Ports <c>InitAI_Mochtroid</c> at $A3:A77D.</summary>
    private void InitializeMochtroid(RoomEnemySlot slot)
    {
        // Native room WRAM clearing initializes both parallel words to zero. Installing the
        // first nonzero list is important: the original's equality shortcut is also the
        // source of its documented respawning-Mochtroid crash when stale extension RAM is
        // retained. Ordinary room loads start clean and therefore take the safe path.
        var state = new MochtroidEnemyState(slot)
        {
            InstalledInstructionList = 0,
            AttachmentDamageTimer = 0,
            MovementMode = MochtroidMovementMode.NotTouchingSamus,
        };
        _mochtroidStates[slot.SlotIndex] = state;
        slot.Layer = 2;
        SetMochtroidInstructionList(slot, state, MochtroidIdleInstructionList);
    }

    /// <summary>Ports the reset-before-dispatch behavior of <c>MainAI_Mochtroid</c>.</summary>
    private void RunMochtroidMain(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Mochtroid AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Mochtroid movement requires room level data.");

        MochtroidEnemyState state = RequireMochtroidState(slot);
        MochtroidMovementMode selectedMode = state.MovementMode;
        state.MovementMode = MochtroidMovementMode.NotTouchingSamus;
        switch (selectedMode)
        {
            case MochtroidMovementMode.NotTouchingSamus:
                RunMochtroidFreeFlight(slot, state, samus, level);
                return;
            case MochtroidMovementMode.TouchingSamus:
                RunMochtroidAttachedFlight(slot, state, samus, level);
                return;
            case MochtroidMovementMode.Shaking:
                RunMochtroidShake(slot, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Mochtroid movement index {(ushort)selectedMode} exceeds its three-entry ROM table.");
        }
    }

    private void RunMochtroidFreeFlight(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // The cartridge treats each coordinate difference as signed, divides it by four,
        // shifts that result left eight fractional bits, and subtracts it from velocity.
        // This is a proportional attraction toward Samus—not a fixed-speed seek—and the
        // accumulated fraction is observable before either whole component reaches ±3.
        int yDifference = unchecked((short)(slot.YPosition - samus.YPosition));
        int yVelocity = unchecked(state.SignedYVelocity - ((yDifference >> 2) << 8));
        yVelocity = ClampMochtroidVelocity(yVelocity);
        StoreMochtroidYVelocity(state, yVelocity);
        if (MoveEnemyVertically(level, slot, yVelocity))
            StoreMochtroidYVelocity(state, 0);

        // Y is intentionally resolved first. Horizontal collision therefore sees the
        // vertically adjusted position, matching $A3:A80D followed by $A3:A87C.
        int xDifference = unchecked((short)(slot.XPosition - samus.XPosition));
        int xVelocity = unchecked(state.SignedXVelocity - ((xDifference >> 2) << 8));
        xVelocity = ClampMochtroidVelocity(xVelocity);
        StoreMochtroidXVelocity(state, xVelocity);
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, xVelocity))
            StoreMochtroidXVelocity(state, 0);

        SetMochtroidInstructionList(slot, state, MochtroidIdleInstructionList);
    }

    private static int ClampMochtroidVelocity(int velocity)
    {
        short whole = unchecked((short)(velocity >> 16));
        if (whole >= MochtroidMaximumVelocity)
            return MochtroidMaximumVelocity << 16;
        if (whole < -MochtroidMaximumVelocity)
            return -MochtroidMaximumVelocity << 16;
        return velocity;
    }

    private void RunMochtroidAttachedFlight(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // Attached mode discards proportional acceleration. It moves exactly one pixel per
        // axis toward Samus, with X collision resolved before Y and ignored return values.
        int xVelocity = slot.XPosition == samus.XPosition
            ? 0
            : unchecked((short)(slot.XPosition - samus.XPosition)) > 0
                ? -MochtroidAttachedVelocity
                : MochtroidAttachedVelocity;
        StoreMochtroidXVelocity(state, xVelocity);
        MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, xVelocity);

        int yVelocity = slot.YPosition == samus.YPosition
            ? 0
            : unchecked((short)(slot.YPosition - samus.YPosition)) > 0
                ? -MochtroidAttachedVelocity
                : MochtroidAttachedVelocity;
        StoreMochtroidYVelocity(state, yVelocity);
        MoveEnemyVertically(level, slot, yVelocity);
    }

    private static void RunMochtroidShake(RoomEnemySlot slot, MochtroidEnemyState state)
    {
        // This third dispatch entry is unused by retail callers but present in the shipped
        // table. The low two even bits select right/up/left/down, then the timer counts down.
        int tableIndex = (state.ShakeTimer & 0x0006) >> 1;
        ReadOnlySpan<short> xOffsets = [2, 0, -2, 0];
        ReadOnlySpan<short> yOffsets = [0, -2, 0, 2];
        slot.XPosition = unchecked((ushort)(slot.XPosition + xOffsets[tableIndex]));
        slot.YPosition = unchecked((ushort)(slot.YPosition + yOffsets[tableIndex]));
        StoreMochtroidXVelocity(state, 0);
        StoreMochtroidYVelocity(state, 0);
        state.ShakeTimer = unchecked((ushort)(state.ShakeTimer - 1));
        if (state.ShakeTimer == 0)
            state.MovementMode = MochtroidMovementMode.NotTouchingSamus;
        SetMochtroidInstructionList(slot, state, MochtroidIdleInstructionList);
    }

    private static void StoreMochtroidXVelocity(MochtroidEnemyState state, int velocity)
    {
        state.XVelocity = unchecked((short)(velocity >> 16));
        state.XSubvelocity = unchecked((ushort)velocity);
    }

    private static void StoreMochtroidYVelocity(MochtroidEnemyState state, int velocity)
    {
        state.YVelocity = unchecked((short)(velocity >> 16));
        state.YSubvelocity = unchecked((ushort)velocity);
    }

    private static void SetMochtroidInstructionList(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        ushort instructionList)
    {
        if (state.InstalledInstructionList == instructionList)
            return;
        state.InstalledInstructionList = instructionList;
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Ports the family-specific portion of <c>EnemyTouch_Mochtroid</c>.</summary>
    private void ResolveMochtroidTouch(
        RoomEnemySlot slot,
        MochtroidEnemyState state,
        SamusState samus,
        ushort controllerInput)
    {
        state.MovementMode = MochtroidMovementMode.TouchingSamus;
        SetMochtroidInstructionList(slot, state, MochtroidAttachedInstructionList);
        state.AttachmentDamageTimer = unchecked((ushort)(state.AttachmentDamageTimer + 1));

        bool applyCommonTouch = samus.HorizontalSpeed.ContactDamageIndex != 0;
        if (!applyCommonTouch)
        {
            // The sound queue itself is owned by the frontend/audio layer. Publish the exact
            // library-three effect on the native global enemy-frame phase so that consumer
            // code and the verifier can observe it without playing host-invented audio.
            if ((_randomEnemyCounter & 7) == 7 && unchecked((short)(samus.Health - 30)) >= 0)
                LastMochtroidSoundEffect = 0x002d;

            if (unchecked((short)(state.AttachmentDamageTimer - MochtroidAttachmentDamagePeriod)) >= 0)
            {
                state.AttachmentDamageTimer = 0;
                applyCommonTouch = true;
            }
        }

        if (!applyCommonTouch)
            return;

        if (samus.HorizontalSpeed.ContactDamageIndex == 0)
        {
            // Common touch would publish 96/5 here, but $A3:A99E clears both words before
            // Samus's movement phase can consume them. Apply only the suit-divided health
            // change; starting the host knockback state and trying to undo its pose would
            // introduce side effects the cartridge never reaches.
            ushort damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                ? unchecked((ushort)(slot.Definition.Damage >> 2))
                : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                    ? unchecked((ushort)(slot.Definition.Damage >> 1))
                    : slot.Definition.Damage;
            samus.Health = samus.Health <= damage
                ? (ushort)0
                : unchecked((ushort)(samus.Health - damage));
        }
        else
        {
            // A live Samus contact-damage mode takes common AI's enemy-damage branch. This
            // includes Speed Booster, shinespark, Screw Attack, and pseudo-Screw.
            ResolveNormalEnemyTouch(slot, samus, controllerInput);
        }

        // Unlike every ordinary contact handler, Mochtroid immediately cancels the timers
        // installed by common touch AI. Samus remains attached rather than entering a normal
        // invincibility/knockback grace period, permitting another 80-contact drain cycle.
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
    }

    private MochtroidEnemyState RequireMochtroidState(RoomEnemySlot slot) =>
        _mochtroidStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Mochtroid state.");
}
