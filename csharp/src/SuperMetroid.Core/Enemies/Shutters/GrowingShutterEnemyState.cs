using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The state of a growing shutter: its behaviour, its spawn row and growth direction, from which its four 16-pixel growth
/// sections' origins follow, its growth level and its extended movement state.
/// </summary>
public sealed class GrowingShutterEnemyState
{
    /// <summary>Pixels between consecutive growth sections' origins along the growth direction.</summary>
    private const int SectionOriginSpacing = 8;

    /// <summary>The number of growth sections, each with a stored origin; a fully grown shutter is at this level.</summary>
    internal const int SectionCount = 4;

    /// <summary>
    /// Creates the state of a shutter spawned at row <paramref name="spawnY"/> growing <paramref name="growth"/>, starting in
    /// <paramref name="action"/>.
    /// </summary>
    internal GrowingShutterEnemyState(GrowingShutterAction action, int spawnY, VerticalDirection growth)
    {
        Action = action;
        SpawnY = spawnY;
        Growth = growth;
    }

    /// <summary>The row the shutter spawned at, the first section's origin (#2183).</summary>
    public int SpawnY { get; internal set; }

    /// <summary>The way the shutter grows, from its spawn row.</summary>
    public VerticalDirection Growth { get; }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ShutterSegment> Playback { get; set; }

    /// <summary>
    /// The current behaviour; initialization selects a wait action and activation replaces it with a growth
    /// action.
    /// </summary>
    public GrowingShutterAction Action { get; internal set; }

    /// <summary>
    /// The timer-mode activation updates left, the one it grows on included: the placement's wait and the call it runs out on
    /// (<see cref="EnemyWaitDefinitions.RunOutUpdate"/>), so a wait of N grows on call N + 1 (#2281).
    /// </summary>
    public int ActivationTimer { get; internal set; }

    /// <summary>The proximity-mode distance in pixels: the shutter activates once Samus is horizontally within it.</summary>
    public int ActivationDistance { get; internal set; }

    /// <summary>
    /// The origin row of the section grown at <paramref name="level"/>, derived from the spawn row (#2183): level 0 is the
    /// spawn row and each later level is eight pixels further along the growth direction. Every placement grows down
    /// from inside the room. A growing shutter only ever moves vertically, so a section's origin is a row
    /// in the shutter's own column.
    /// </summary>
    /// <param name="level">The growth level, 0 through 3.</param>
    public int SectionOriginY(int level) =>
        SpawnY + Ensure.BetweenInclusive(0, SectionCount - 1, level) *
            (Growth == VerticalDirection.Up ? -SectionOriginSpacing : SectionOriginSpacing);

    /// <summary>Number of completed growth sections: zero through four, where four is the fully grown terminal state.</summary>
    public int GrowthLevel
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, SectionCount, value);
    }

    /// <summary>Positive growth speed in pixels per update; upward growth applies its exact negation.</summary>
    public SignedSixteenSixteen GrowthSpeed { get; internal set; }

    /// <summary>
    /// The shutter's whole-pixel row immediately before its current upward move, used to derive Samus's carry (#2178): it
    /// only ever moves vertically.
    /// </summary>
    public int PreviousY { get; internal set; }

    /// <summary>Whether every section has grown (#2182): the shutter grows no further.</summary>
    internal bool IsFullyGrown => GrowthLevel == SectionCount;

    /// <summary>Selects the current growth section's stored origin row; a fully grown shutter has no next section.</summary>
    internal int OriginYForLevel() => GrowthLevel < SectionCount
        ? SectionOriginY(GrowthLevel)
        : throw new InvalidOperationException($"Growing shutter level {GrowthLevel} has no section origin.");
}
