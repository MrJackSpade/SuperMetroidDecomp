namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A8 function words stored in the Wrecked Ship ghost's native enemy variable A.
/// Keeping the cartridge addresses makes debugger watches directly comparable with WRAM.
/// </summary>
public enum WreckedShipGhostAiFunction : ushort
{
    BrighteningAndFlickering = 0x9b42,
    FadingToGhostPalette = 0x9bad,
    WaitingForWhiteFade = 0x9c69,
    BobbingWhileVisible = 0x9c8a,
    InitialInvisibleDelay = 0x9d13,
    TrackingSamusForSpawn = 0x9d36,
}

/// <summary>
/// Typed projection of the ghost's ordinary variables and its bank-$A8 extension in WRAM
/// $7800. The extension is actor-owned state in the original even though it lives outside
/// the common 64-byte enemy record; representing it here avoids disguising those words as
/// unrelated host counters.
/// </summary>
public sealed class WreckedShipGhostEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _targetPalette = new ushort[16];

    internal WreckedShipGhostEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Enemy variable A: bank-$A8 indirect main-AI function.</summary>
    public WreckedShipGhostAiFunction Function
    {
        get => (WreckedShipGhostAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Enemy variable B: initial/reappearance delay, flicker interval, or visible lifetime,
    /// depending on <see cref="Function"/>.
    /// </summary>
    public ushort PhaseTimer
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Enemy variable C: byte offset into the ROM flicker-duration table.</summary>
    public ushort FlickerTableOffset
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Extended word $00: Y coordinate about which the visible ghost oscillates.</summary>
    public ushort BobbingOriginY { get; internal set; }

    /// <summary>Extended word $01: low 16 bits of the signed 16.16 vertical velocity.</summary>
    public ushort VerticalVelocityFraction { get; internal set; }

    /// <summary>Extended word $02: signed whole part of the 16.16 vertical velocity.</summary>
    public ushort VerticalVelocityWhole { get; internal set; }

    /// <summary>
    /// Extended word $05: horizontal movement class: 0 left, 4 stationary, or 8 right.
    /// These values are byte offsets used by the native nine-entry spawn-offset table.
    /// </summary>
    public ushort HorizontalMovementClass { get; internal set; }

    /// <summary>Extended words $06-$08: previous Samus X and its asymmetric ±1 bounds.</summary>
    public ushort PreviousSamusX { get; internal set; }
    public ushort SamusXLowerBound { get; internal set; }
    public ushort SamusXUpperBound { get; internal set; }

    /// <summary>
    /// Extended word $09: vertical movement class: 0 up, 12 stationary, or 24 down.
    /// </summary>
    public ushort VerticalMovementClass { get; internal set; }

    /// <summary>Extended words $0A-$0C: previous Samus Y and its asymmetric ±1 bounds.</summary>
    public ushort PreviousSamusY { get; internal set; }
    public ushort SamusYLowerBound { get; internal set; }
    public ushort SamusYUpperBound { get; internal set; }

    /// <summary>Extended word $0D: 64-frame timer while Samus remains nearly still.</summary>
    public ushort StablePositionTimer { get; internal set; }

    /// <summary>Extended word $0E: 16-frame timer while Samus keeps one direction.</summary>
    public ushort StableDirectionTimer { get; internal set; }

    /// <summary>
    /// Actor-selected sixteen-color target palette. Retail stores this in global target
    /// palette WRAM; the sidecar preserves that second buffer because <see cref="Hardware.SnesCgram"/>
    /// models the independently visible current palette.
    /// </summary>
    public ReadOnlyMemory<ushort> TargetPalette => _targetPalette;

    internal Span<ushort> MutableTargetPalette => _targetPalette;
}

/// <summary>
/// Literal translation of Wrecked Ship ghost definition <c>$A0:E77F</c>, initialization
/// <c>$A8:9AEE</c>, and main state machine <c>$A8:9B3C-$9E87</c>. The ghost spends most of
/// its life invisible, observes Samus movement, appears in one of nine relative positions,
/// flickers while its palette turns white, becomes vulnerable for 120 bobbing frames, then
/// fades back to white and repeats. It has no bespoke projectile: its attack and damage
/// paths are the ordinary header-authored touch/shot handlers.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort WreckedShipGhostDefinition = 0xe77f;

    private const ushort WreckedShipGhostInstructionList = 0x9a8c;
    private const ushort WreckedShipGhostInitialDelay = 0x0118; // $78 + $A0 at $A8:9B17.
    private const ushort WreckedShipGhostVisibleDuration = 0x0078;
    private const ushort WreckedShipGhostStablePositionDuration = 0x0040;
    private const ushort WreckedShipGhostStableDirectionDuration = 0x0010;
    private const ushort WreckedShipGhostInitialVerticalVelocityWhole = 1;
    private const int WreckedShipGhostVerticalAccelerationFraction = 0x1800;

    // $A8:9ACC. Offset zero is intentionally present even though the normal appearance
    // path begins at byte offset two; once the terminal word is reached, native code resets
    // the offset to zero and permanently clears invisibility for that appearance.
    private static readonly short[] WreckedShipGhostFlickerDurations =
    [
        1, 8, 1, 8, 1, 7, 1, 7, 2, 6, 2, 6, 3, 5, 3, 5, -1,
    ];

    // $A8:9AA8. Horizontal classes 0/4/8 and vertical classes 0/12/24 add to a byte
    // offset; dividing by two selects one of these signed (X,Y) pairs.
    private static readonly (short X, short Y)[] WreckedShipGhostSpawnOffsets =
    [
        (-64, -64), (0, -64), (64, -64),
        (-64,   0), (0,   0), (64,   0),
        (-64,  64), (0,  64), (64,  64),
    ];

    // $A8:99AC. This is the target after the initial white flash and again after each
    // reappearance. Words remain native BGR555 so every component step is auditable.
    private static readonly ushort[] WreckedShipGhostPalette =
    [
        0x3800, 0x57ff, 0x42f7, 0x0929,
        0x00a5, 0x4f5a, 0x36b5, 0x2610,
        0x1dce, 0x01df, 0x001f, 0x0018,
        0x000a, 0x06b9, 0x00ea, 0x0045,
    ];

    private readonly WreckedShipGhostEnemyState?[] _wreckedShipGhostStates =
        new WreckedShipGhostEnemyState?[MaximumEnemyCount];

    /// <summary>Typed native state for every physical Wrecked Ship ghost slot.</summary>
    public IReadOnlyList<WreckedShipGhostEnemyState?> WreckedShipGhostStates =>
        _wreckedShipGhostStates;

    /// <summary>Ports <c>WreckedShipGhost_Init</c> at <c>$A8:9AEE</c>.</summary>
    private void InitializeWreckedShipGhost(RoomEnemySlot slot)
    {
        // Every named retail ghost population places this actor in physical slot zero.
        // Its tracking routine contains several absolute-Samus reads indexed by the native
        // enemy offset; a nonzero slot would deliberately alias unrelated WRAM. Enforce the
        // authored layout rather than silently pretending that retail bug has generic rules.
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                "Wrecked Ship ghost requires retail physical slot zero for indexed Samus reads.");
        }

        // Native ORs property bits $2000, $0400, and $0100. In this host those mean
        // instruction processing, exclusion from Samus interaction, and invisibility.
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions |
            EnemyProperties.IgnoreSamusCollision |
            EnemyProperties.Invisible);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = WreckedShipGhostInstructionList;

        var state = new WreckedShipGhostEnemyState(slot)
        {
            Function = WreckedShipGhostAiFunction.InitialInvisibleDelay,
            PhaseTimer = WreckedShipGhostInitialDelay,
        };

        // Retail clears the actor palette's target-buffer entries here. Its room fade then
        // settles the independently stored current palette to that black target long before
        // the 280-frame appearance delay expires. SnesCgram is this runtime's already-settled
        // visible buffer, so clear it alongside the retained target buffer; otherwise the
        // first white ramp would begin from the graphics-set palette and overflow BGR fields.
        state.MutableTargetPalette.Clear();
        int paletteStart = WreckedShipGhostPaletteStart(slot);
        for (int color = 0; color < 16; color++)
            _cgram!.SetColor(paletteStart + color, 0);
        _wreckedShipGhostStates[slot.SlotIndex] = state;
    }

    /// <summary>Ports the indirect dispatch in <c>WreckedShipGhost_Main</c>.</summary>
    private void RunWreckedShipGhostMain(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state,
        SamusState? samus)
    {
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Wrecked Ship ghost AI requires the active Samus actor.");

        switch (state.Function)
        {
            case WreckedShipGhostAiFunction.BrighteningAndFlickering:
                RunWreckedShipGhostBrightening(slot, state);
                break;
            case WreckedShipGhostAiFunction.FadingToGhostPalette:
                RunWreckedShipGhostPaletteFade(slot, state, activeSamus);
                break;
            case WreckedShipGhostAiFunction.WaitingForWhiteFade:
                RunWreckedShipGhostWhiteFade(slot, state);
                break;
            case WreckedShipGhostAiFunction.BobbingWhileVisible:
                RunWreckedShipGhostBobbing(slot, state);
                break;
            case WreckedShipGhostAiFunction.InitialInvisibleDelay:
                RunWreckedShipGhostInitialDelay(state);
                break;
            case WreckedShipGhostAiFunction.TrackingSamusForSpawn:
                RunWreckedShipGhostTracking(slot, state, activeSamus);
                break;
            default:
                throw new InvalidDataException(
                    $"Wrecked Ship ghost function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// Ports $A8:9D13. A zero timer also advances immediately, matching the native ORA/DEC
    /// branch rather than wrapping zero to $FFFF.
    /// </summary>
    private static void RunWreckedShipGhostInitialDelay(WreckedShipGhostEnemyState state)
    {
        if (state.PhaseTimer != 0)
            state.PhaseTimer = unchecked((ushort)(state.PhaseTimer - 1));
        if (state.PhaseTimer != 0)
            return;

        state.PhaseTimer = 1;
        state.FlickerTableOffset = 2;
        state.Function = WreckedShipGhostAiFunction.TrackingSamusForSpawn;
    }

    /// <summary>
    /// Ports $A8:9D36. The one-pixel upper bound is exclusive in retail; this asymmetric
    /// comparison is deliberately not replaced by an absolute-distance convenience test.
    /// </summary>
    private static void RunWreckedShipGhostTracking(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state,
        SamusState samus)
    {
        ushort samusX = samus.XPosition;
        ushort samusY = samus.YPosition;

        bool withinPreviousBox =
            unchecked((short)(samusX - state.SamusXLowerBound)) >= 0 &&
            unchecked((short)(samusX - state.SamusXUpperBound)) < 0 &&
            unchecked((short)(samusY - state.SamusYLowerBound)) >= 0 &&
            unchecked((short)(samusY - state.SamusYUpperBound)) < 0;

        if (withinPreviousBox)
        {
            state.StablePositionTimer = unchecked((ushort)(state.StablePositionTimer - 1));
            if (state.StablePositionTimer == 0)
            {
                // Remaining inside the tiny box for 64 frames means "stationary" on both
                // axes and therefore selects the central spawn offset.
                state.HorizontalMovementClass = 4;
                state.VerticalMovementClass = 12;
                SpawnWreckedShipGhost(slot, state, samusX, samusY);
                return;
            }
        }
        else
        {
            state.StablePositionTimer = WreckedShipGhostStablePositionDuration;
            ushort horizontalClass = unchecked((short)(samusX - state.PreviousSamusX)) switch
            {
                < 0 => 0,
                0 => 4,
                _ => 8,
            };

            if (horizontalClass != state.HorizontalMovementClass)
            {
                state.HorizontalMovementClass = horizontalClass;
                state.StableDirectionTimer = WreckedShipGhostStableDirectionDuration;
            }
            else
            {
                ushort verticalClass = unchecked((short)(samusY - state.PreviousSamusY)) switch
                {
                    < 0 => 0,
                    0 => 12,
                    _ => 24,
                };

                if (verticalClass != state.VerticalMovementClass)
                {
                    state.VerticalMovementClass = verticalClass;
                    state.StableDirectionTimer = WreckedShipGhostStableDirectionDuration;
                }
                else
                {
                    state.StableDirectionTimer = unchecked((ushort)(state.StableDirectionTimer - 1));
                    if (state.StableDirectionTimer == 0)
                    {
                        SpawnWreckedShipGhost(slot, state, samusX, samusY);
                        return;
                    }
                }
            }
        }

        // The native tracker publishes a fresh asymmetric [position-1, position+1) box at
        // the end of every non-spawning frame.
        state.PreviousSamusX = samusX;
        state.SamusXLowerBound = unchecked((ushort)(samusX - 1));
        state.SamusXUpperBound = unchecked((ushort)(samusX + 1));
        state.PreviousSamusY = samusY;
        state.SamusYLowerBound = unchecked((ushort)(samusY - 1));
        state.SamusYUpperBound = unchecked((ushort)(samusY + 1));
    }

    /// <summary>Installs the direction-selected relative position and begins $A8:9B42.</summary>
    private static void SpawnWreckedShipGhost(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state,
        ushort samusX,
        ushort samusY)
    {
        int byteOffset = state.HorizontalMovementClass + state.VerticalMovementClass;
        if ((byteOffset & 3) != 0 || (uint)(byteOffset / 4) >= WreckedShipGhostSpawnOffsets.Length)
        {
            throw new InvalidDataException(
                $"Wrecked Ship ghost movement classes ${state.HorizontalMovementClass:X4}/" +
                $"${state.VerticalMovementClass:X4} do not select a retail spawn offset.");
        }

        (short xOffset, short yOffset) = WreckedShipGhostSpawnOffsets[byteOffset / 4];
        slot.XPosition = unchecked((ushort)(samusX + xOffset));
        slot.YPosition = unchecked((ushort)(samusY + yOffset));
        state.Function = WreckedShipGhostAiFunction.BrighteningAndFlickering;
        state.StablePositionTimer = WreckedShipGhostStablePositionDuration;
        state.StableDirectionTimer = WreckedShipGhostStableDirectionDuration;
    }

    /// <summary>Ports the white-flash ramp at $A8:9B42 and its flicker helper.</summary>
    private void RunWreckedShipGhostBrightening(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        AdvanceWreckedShipGhostFlicker(slot, state);
        int paletteStart = WreckedShipGhostPaletteStart(slot);
        int colorsChanged = 0;
        for (int color = 0; color < 16; color++)
        {
            ushort current = _cgram!.Colors[paletteStart + color];
            if ((current & 0x001f) >= 31)
                continue;

            // Native adds $0421 as a word, raising red, green, and blue together. It gates
            // only on red and does not independently saturate the other components.
            _cgram.SetColor(paletteStart + color, unchecked((ushort)(current + 0x0421)));
            colorsChanged++;
        }

        if (colorsChanged != 0)
            return;

        state.Function = WreckedShipGhostAiFunction.FadingToGhostPalette;
        WreckedShipGhostPalette.CopyTo(state.MutableTargetPalette);
    }

    /// <summary>Ports $A8:9BAD, including the simultaneous flicker and per-component fade.</summary>
    private void RunWreckedShipGhostPaletteFade(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state,
        SamusState samus)
    {
        int componentsChanged = StepWreckedShipGhostPaletteTowardTarget(slot, state);
        AdvanceWreckedShipGhostFlicker(slot, state);
        if (state.PhaseTimer != 0 || componentsChanged != 0)
            return;

        // Clearing $0500 makes the fully materialized ghost visible and eligible for the
        // ordinary touch/shot collision handlers authored by its enemy definition.
        slot.Properties = slot.Properties.Without(
            EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
        state.Function = WreckedShipGhostAiFunction.BobbingWhileVisible;
        state.BobbingOriginY = slot.YPosition;
        state.PhaseTimer = WreckedShipGhostVisibleDuration;
        state.FlickerTableOffset = 2;
        state.VerticalVelocityFraction = 0;
        state.VerticalVelocityWhole = WreckedShipGhostInitialVerticalVelocityWhole;
        state.HorizontalMovementClass = 4;
        state.PreviousSamusX = samus.XPosition;
        state.SamusXLowerBound = samus.XPosition;
        state.SamusXUpperBound = samus.XPosition;
        state.VerticalMovementClass = 12;
        state.PreviousSamusY = samus.YPosition;
        state.SamusYLowerBound = samus.YPosition;
        state.SamusYUpperBound = samus.YPosition;
        state.StablePositionTimer = WreckedShipGhostStablePositionDuration;
        state.StableDirectionTimer = WreckedShipGhostStableDirectionDuration;
    }

    /// <summary>Ports the signed 16.16 oscillation and 120-frame visible lifetime at $A8:9C8A.</summary>
    private static void RunWreckedShipGhostBobbing(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        uint position = ((uint)slot.YPosition << 16) | slot.YSubposition;
        uint velocity = ((uint)state.VerticalVelocityWhole << 16) | state.VerticalVelocityFraction;
        position = unchecked(position + velocity);
        slot.YPosition = unchecked((ushort)(position >> 16));
        slot.YSubposition = unchecked((ushort)position);

        int acceleration = unchecked((short)(slot.YPosition - state.BobbingOriginY)) < 0
            ? WreckedShipGhostVerticalAccelerationFraction
            : -WreckedShipGhostVerticalAccelerationFraction;
        velocity = unchecked((uint)(velocity + acceleration));
        state.VerticalVelocityWhole = unchecked((ushort)(velocity >> 16));
        state.VerticalVelocityFraction = unchecked((ushort)velocity);

        state.PhaseTimer = unchecked((ushort)(state.PhaseTimer - 1));
        if (state.PhaseTimer != 0)
            return;

        state.Function = WreckedShipGhostAiFunction.WaitingForWhiteFade;
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.MutableTargetPalette.Fill(0x7fff);
    }

    /// <summary>Ports $A8:9C69: finish whitening, hide, and restart the 120-frame delay.</summary>
    private void RunWreckedShipGhostWhiteFade(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        if (StepWreckedShipGhostPaletteTowardTarget(slot, state) != 0)
            return;

        state.Function = WreckedShipGhostAiFunction.InitialInvisibleDelay;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.PhaseTimer = WreckedShipGhostVisibleDuration;
    }

    /// <summary>
    /// Ports $A8:9C31. The table alternates hidden and visible intervals whose lengths
    /// converge; its $FFFF terminator leaves the ghost permanently visible for this cycle.
    /// </summary>
    private static void AdvanceWreckedShipGhostFlicker(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        if (state.PhaseTimer != 0)
        {
            state.PhaseTimer = unchecked((ushort)(state.PhaseTimer - 1));
            if (state.PhaseTimer != 0)
                return;

            int tableIndex = state.FlickerTableOffset / 2;
            if ((uint)tableIndex >= WreckedShipGhostFlickerDurations.Length)
            {
                throw new InvalidDataException(
                    $"Wrecked Ship ghost flicker offset ${state.FlickerTableOffset:X4} exceeds $A8:9ACC.");
            }

            short duration = WreckedShipGhostFlickerDurations[tableIndex];
            if (duration < 0)
            {
                state.PhaseTimer = 0;
                state.FlickerTableOffset = 0;
                return;
            }

            state.PhaseTimer = unchecked((ushort)duration);
            ushort oldOffset = state.FlickerTableOffset;
            state.FlickerTableOffset = unchecked((ushort)(oldOffset + 2));
            if ((oldOffset & 2) != 0)
                return;
        }

        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
    }

    /// <summary>
    /// Ports $A8:9E88. Every BGR555 component moves exactly one hardware level toward its
    /// target per frame; the return value counts component changes, not changed colors.
    /// </summary>
    private int StepWreckedShipGhostPaletteTowardTarget(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        int paletteStart = WreckedShipGhostPaletteStart(slot);
        int componentsChanged = 0;
        ReadOnlySpan<ushort> target = state.TargetPalette.Span;
        for (int color = 0; color < 16; color++)
        {
            ushort current = _cgram!.Colors[paletteStart + color];
            ushort next = current;
            next = StepWreckedShipGhostPaletteComponent(next, target[color], 0x001f, 0, ref componentsChanged);
            next = StepWreckedShipGhostPaletteComponent(next, target[color], 0x03e0, 5, ref componentsChanged);
            next = StepWreckedShipGhostPaletteComponent(next, target[color], 0x7c00, 10, ref componentsChanged);
            if (next != current)
                _cgram.SetColor(paletteStart + color, next);
        }
        return componentsChanged;
    }

    private static ushort StepWreckedShipGhostPaletteComponent(
        ushort current,
        ushort target,
        ushort mask,
        int shift,
        ref int componentsChanged)
    {
        int currentComponent = (current & mask) >> shift;
        int targetComponent = (target & mask) >> shift;
        if (currentComponent == targetComponent)
            return current;

        currentComponent += currentComponent > targetComponent ? -1 : 1;
        componentsChanged++;
        return unchecked((ushort)((current & ~mask) | (currentComponent << shift)));
    }

    private static int WreckedShipGhostPaletteStart(RoomEnemySlot slot)
    {
        int objectPalette = (slot.PaletteIndex >> 9) & 7;
        return 128 + objectPalette * 16;
    }

    private WreckedShipGhostEnemyState RequireWreckedShipGhostState(RoomEnemySlot slot) =>
        _wreckedShipGhostStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Wrecked Ship ghost slot {slot.SlotIndex} has no initialized native state.");
}
