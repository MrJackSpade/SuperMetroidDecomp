using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// Typed projection of Magdollite's common slot words and bank-$7E:7800 extension. The
/// ordinary A-F words remain backed by <see cref="RoomEnemySlot"/> so debugger values match
/// the cartridge. Extension words are named here instead of reproducing native's deliberately
/// obscure drawing-queue/spawn-snapshot aliases between three neighboring records.
/// </summary>
public sealed class MagdolliteEnemyState
{
    /// <summary>Physical head, arm, or hand slot whose variable words back this typed view.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Attaches one typed role to its own native enemy record.</summary>
    internal MagdolliteEnemyState(RoomEnemySlot slot, MagdollitePart part)
    {
        _slot = slot;
        Part = part;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<MagdolliteProgram> Program { get; set; }

    /// <summary>The immutable head, arm, or hand role decoded from this physical enemy slot's population parameter one.</summary>
    public MagdollitePart Part { get; }
    /// <summary>Native variable F ($0FB2 plus the slot's byte index): the current bank-$A8 AI function pointer.</summary>
    public MagdolliteEnemyFunction Function
    {
        get => (MagdolliteEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Native variable B: the arm's even byte offset into the nine-entry rise tables, aliased by the hand as throw direction 0 for left or 1 for right.</summary>
    public ushort BodyPhaseOffset
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Native variable C: the program last installed; null until a part's initializer installs its first.</summary>
    internal MagdolliteProgram? InstalledProgram { get; set; }

    /// <summary>Native variable D: the program the part's AI wants shown.</summary>
    internal MagdolliteProgram DesiredProgram { get; set; }

    /// <summary>Native variable E: signed whole-pixel displacement accumulated by the body.</summary>
    public ushort VerticalWholeDisplacement
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native headDirection at extension offset $00: main AI uses 0 for left and 1 for right; head initialization briefly uses the opposite convention.</summary>
    public ushort FacingMarker { get; internal set; }
    /// <summary>Native animationActiveFlag at extension offset $02, set and cleared by instruction callbacks to gate head transitions and completion of the hand's throw.</summary>
    public bool AnimationBusy { get; internal set; }
    /// <summary>Native emergeNotReadyFlag at extension offset $04: set by the head after submerging and cleared by the arm on landing so the head may emerge.</summary>
    public bool HeadWaitsForBodyLanding { get; internal set; }
    /// <summary>Native YSpawnPosition at extension offset $06, captured in whole room pixels before submerging offsets and used to reset the head and arm.</summary>
    public ushort OriginY { get; internal set; }

    /// <summary>
    /// Native extension variable four. Only the overlay's instance is polled by the head;
    /// decrementing it in every part remains observable and matches main AI exactly.
    /// The wrapping word counts enemy updates, is reset to $0100 by the throw list,
    /// and expires when the head observes its sign bit after decrementing through zero.
    /// </summary>
    public NativeCountdown AttackTimer { get; internal set; }

    /// <summary>Native downSubVelocity at extension offset $0E: unsigned fractional word of downward 16.16 pixels per enemy update; the arm consumes the head's copy.</summary>
    public ushort PositiveSpeedFraction { get; internal set; }
    /// <summary>Native downVelocity at extension offset $10: signed whole-pixel word of the downward 16.16 velocity, stored as raw unsigned bits.</summary>
    public ushort PositiveSpeedWhole { get; internal set; }
    /// <summary>Native upSubVelocity at extension offset $12: fractional word of upward 16.16 pixels per enemy update, imported independently from the negative speed-table record.</summary>
    public ushort NegativeSpeedFraction { get; internal set; }
    /// <summary>Native upVelocity at extension offset $14: signed whole-pixel word of the upward 16.16 velocity, stored as raw unsigned bits; the arm consumes the head's copy.</summary>
    public ushort NegativeSpeedWhole { get; internal set; }
    /// <summary>Native finishedGrowingFlag at extension offset $18, set by the arm at maximum or Samus-relative height and cleared by the hand when shrinking begins.</summary>
    public bool BodyReachedApex { get; internal set; }
    /// <summary>Native notEmergedFlag at extension offset $1A: despite the host name, true holds the arm dormant; the head clears it to start growth and landing sets it again.</summary>
    public bool BodyWakeRequested { get; internal set; }
    /// <summary>The hand's initial whole-pixel room X, captured by $A8:B02D at native extension offset $22, separately from its later throw origin.</summary>
    public ushort InitializationOriginX { get; internal set; }
    /// <summary>Native throwXPosition/throwYPosition at extension offsets $24/$26: whole-pixel room position captured at the apex and used as the base for hand-animation shifts and restoration.</summary>
    public Position16<RoomPixels> ThrowOrigin { get; internal set; }
}
