namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The jet Ripper (#2146).</summary>
/// <param name="species">The Ripper species owner.</param>
/// <param name="place">The place the jet Ripper occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class JetRipperEntity(RipperBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : RipperEntity(species, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => RipperBehavior.InitializeJetRipper(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunJetRipperMainAi(Place, frame);
}
