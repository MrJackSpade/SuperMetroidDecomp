using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb Torizo (#2146): the Torizo's shared participation, with Bomb Torizo's fight and shot reaction.</summary>
/// <param name="species">The Torizo species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place Bomb Torizo occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BombTorizoEntity(TorizoBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : TorizoEntity(species, occupant, place, common)
{
    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunBombTorizoMainAi(Occupant, Place, frame);

    /// <summary>He is Crateria's Bomb Torizo.</summary>
    protected override TorizoVariant Variant => TorizoVariant.Bomb;

    /// <summary>The shot reaction of this Torizo's own that its definition names (#2558).</summary>
    protected override EnemyReaction OwnShot => TorizoReactions.Shot;

    /// <inheritdoc/>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        Species.Combat.ApplyBombTorizoHurt(Occupant, Place);
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
