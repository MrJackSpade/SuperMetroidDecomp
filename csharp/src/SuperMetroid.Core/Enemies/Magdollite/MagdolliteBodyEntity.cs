namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's rising body, the arm that grows out of the lava under the head and sinks back.</summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="partners">The Magdollite its head began, which the body joins as it is built (#2554).</param>
/// <param name="body">What the enemy's setup built (#2554).</param>
/// <param name="followUpRecipients">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class MagdolliteBodyEntity(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, MagdollitePartners partners, MagdolliteBodyState body, IReadOnlyList<RoomEnemyPlace> followUpRecipients)
    : MagdollitePartEntity(species, occupant, place, common, followUpRecipients)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Magdollite species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the body occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="partners">The Magdollite its head began, which the body joins as it is built (#2554).</param>
    internal MagdolliteBodyEntity(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        MagdollitePartners partners)
        : this(species, occupant, place, common, partners, species.InitializeMagdolliteBody(place, partners), species.FollowUpRecipientsOf(place))
    {
    }

    /// <inheritdoc/>
    internal override MagdollitePartState State => body;

    /// <summary>The body's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly MagdolliteBodyState body = body;

    /// <summary>The Magdollite its head began, which the body joined as it was built (#2554).</summary>
    internal override MagdollitePartners Partners { get; } = partners;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) =>
        Species.RunMagdolliteBody(Place, body, Partners, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        MagdolliteBehavior.AdvanceMagdolliteBodyAnimation(Place, body);
}
