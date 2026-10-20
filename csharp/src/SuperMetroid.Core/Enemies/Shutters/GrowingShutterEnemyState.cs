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
    private const short SectionOriginSpacing = 8;

    /// <summary>The shutter's slot, whose position the state reads.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates the state for a room-owned shutter slot.</summary>
    internal GrowingShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ShutterProgram> Program { get; set; }

    /// <summary>
    /// The bank-$A2 dispatcher behaviour; initialization selects a wait function and activation replaces it with a
    /// growth function. None once a kill has emptied the record.
    /// </summary>
    public GrowingShutterFunction? Function { get; internal set; }

    /// <summary>The authored activation word, parameter one: a timer-mode wait or a proximity-mode distance.</summary>
    private int _activationWord;

    /// <summary>
    /// The timer-mode activation wait: tested for expiry before each decrement, so a wait of N activates on call N + 1
    /// and an expired wait stays expired instead of wrapping.
    /// </summary>
    public NativeCountdown ActivationTimer
    {
        get => NativeCountdown.FromRaw(checked((ushort)_activationWord));
        internal set => _activationWord = value.RawValue;
    }

    /// <summary>The proximity-mode distance in pixels: the shutter activates once Samus is horizontally within it.</summary>
    public int ActivationDistance
    {
        get => _activationWord;
        internal set => _activationWord = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// The first section's origin: the spawn position. A growing shutter only ever moves vertically, so every section
    /// origin shares the shutter's own column.
    /// </summary>
    public Position16<RoomPixels> GrowthLevel0Origin
    {
        get => new(_slot.Position.X.Whole, field.Y);
        internal set => field = new(0, value.Y);
    }
    /// <summary>The second section's origin, eight pixels above or below the spawn position according to growth direction.</summary>
    public Position16<RoomPixels> GrowthLevel1Origin
    {
        get => new(_slot.Position.X.Whole, field.Y);
        internal set => field = new(0, value.Y);
    }
    /// <summary>The third section's origin, sixteen pixels from the spawn position, wrapping at the room coordinate's sixteen bits.</summary>
    public Position16<RoomPixels> GrowthLevel2Origin
    {
        get => new(_slot.Position.X.Whole, field.Y);
        internal set => field = new(0, value.Y);
    }
    /// <summary>The fourth section's origin, twenty-four pixels from the spawn position, wrapping at the room coordinate's sixteen bits.</summary>
    public Position16<RoomPixels> GrowthLevel3Origin
    {
        get => new(_slot.Position.X.Whole, field.Y);
        internal set => field = new(0, value.Y);
    }

    /// <summary>Number of completed growth sections: zero through four, where four is the fully grown terminal state.</summary>
    public int GrowthLevel
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, 4, value);
    }

    /// <summary>Positive 16.16 growth speed in pixels per update; upward growth applies its exact negation.</summary>
    public SnesSignedSixteenSixteen GrowthSpeed { get; internal set; }

    /// <summary>The shutter's whole-pixel position immediately before its current upward move, used to derive Samus's carry.</summary>
    public Position16<RoomPixels> PreviousPosition { get; internal set; }

    /// <summary>
    /// Places the four section origins from the shutter's current (spawn) position, each eight pixels further along the
    /// growth direction, wrapping at the room coordinate's sixteen bits as the cartridge's origins do.
    /// </summary>
    internal void PlaceSectionOrigins(bool growsUp)
    {
        Position16<RoomPixels> spawn = _slot.Position.Whole;
        var spacing = new Offset16<RoomPixels>(0, growsUp ? (short)-SectionOriginSpacing : SectionOriginSpacing);
        GrowthLevel0Origin = spawn;
        GrowthLevel1Origin = spawn + spacing;
        GrowthLevel2Origin = spawn + spacing + spacing;
        GrowthLevel3Origin = spawn + spacing + spacing + spacing;
    }

    /// <summary>Selects the current growth section's stored origin; level four has no next section.</summary>
    internal Position16<RoomPixels> OriginForLevel() => GrowthLevel switch
    {
        0 => GrowthLevel0Origin,
        1 => GrowthLevel1Origin,
        2 => GrowthLevel2Origin,
        3 => GrowthLevel3Origin,
        _ => throw new InvalidOperationException(
            $"Growing shutter level {GrowthLevel} has no section origin."),
    };

    /// <summary>Empties the values the cartridge keeps in the enemy's common record: no behaviour, no origins, level zero.</summary>
    internal void ClearWithRecord()
    {
        _activationWord = 0;
        Function = null;
        GrowthLevel0Origin = GrowthLevel1Origin = GrowthLevel2Origin = GrowthLevel3Origin = Position16<RoomPixels>.Zero;
        GrowthLevel = 0;
    }
}
