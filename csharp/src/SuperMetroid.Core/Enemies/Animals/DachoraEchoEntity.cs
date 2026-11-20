namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// One of a Dachora's speed-boost echoes (#2283): its own flicker and pose, placed and timed by the body it trails, with the
/// shared reactions.
/// </summary>
/// <param name="place">The place the echo occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DachoraEchoEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => DachoraBehavior.InitializeDachoraEcho(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => DachoraBehavior.RunDachoraEchoMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => DachoraBehavior.AdvanceDachoraEchoTypedAnimation(Place);
}
