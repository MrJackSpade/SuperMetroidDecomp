using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The state of a growing shutter: its behaviour, the four independently positioned 16-pixel growth sections' origins,
/// its growth level and its extended movement state. The values the cartridge keeps in the enemy's common record are
/// emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class GrowingShutterEnemyState
{
    /// <summary>Pixels between consecutive growth sections' origins along the growth direction.</summary>
    private const int SectionOriginSpacing = 8;

    /// <summary>The number of growth sections, each with a stored origin; a fully grown shutter is at this level.</summary>
    internal const int SectionCount = 4;

    /// <summary>The shutter's slot, whose position the state reads.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Each growth section's stored origin row, indexed by growth level; the column is always the shutter's own.</summary>
    private readonly Position16<RoomPixels>[] _sectionOrigins = new Position16<RoomPixels>[SectionCount];

    /// <summary>Creates the state for a room-owned shutter slot.</summary>
    internal GrowingShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ShutterProgram> Program { get; set; }

    /// <summary>
    /// The dispatcher behaviour; initialization selects a wait function and activation replaces it with a
    /// growth function. None once a kill has emptied the record.
    /// </summary>
    public GrowingShutterFunction? Function { get; internal set; }

    /// <summary>
    /// The timer-mode activation wait: tested for expiry before each decrement, so a wait of N activates on call N + 1
    /// and an expired wait stays expired instead of wrapping.
    /// </summary>
    public WrappingCountdown ActivationTimer { get; internal set; }

    /// <summary>The proximity-mode distance in pixels: the shutter activates once Samus is horizontally within it.</summary>
    public int ActivationDistance
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// The origin of the section grown at <paramref name="level"/>, indexed by growth level: level 0 is the spawn position
    /// and each later level is eight pixels further along the growth direction, wrapping at the room coordinate's sixteen
    /// bits. A growing shutter only ever moves vertically, so every section origin shares the shutter's own column.
    /// </summary>
    /// <param name="level">The growth level, 0 through 3.</param>
    public Position16<RoomPixels> SectionOrigin(int level) => new(_slot.Position.X.Whole, _sectionOrigins[level].Y);

    /// <summary>Stores the origin row of the section grown at <paramref name="level"/>.</summary>
    /// <param name="level">The growth level, 0 through 3.</param>
    /// <param name="origin">The section's origin; only its row is kept.</param>
    internal void SetSectionOrigin(int level, Position16<RoomPixels> origin) => _sectionOrigins[level] = new(0, origin.Y);

    /// <summary>Number of completed growth sections: zero through four, where four is the fully grown terminal state.</summary>
    public int GrowthLevel
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, SectionCount, value);
    }

    /// <summary>Positive 16.16 growth speed in pixels per update; upward growth applies its exact negation.</summary>
    public SignedSixteenSixteen GrowthSpeed { get; internal set; }

    /// <summary>The shutter's whole-pixel position immediately before its current upward move, used to derive Samus's carry.</summary>
    public Position16<RoomPixels> PreviousPosition { get; internal set; }

    /// <summary>
    /// Places the four section origins from the shutter's current (spawn) position, each eight pixels further along the
    /// growth direction, wrapping at the room coordinate's sixteen bits as the cartridge's origins do.
    /// </summary>
    internal void PlaceSectionOrigins(bool growsUp)
    {
        Position16<RoomPixels> spawn = _slot.Position.Whole;
        var spacing = new Offset16<RoomPixels>(0, growsUp ? CoordinateAxis.AsSigned(-SectionOriginSpacing) : SectionOriginSpacing);
        Position16<RoomPixels> origin = spawn;
        for (int level = 0; level < SectionCount; level++)
        {
            SetSectionOrigin(level, origin);
            origin += spacing;
        }
    }

    /// <summary>Selects the current growth section's stored origin; level four has no next section.</summary>
    internal Position16<RoomPixels> OriginForLevel() => GrowthLevel < SectionCount
        ? SectionOrigin(GrowthLevel)
        : throw new InvalidOperationException($"Growing shutter level {GrowthLevel} has no section origin.");

    /// <summary>Empties the values the cartridge keeps in the enemy's common record: no behaviour, no origins, level zero.</summary>
    internal void ClearWithRecord()
    {
        ActivationTimer = WrappingCountdown.Start(0);
        ActivationDistance = 0;
        Function = null;
        Array.Clear(_sectionOrigins);
        GrowthLevel = 0;
    }
}
