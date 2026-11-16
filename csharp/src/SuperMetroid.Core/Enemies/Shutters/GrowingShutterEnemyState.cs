using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The state of a growing shutter: its behaviour, the four independently positioned 16-pixel growth sections' origins,
/// its growth level and its extended movement state.
/// </summary>
public sealed class GrowingShutterEnemyState
{
    /// <summary>Pixels between consecutive growth sections' origins along the growth direction.</summary>
    private const int SectionOriginSpacing = 8;

    /// <summary>The number of growth sections, each with a stored origin; a fully grown shutter is at this level.</summary>
    internal const int SectionCount = 4;

    /// <summary>Each growth section's origin row, indexed by growth level.</summary>
    private readonly int[] _sectionOriginYs = new int[SectionCount];

    /// <summary>Creates the state starting in <paramref name="action"/>.</summary>
    internal GrowingShutterEnemyState(GrowingShutterAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ShutterSegment> Playback { get; set; }

    /// <summary>
    /// The current behaviour; initialization selects a wait action and activation replaces it with a growth
    /// action.
    /// </summary>
    public GrowingShutterAction Action { get; internal set; }

    /// <summary>
    /// The timer-mode activation wait: tested for expiry before each decrement, so a wait of N activates on call N + 1
    /// and an expired wait stays expired instead of wrapping.
    /// </summary>
    public int ActivationTimer { get; internal set; }

    /// <summary>The proximity-mode distance in pixels: the shutter activates once Samus is horizontally within it.</summary>
    public int ActivationDistance { get; internal set; }

    /// <summary>
    /// The origin row of the section grown at <paramref name="level"/>, indexed by growth level: level 0 is the spawn row
    /// and each later level is eight pixels further along the growth direction, wrapping around the room. A growing shutter
    /// only ever moves vertically, so a section's origin is a row in the shutter's own column.
    /// </summary>
    /// <param name="level">The growth level, 0 through 3.</param>
    public int SectionOriginY(int level) => _sectionOriginYs[level];

    /// <summary>Stores the origin row of the section grown at <paramref name="level"/>.</summary>
    /// <param name="level">The growth level, 0 through 3.</param>
    /// <param name="originY">The section's origin row.</param>
    internal void SetSectionOriginY(int level, int originY) => _sectionOriginYs[level] = originY;

    /// <summary>Number of completed growth sections: zero through four, where four is the fully grown terminal state.</summary>
    public int GrowthLevel
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, SectionCount, value);
    }

    /// <summary>Positive 1/65536-pixel growth speed in pixels per update; upward growth applies its exact negation.</summary>
    public SignedSixteenSixteen GrowthSpeed { get; internal set; }

    /// <summary>The shutter's whole-pixel position immediately before its current upward move, used to derive Samus's carry.</summary>
    public Position16<RoomPixels> PreviousPosition { get; internal set; }

    /// <summary>
    /// Places the four section origins from the shutter's spawn row <paramref name="spawnY"/>, each eight pixels further
    /// along the growth direction, wrapping around the room.
    /// </summary>
    internal void PlaceSectionOrigins(int spawnY, bool growsUp)
    {
        int spacing = growsUp ? -SectionOriginSpacing : SectionOriginSpacing;
        int originY = spawnY;
        for (int level = 0; level < SectionCount; level++)
        {
            SetSectionOriginY(level, originY);
            originY = CoordinateAxis.Wrap(originY + spacing);
        }
    }

    /// <summary>Selects the current growth section's stored origin row; level four has no next section.</summary>
    internal int OriginYForLevel() => GrowthLevel < SectionCount
        ? SectionOriginY(GrowthLevel)
        : throw new InvalidOperationException($"Growing shutter level {GrowthLevel} has no section origin.");
}
