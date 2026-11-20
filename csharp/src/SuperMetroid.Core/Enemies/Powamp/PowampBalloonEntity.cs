namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// A Powamp's balloon (#2163), which shares the Powamp's definition and which its body, listed right after it, moves. It
/// has no AI of its own and plays its inflate and sink animations. Every room places it intangible and nothing makes it
/// tangible, so the enemies' collision handling passes it by: no touch, shot or bomb reaches it, and it has none
/// (#2554). It keeps the Power Bomb reaction its definition leaves to it.
/// </summary>
/// <param name="species">The Powamp species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the balloon occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">How far the grapple pulls the balloon, as its placement names.</param>
internal sealed class PowampBalloonEntity(PowampBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, PowampSpawnSettings settings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyTypedAnimation
{
    /// <summary>The balloon's own shot reaction, built for this occupant.</summary>
    private readonly EnemyShotReaction ownShot = species.PowampBalloonShotReaction(occupant);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal PowampBalloonState State { get; } = PowampBehavior.InitializePowampBalloon(place, settings);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PowampBehavior.AdvancePowampBalloonTypedAnimation(Place, State);


    /// <summary>The balloon's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction => LeavesPowerBombToSpecies ? species.PowampBalloonPowerBombReaction : base.PowerBombReaction;

}
