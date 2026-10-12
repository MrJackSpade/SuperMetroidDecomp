using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A8 function words stored in the Wrecked Ship ghost's native enemy variable A.
/// Keeping the cartridge addresses makes debugger watches directly comparable with WRAM.
/// </summary>
public enum WreckedShipGhostAiFunction : ushort
{
    /// <summary>$A8:9B42 raises the palette toward white while advancing the flicker table.</summary>
    BrighteningAndFlickering = 0x9b42,
    /// <summary>$A8:9BAD fades from white to the installed ghost palette while flickering.</summary>
    FadingToGhostPalette = 0x9bad,
    /// <summary>$A8:9C69 waits for the post-visible fade to reach white before hiding.</summary>
    WaitingForWhiteFade = 0x9c69,
    /// <summary>$A8:9C8A oscillates vertically during the vulnerable visible interval.</summary>
    BobbingWhileVisible = 0x9c8a,
    /// <summary>$A8:9D13 counts down the invisible delay before Samus tracking begins.</summary>
    InitialInvisibleDelay = 0x9d13,
    /// <summary>$A8:9D36 observes Samus movement to select one of nine relative spawn positions.</summary>
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
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Stores target palette.</summary>
    private readonly Bgr555[] _targetPalette = new Bgr555[16];

    /// <summary>Initializes a new wrecked ship ghost enemy state instance.</summary>
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
    public ushort StablePositionTimer { get; internal set; }

    /// <summary>Extended word $0E: 16-frame timer while Samus keeps one direction.</summary>
    public ushort StableDirectionTimer { get; internal set; }

    /// <summary>
    /// Actor-selected sixteen-color target palette. Retail stores this in global target
    /// palette WRAM; the sidecar preserves that second buffer because <see cref="Hardware.SnesCgram"/>
    /// models the independently visible current palette.
    /// </summary>
    public ReadOnlyMemory<Bgr555> TargetPalette => _targetPalette;

    /// <summary>Gets mutable target palette.</summary>
    internal Span<Bgr555> MutableTargetPalette => _targetPalette;
}
