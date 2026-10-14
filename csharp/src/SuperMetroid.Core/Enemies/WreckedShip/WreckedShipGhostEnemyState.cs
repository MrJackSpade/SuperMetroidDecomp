namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// Typed projection of the ghost's ordinary variables and its bank-$A8 extension in WRAM
/// $7800. The extension is actor-owned state in the original even though it lives outside
/// the common 64-byte enemy record; representing it here avoids disguising those words as
/// unrelated host counters.
/// </summary>
public sealed class WreckedShipGhostEnemyState
{
    /// <summary>Common enemy words containing the ghost's function and phase timers.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Ghost color target kept separate from currently visible CGRAM colors.</summary>
    private readonly Bgr555[] _targetPalette = new Bgr555[16];

    /// <summary>Projects the ghost's extended state onto its live enemy slot.</summary>
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
    public NativeCountdown PhaseTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableB);
        internal set => _slot.VariableB = value.RawValue;
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
    /// Extended word $05: Samus's horizontal movement class as the spawn-offset table's column
    /// byte offset: 0 left, 4 stationary, or 8 right. Added to <see cref="SpawnRowOffset"/> it
    /// indexes the native nine-entry spawn-offset table; it is a table offset, not a coordinate.
    /// </summary>
    public ushort SpawnColumnOffset { get; internal set; }

    /// <summary>
    /// Extended word $09: Samus's vertical movement class as the spawn-offset table's row byte
    /// offset: 0 up, 12 stationary, or 24 down.
    /// </summary>
    public ushort SpawnRowOffset { get; internal set; }

    /// <summary>Extended words $06/$0A: Samus's previous whole-pixel position.</summary>
    public Position16<RoomPixels> PreviousSamusPosition { get; internal set; }
    /// <summary>Extended words $07/$0B: the inclusive lower corner of the nearly-still box (previous position -1).</summary>
    public Position16<RoomPixels> SamusLowerBound { get; internal set; }
    /// <summary>Extended words $08/$0C: the exclusive upper corner of the nearly-still box (previous position +1).</summary>
    public Position16<RoomPixels> SamusUpperBound { get; internal set; }

    /// <summary>Extended word $0D: 64-frame timer while Samus remains nearly still.</summary>
    public NativeCountdown StablePositionTimer { get; internal set; }

    /// <summary>Extended word $0E: 16-frame timer while Samus keeps one direction.</summary>
    public NativeCountdown StableDirectionTimer { get; internal set; }

    /// <summary>
    /// Actor-selected sixteen-color target palette. Retail stores this in global target
    /// palette WRAM; the sidecar preserves that second buffer because <see cref="Hardware.SnesCgram"/>
    /// models the independently visible current palette.
    /// </summary>
    public ReadOnlyMemory<Bgr555> TargetPalette => _targetPalette;

    /// <summary>Allows the palette transition to update all sixteen target colors in place.</summary>
    internal Span<Bgr555> MutableTargetPalette => _targetPalette;
}
