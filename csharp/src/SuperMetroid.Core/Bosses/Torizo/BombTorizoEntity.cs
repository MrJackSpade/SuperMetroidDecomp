using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb Torizo (#2146): the Torizo's shared participation, with Bomb Torizo's fight and shot reaction.</summary>
/// <param name="species">The Torizo species owner.</param>
/// <param name="place">The place Bomb Torizo occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="setup">The population load that places the Torizo.</param>
internal sealed class BombTorizoEntity(TorizoBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup)
    : TorizoEntity(species, place, common, TorizoVariant.Bomb, setup)
{
    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunBombTorizoMainAi(Place, frame);

    /// <summary>Bomb Torizo's shot reaction, which its definition leaves to it.</summary>
    public override EnemyReaction? SpeciesShot => TorizoReactions.Shot;

    /// <inheritdoc/>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        Species.Combat.ApplyBombTorizoHurt(Place);
        return true;
    }

    /// <inheritdoc/>
    protected override EnemyShotReaction OwnShotReaction => Species.Combat.BombTorizoShotReaction;

    /// <inheritdoc/>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        Species.Combat.ReactToBombTorizoBomb(context);
        return true;
    }
}
