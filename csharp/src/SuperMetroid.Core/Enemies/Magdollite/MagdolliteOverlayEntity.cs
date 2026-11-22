namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's tracking overlay, the hand that rides the arm's cap and throws the lava.</summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the overlay occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="partners">The Magdollite its head began, which the overlay joins as it is built (#2554).</param>
/// <param name="overlay">What the enemy's setup built (#2554).</param>
/// <param name="followUpRecipients">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class MagdolliteOverlayEntity(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, MagdollitePartners partners, MagdolliteOverlayState overlay, IReadOnlyList<RoomEnemyPlace> followUpRecipients)
    : MagdollitePartEntity(species, occupant, place, common, followUpRecipients)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Magdollite species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the overlay occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="partners">The Magdollite its head began, which the overlay joins as it is built (#2554).</param>
    internal MagdolliteOverlayEntity(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        MagdollitePartners partners)
        : this(species, occupant, place, common, partners, species.InitializeMagdolliteOverlay(place, partners), species.FollowUpRecipientsOf(place))
    {
    }

    /// <summary>The overlay's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly MagdolliteOverlayState overlay = overlay;

    /// <summary>The Magdollite its head began, which the overlay joined as it was built (#2554).</summary>
    internal override MagdollitePartners Partners { get; } = partners;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) =>
        MagdolliteBehavior.RunMagdolliteOverlay(Place, overlay, Partners, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        Species.AdvanceMagdolliteOverlayAnimation(Occupant, Place, overlay, context);
}
