namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// A Dragon's cosmetic wing (#2175), which shares the Dragon's definition: its placement names it the wing of the body its
/// room lists right before it, which moves it and starts its flapping. It has no AI of its own, and keeps the Dragon's
/// reactions where its definition leaves them to it; its intangibility normally keeps Samus and her shots from reaching it.
/// </summary>
/// <param name="species">The Dragon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the wing occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="ownState">What the enemy's setup built (#2554).</param>
/// <param name="followUpRecipient">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class DragonWingEntity(DragonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, DragonWingState ownState, RoomEnemyPlace followUpRecipient)
    : DragonPartEntity(species, occupant, place, common, followUpRecipient), IEnemyTypedAnimation
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Dragon species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the wing occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="state">The wing's state: the one its body began as it was placed, or, for a wing no body was listed before, its own.</param>
    /// <returns>The enemy, set up.</returns>
    internal static DragonWingEntity SetUp(DragonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        DragonWingState state) =>
        new(species, occupant, place, common, DragonBehavior.InitializeDragonWing(place, state), species.FollowUpRecipientOf(place));

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly DragonWingState ownState = ownState;

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DragonBehavior.AdvanceDragonWingTypedAnimation(Place, ownState);
}
