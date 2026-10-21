using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The state of one Magdollite part, with its bank-$7E:7800 extension. Extension words are named here instead of
/// reproducing native's deliberately obscure drawing-queue/spawn-snapshot aliases between three neighboring records.
/// The values the cartridge keeps in the enemy's common record are emptied with it when the enemy is killed (<see
/// cref="ClearWithRecord"/>).
/// </summary>
public sealed class MagdolliteEnemyState
{
    /// <summary>Creates the state for one Magdollite part.</summary>
    internal MagdolliteEnemyState(MagdollitePart part)
    {
        Part = part;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<MagdolliteProgram> Program { get; set; }

    /// <summary>The immutable head, arm, or hand role decoded from this physical enemy slot's population parameter one.</summary>
    public MagdollitePart Part { get; }
    /// <summary>The current bank-$A8 AI function pointer; none once a kill has emptied the record.</summary>
    public MagdolliteEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// The arm's rise phase, 0 through 8, selecting its pillar stack, threshold and overlay offset. The cartridge keeps
    /// twice the phase, the byte offset into the nine-entry rise tables, in the record word the hand uses for <see
    /// cref="ThrowsRight"/>.
    /// </summary>
    public int BodyPhase
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue / 2, value);
    }

    /// <summary>Whether the hand throws its lava rightward, chosen from Samus's side at the apex.</summary>
    public bool ThrowsRight { get; internal set; }

    /// <summary>The program last installed; null until a part's initializer installs its first.</summary>
    internal MagdolliteProgram? InstalledProgram { get; set; }

    /// <summary>The program the part's AI wants shown.</summary>
    internal MagdolliteProgram DesiredProgram { get; set; }

    /// <summary>
    /// The arm's whole-pixel displacement from where it started rising; only Y is meaningful, negative while the arm is
    /// above that point.
    /// </summary>
    public Offset16<RoomPixels> RiseDisplacement { get; internal set; }

    /// <summary>Native animationActiveFlag at extension offset $02, set and cleared by instruction callbacks to gate head transitions and completion of the hand's throw.</summary>
    public bool AnimationBusy { get; internal set; }
    /// <summary>Native emergeNotReadyFlag at extension offset $04: set by the head after submerging and cleared by the arm on landing so the head may emerge.</summary>
    public bool HeadWaitsForBodyLanding { get; internal set; }
    /// <summary>
    /// The part's whole-pixel spawn position, captured before its initializer moves it; only Y is meaningful. The head
    /// and arm return to its height, and the head's sink and surface cues are measured from it.
    /// </summary>
    public Position16<RoomPixels> Origin { get; internal set; }

    /// <summary>
    /// Native extension variable four. Only the overlay's instance is polled by the head; decrementing it in every part
    /// remains observable and matches main AI exactly. The wrapping word counts enemy updates, is reset to $0100 by the
    /// throw list, and expires when the head observes its sign bit after decrementing through zero.
    /// </summary>
    public WrappingCountdown AttackTimer { get; internal set; }

    /// <summary>
    /// Signed 16.16 downward velocity in room pixels per update, from the population's linear speed record; the falling
    /// arm consumes the head's copy.
    /// </summary>
    public SnesSignedSixteenSixteen DownwardVelocity { get; internal set; }
    /// <summary>
    /// Signed 16.16 upward velocity in room pixels per update: the speed record's separately authored negative half,
    /// not a negation; the rising arm consumes the head's copy.
    /// </summary>
    public SnesSignedSixteenSixteen UpwardVelocity { get; internal set; }
    /// <summary>Native finishedGrowingFlag at extension offset $18, set by the arm at maximum or Samus-relative height and cleared by the hand when shrinking begins.</summary>
    public bool BodyReachedApex { get; internal set; }
    /// <summary>Native notEmergedFlag at extension offset $1A: despite the host name, true holds the arm dormant; the head clears it to start growth and landing sets it again.</summary>
    public bool BodyWakeRequested { get; internal set; }
    /// <summary>Native throwXPosition/throwYPosition at extension offsets $24/$26: whole-pixel room position captured at the apex and used as the base for hand-animation shifts and restoration.</summary>
    public Position16<RoomPixels> ThrowOrigin { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        BodyPhase = 0;
        ThrowsRight = false;
        RiseDisplacement = Offset16<RoomPixels>.Zero;
    }
}
