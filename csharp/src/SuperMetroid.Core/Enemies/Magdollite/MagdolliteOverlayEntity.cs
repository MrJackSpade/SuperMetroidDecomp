namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's tracking overlay, the hand that rides the arm's cap and throws the lava.</summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the overlay occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MagdolliteOverlayEntity(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : MagdollitePartEntity(species, occupant, place, common), IEnemyInitialization
{
    /// <inheritdoc/>
    internal override MagdollitePartState State => Overlay;

    /// <summary>The overlay's state, which its setup builds as the enemy is built (#2554).</summary>
    private MagdolliteOverlayState Overlay { get; } = species.InitializeMagdolliteOverlay(place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) =>
        Species.RunMagdolliteOverlay(Place, Overlay, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        Species.AdvanceMagdolliteTypedAnimation<MagdolliteOverlayState>(Occupant, Place, Overlay, context);
}
