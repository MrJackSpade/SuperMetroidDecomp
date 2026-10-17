using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
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

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<WreckedShipGhostProgram> Program { get; set; }

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

    /// <summary>
    /// Enemy variable C: the flicker-duration table's byte offset, two per interval. It starts at two, steps by two
    /// with each interval and returns to zero at the table's terminator; its second bit selects whether the interval
    /// just started shows the ghost. An offset past the table's seventeen intervals is rejected before it can grow
    /// further, and the slot variable keeps the same value for other enemies' aliased enemy-RAM reads.
    /// </summary>
    public int FlickerTableOffset
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = checked((ushort)value);
    }

    /// <summary>
    /// Extended word $00: the position about which the visible ghost oscillates; only its Y is meaningful. Positions
    /// compare with it by the wrapped spatial difference.
    /// </summary>
    public Position16<RoomPixels> BobbingOrigin { get; internal set; }

    /// <summary>
    /// Extended words $01 (fraction) and $02 (whole pixels): the visible ghost's signed 16.16 vertical speed in pixels
    /// per update, accelerating toward <see cref="BobbingOrigin"/> with the cartridge's carry and wraparound.
    /// </summary>
    public SnesSignedSixteenSixteen VerticalVelocity { get; internal set; }

    /// <summary>
    /// Extended word $05: Samus's horizontal movement class, 0 moving left, 1 stationary or 2 moving right. With
    /// <see cref="SpawnRow"/> it selects one of the nine spawn offsets.
    /// </summary>
    public int SpawnColumn { get; internal set; }

    /// <summary>Extended word $09: Samus's vertical movement class, 0 moving up, 1 stationary or 2 moving down.</summary>
    public int SpawnRow { get; internal set; }

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
