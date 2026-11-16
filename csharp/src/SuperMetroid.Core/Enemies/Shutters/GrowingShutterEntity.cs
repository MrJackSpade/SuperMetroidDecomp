namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing shutter (#2146): it grows and shrinks by its own AI and animation, and takes the destroyable shutter's shot
/// reaction where its definition leaves shots to it.
/// </summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="place">The place the shutter occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GrowingShutterEntity(ShuttersBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : ShuttersEntity(species, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => ShuttersBehavior.InitializeGrowingShutter(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunShutterGrowingMainAi(Place, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ShuttersBehavior.AdvanceShutterGrowingTypedAnimation(Place, context);

    /// <summary>The destroyable shutter's shot reaction where the definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? Species.DestroyableShutterShotReaction : base.ShotReaction;
}
