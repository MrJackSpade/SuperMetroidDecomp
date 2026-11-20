namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The ordinary Ripper (#2146).</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="place">The place the Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class OrdinaryRipperEntity(RipperBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : RipperEntity(species, place, common)
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal override RipperEnemyState State { get; } = RipperBehavior.InitializeRipper(place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunRipperMainAi(Place, frame);
}
