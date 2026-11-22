namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's head: it faces Samus, sinks into the lava to start an attack and emerges when its arm lands.</summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the head occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the head's population entry tells it: its speed and range.</param>
/// <param name="partners">What the enemy's setup built (#2554).</param>
/// <param name="followUpRecipients">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class MagdolliteHeadEntity(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, MagdolliteHeadSpawnSettings settings, MagdollitePartners partners, IReadOnlyList<RoomEnemyPlace> followUpRecipients)
    : MagdollitePartEntity(species, occupant, place, common, followUpRecipients)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Magdollite species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the head occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="surroundings">The room, Samus, controller and camera the population load sets the head up in.</param>
    /// <param name="settings">What the head's population entry tells it: its speed and range.</param>
    internal MagdolliteHeadEntity(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings, MagdolliteHeadSpawnSettings settings)
        : this(species, occupant, place, common, settings, species.InitializeMagdolliteHead(occupant, settings, place, Ensure.NotNull(surroundings).Samus),
            species.FollowUpRecipientsOf(place))
    {
    }

    /// <inheritdoc/>
    internal override MagdollitePartState State => Head;

    /// <summary>The Magdollite the head began as it was set up.</summary>
    internal override MagdollitePartners Partners { get; } = partners;

    /// <summary>The head's state, which its setup builds as the enemy is built (#2554).</summary>
    private MagdolliteHeadState Head => Partners.HeadState;

    /// <summary>What the head's population entry tells it.</summary>
    private MagdolliteHeadSpawnSettings Settings { get; } = Ensure.NotNull(settings);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) =>
        MagdolliteBehavior.RunMagdolliteHead(Settings, Place, Head, Partners, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        MagdolliteBehavior.AdvanceMagdolliteHeadAnimation(Place, Head, Partners);
}
