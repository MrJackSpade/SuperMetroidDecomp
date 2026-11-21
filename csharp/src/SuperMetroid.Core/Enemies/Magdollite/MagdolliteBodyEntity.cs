namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>A Magdollite's rising body, the arm that grows out of the lava under the head and sinks back.</summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="partners">The Magdollite its head began, which the body joins as it is built (#2554).</param>
internal sealed class MagdolliteBodyEntity(MagdolliteBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    MagdollitePartners partners)
    : MagdollitePartEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    internal override MagdollitePartState State => Body;

    /// <summary>The body's state, which its setup builds as the enemy is built (#2554).</summary>
    internal MagdolliteBodyState Body { get; } = species.InitializeMagdolliteBody(place, partners);

    /// <summary>The Magdollite its head began, which the body joined as it was built (#2554).</summary>
    internal override MagdollitePartners Partners { get; } = partners;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) =>
        Species.RunMagdolliteBody(Place, Body, Partners, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) =>
        MagdolliteBehavior.AdvanceMagdolliteBodyAnimation(Place, Body);
}
