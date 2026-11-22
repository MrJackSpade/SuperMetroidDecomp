namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's tracking overlay, the hand that rides the arm's cap and throws the lava.</summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the overlay occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="partners">The Magdollite its head began, which the overlay joins as it is built (#2554).</param>
internal sealed partial class MagdolliteOverlayEntity(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    MagdollitePartners partners)
    : MagdollitePartEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    internal override MagdollitePartState State => overlay;

    /// <summary>The overlay's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly MagdolliteOverlayState overlay = species.InitializeMagdolliteOverlay(place, partners);

    /// <summary>The Magdollite its head began, which the overlay joined as it was built (#2554).</summary>
    internal override MagdollitePartners Partners { get; } = partners;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) =>
        MagdolliteBehavior.RunMagdolliteOverlay(Place, overlay, Partners, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        Species.AdvanceMagdolliteOverlayAnimation(Occupant, Place, overlay, context);
}
