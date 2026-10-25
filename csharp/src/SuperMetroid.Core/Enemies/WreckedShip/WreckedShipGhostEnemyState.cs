using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The state of the Wrecked Ship ghost, with its bank-$A8 extension in WRAM $7800. Its behaviour, phase timer and
/// flicker offset are values the cartridge keeps in the enemy's common record, so a kill empties them with that record
/// (<see cref="ClearWithRecord"/>); the extension outlives the kill.
/// </summary>
public sealed class WreckedShipGhostEnemyState
{
    /// <summary>The colours the ghost fades to, kept separate from the colours it shows.</summary>
    private readonly RgbColor[] _targetColours = new RgbColor[16];

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<WreckedShipGhostProgram> Program { get; set; }

    /// <summary>The current behaviour; none once a kill has emptied the record.</summary>
    public WreckedShipGhostAiFunction? Function { get; internal set; }

    /// <summary>Initial/reappearance delay, flicker interval, or visible lifetime, depending on <see cref="Function"/>.</summary>
    public WrappingCountdown PhaseTimer { get; internal set; }

    /// <summary>
    /// The flicker-duration table's byte offset, two per interval, 0 through 65535. It starts at two, steps by two with
    /// each interval and returns to zero at the table's terminator; its second bit selects whether the interval just
    /// started shows the ghost. An offset past the table's seventeen intervals is rejected before it can grow further.
    /// </summary>
    public int FlickerTableOffset
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record: no behaviour, timer or flicker offset.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        PhaseTimer = WrappingCountdown.Start(0);
        FlickerTableOffset = 0;
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
    public SignedSixteenSixteen VerticalVelocity { get; internal set; }

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
    public WrappingCountdown StablePositionTimer { get; internal set; }

    /// <summary>Extended word $0E: 16-frame timer while Samus keeps one direction.</summary>
    public WrappingCountdown StableDirectionTimer { get; internal set; }

    /// <summary>
    /// Actor-selected sixteen-color target palette. Retail stores this in global target
    /// palette WRAM; the sidecar preserves that second buffer because the actor's shown colours
    /// model the independently visible current palette.
    /// </summary>
    public ReadOnlyMemory<RgbColor> TargetColours => _targetColours;

    /// <summary>Allows the palette transition to update all sixteen target colors in place.</summary>
    internal Span<RgbColor> MutableTargetColours => _targetColours;
}
