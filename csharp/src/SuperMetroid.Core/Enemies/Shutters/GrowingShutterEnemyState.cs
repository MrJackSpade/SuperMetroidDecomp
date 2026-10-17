using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// Typed view of growing-shutter enemy variables and its extended movement state.
/// The four section origins are the independently positioned 16-pixel growth sections; they stay in
/// variables B-E because other enemies' aliased enemy-RAM reads can observe those words.
/// </summary>
public sealed class GrowingShutterEnemyState
{
    /// <summary>Pixels between consecutive growth sections' origins along the growth direction.</summary>
    private const short SectionOriginSpacing = 8;

    /// <summary>Backs native variable A-F and parameter access for one growing-shutter slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates the typed view over a room-owned shutter slot.</summary>
    internal GrowingShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ShutterProgram> Program { get; set; }

    /// <summary>Enemy variable A's bank-$A2 dispatcher pointer; initialization selects a wait function and activation replaces it with a growth function.</summary>
    public GrowingShutterFunction Function
    {
        get => (GrowingShutterFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// The timer-mode activation wait held in parameter 1: tested for expiry before each decrement, so a
    /// wait of N activates on call N + 1 and an expired wait stays expired instead of wrapping.
    /// </summary>
    public NativeCountdown ActivationTimer
    {
        get => NativeCountdown.FromRaw(checked((ushort)_slot.Parameter1));
        internal set => _slot.Parameter1 = value.RawValue;
    }

    /// <summary>
    /// The first section's origin: the spawn position. A growing shutter only ever moves vertically, so
    /// every section origin shares the shutter's own column.
    /// </summary>
    public Position16<RoomPixels> GrowthLevel0Origin => new(_slot.Position.X.Whole, _slot.VariableB);
    /// <summary>The second section's origin, eight pixels above or below the spawn position according to growth direction.</summary>
    public Position16<RoomPixels> GrowthLevel1Origin => new(_slot.Position.X.Whole, _slot.VariableC);
    /// <summary>The third section's origin, sixteen pixels from the spawn position, wrapping at the room coordinate's sixteen bits.</summary>
    public Position16<RoomPixels> GrowthLevel2Origin => new(_slot.Position.X.Whole, _slot.VariableD);
    /// <summary>The fourth section's origin, twenty-four pixels from the spawn position, wrapping at the room coordinate's sixteen bits.</summary>
    public Position16<RoomPixels> GrowthLevel3Origin => new(_slot.Position.X.Whole, _slot.VariableE);

    /// <summary>
    /// Number of completed growth sections: zero through four, where four is the fully grown terminal state.
    /// Held in variable F, which other enemies' aliased enemy-RAM reads can observe.
    /// </summary>
    public int GrowthLevel
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = checked((ushort)Ensure.BetweenInclusive(0, 4, value));
    }

    /// <summary>Positive 16.16 growth speed in pixels per update; upward growth applies its exact negation.</summary>
    public SnesSignedSixteenSixteen GrowthSpeed { get; internal set; }

    /// <summary>The shutter's whole-pixel position immediately before its current upward move, used to derive Samus's carry.</summary>
    public Position16<RoomPixels> PreviousPosition { get; internal set; }

    /// <summary>
    /// Places the four section origins from the shutter's current (spawn) position, each eight pixels further along
    /// the growth direction, wrapping at the room coordinate's sixteen bits as the cartridge's origins do.
    /// </summary>
    internal void PlaceSectionOrigins(bool growsUp)
    {
        Position16<RoomPixels> spawn = _slot.Position.Whole;
        var spacing = new Offset16<RoomPixels>(0, growsUp ? (short)-SectionOriginSpacing : SectionOriginSpacing);
        _slot.VariableB = spawn.Y;
        _slot.VariableC = (spawn + spacing).Y;
        _slot.VariableD = (spawn + spacing + spacing).Y;
        _slot.VariableE = (spawn + spacing + spacing + spacing).Y;
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
}
