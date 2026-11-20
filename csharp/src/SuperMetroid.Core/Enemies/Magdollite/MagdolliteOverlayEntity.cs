namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's tracking overlay, the hand that rides the arm's cap and throws the lava.</summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="place">The place the overlay occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MagdolliteOverlayEntity(MagdolliteBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : MagdollitePartEntity(species, place, common)
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal MagdolliteOverlayState State { get; } = species.InitializeMagdolliteOverlay(place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) =>
        Species.RunMagdolliteOverlay(Place, State, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        Species.AdvanceMagdolliteTypedAnimation<MagdolliteOverlayState>(Place, context);
}
