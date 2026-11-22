using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>One Shaktool occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Shaktool species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
/// <param name="chain">The chain the segment is in, which its leading saw started (#2452, #2554).</param>
/// <param name="course">How the segment moves, holding the state its setup built (#2554).</param>
[RoomLifetime]
internal sealed partial class ShaktoolEntity(ShaktoolBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, ShaktoolSpawnSettings settings, ShaktoolChain chain, IShaktoolCourse course)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Shaktool species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the enemy occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="settings">What the enemy's population entry tells its species.</param>
    /// <param name="chain">The chain the segment is in, which its leading saw started (#2452, #2554).</param>
    internal ShaktoolEntity(ShaktoolBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        ShaktoolSpawnSettings settings, ShaktoolChain chain)
        : this(species, occupant, place, common, settings, chain,
            species.CourseOf(place, ShaktoolBehavior.InitializeShaktool(settings, place, chain), settings.Segment, chain))
    {
    }

    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal ShaktoolSpawnSettings Settings { get; } = settings;

    /// <summary>The chain the segment is in: its seven segments and their shared motion and target.</summary>
    private ShaktoolChain Chain { get; } = chain;

    /// <summary>
    /// How the segment moves while its motion runs, as its definition names it, found as it is built, holding the state its
    /// setup builds as the enemy is built (#2554).
    /// </summary>
    private IShaktoolCourse Course { get; } = course;

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => ShaktoolBehavior.RunShaktoolMainAi(Course, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ShaktoolBehavior.AdvanceShaktoolTypedAnimation(Place, Course.State, Chain);

    /// <summary>Whether Shaktool's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == ShaktoolReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs Shaktool's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == ShaktoolReactions.Touch)
            species.ReactToShaktoolTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>Shaktool's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == ShaktoolReactions.Shot ? species.ShaktoolShotReaction : base.ShotReaction;

    /// <summary>Shaktool's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == ShaktoolReactions.Shot ? species.ShaktoolBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>Shaktool's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == ShaktoolReactions.Shot;
}
