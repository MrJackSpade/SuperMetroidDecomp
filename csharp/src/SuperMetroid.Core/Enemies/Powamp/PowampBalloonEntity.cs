namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>
/// A Powamp's balloon (#2163), which shares the Powamp's definition and which its body, listed right after it, moves. It
/// has no AI of its own and plays its inflate and sink animations. Every room places it intangible and nothing makes it
/// tangible, so the enemies' collision handling passes it by: no touch, shot or bomb reaches it, and it has none
/// (#2554). It keeps the Power Bomb reaction its definition leaves to it.
/// </summary>
/// <param name="species">The Powamp species owner.</param>
/// <param name="place">The place the balloon occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PowampBalloonEntity(PowampBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal PowampBalloonState State { get; } = PowampBehavior.InitializePowampBalloon(place);

    /// <summary>Does nothing: the balloon has no AI of its own, its body moves it.</summary>
    /// <param name="frame">The enemy pass's frame.</param>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PowampBehavior.AdvancePowampBalloonTypedAnimation(Place, State);


    /// <summary>The balloon's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction => LeavesPowerBombToSpecies ? species.PowampBalloonPowerBombReaction : base.PowerBombReaction;

}
