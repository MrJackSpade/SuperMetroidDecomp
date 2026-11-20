using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The state of a growing shutter: its behaviour, its spawn row and growth direction, from which its growth
/// sections' origins follow, its growth level and its extended movement state. It reads how near Samus must come and how
/// fast it grows from its spawn settings where it uses them (#2450).
/// </summary>
public sealed class GrowingShutterEnemyState
{
    /// <summary>Pixels between consecutive growth sections' origins along the growth direction.</summary>
    private const int SectionOriginSpacing = 8;

    /// <summary>
    /// The growth sections in the order they grow, each by the artwork the shutter shows while it grows: a quarter, a half,
    /// three quarters and the whole of its length (#2553).
    /// </summary>
    private static readonly ShutterSegment[] Sections =
        [ShutterSegment.QuarterGrown, ShutterSegment.HalfGrown, ShutterSegment.ThreeQuartersGrown, ShutterSegment.FullyGrown];

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
    public int SpawnY { get; private set; }

    /// <summary>The way the shutter grows, from its spawn row.</summary>
    public VerticalDirection Growth { get; }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ShutterSegment> Playback { get; set; }

    /// <summary>
    /// The current behaviour; initialization selects a wait action and activation replaces it with growth, which grows
    /// the shutter the way <see cref="Growth"/> says (#2512).
    /// </summary>
    public GrowingShutterAction Action { get; internal set; }

    /// <summary>
    /// The timer-mode wait before the shutter grows: it grows on the update after its placement's wait
    /// (<see cref="EnemyWaitDefinitions.RunOutUpdate"/>) (#2281).
    /// </summary>
    public UpdateCountdown ActivationTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The section growing, by its place in the growth order and the artwork it shows, or null once every section has grown
    /// and the shutter grows no further (#2182).
    /// </summary>
    internal ListStep<ShutterSegment>? Growing { get; set; } = FirstSection;

    /// <summary>The first section to grow, a quarter of the shutter's length.</summary>
    internal static ListStep<ShutterSegment> FirstSection => ListStep<ShutterSegment>.FirstOf(Sections);

    /// <summary>
    /// The origin row of growing section <paramref name="section"/>, derived from the spawn row (#2183): the first section's
    /// is the spawn row and each later one's is <see cref="SectionOriginSpacing"/> further along the growth direction, down
    /// or up. A growing shutter only ever moves vertically, so a section's origin is a row in the shutter's own column.
    /// </summary>
    internal int OriginY(ListStep<ShutterSegment> section) => SpawnY + section.Place * SectionOriginSpacing.Toward(Growth);
}
