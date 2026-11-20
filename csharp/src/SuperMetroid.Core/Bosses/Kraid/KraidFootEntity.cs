using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's foot (#2146): it follows his walk, with the limbs' own shot and bomb reactions.</summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the foot occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="encounter">The encounter his body began, which the part joins as it is built.</param>
internal sealed class KraidFootEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    KraidEnemyState encounter)
    : KraidLimbEntity(species, occupant, place, common, species.Parts.FootReference, () => species.Limbs.InitializeKraidFoot(place, encounter))
{

    /// <summary>The foot's own part of the encounter.</summary>
    protected override KraidAnimatedPartState Part => Encounter.FootState;

    /// <summary>The foot's hitboxes, which its pose places.</summary>
    public override EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe) =>
        KraidCombat.FindKraidFootHitboxReaction(Place, Part.Pose, probe);

    /// <summary>The foot's shot reaction.</summary>
    protected override EnemyShotReaction LimbShotReaction => KraidCombat.FootShotReaction;

    /// <summary>The foot's bomb reaction.</summary>
    protected override void ReactToLimbBomb(EnemyBombContext context) => KraidCombat.ReactToFootBomb(context);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Limbs.RunKraidFootMain(Place, Encounter.FootState, frame.Camera.Y);
}
