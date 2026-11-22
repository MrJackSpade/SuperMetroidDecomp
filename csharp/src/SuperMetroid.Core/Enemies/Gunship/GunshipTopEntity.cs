namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>The gunship's top hull (#2146), which moves the whole ship: its own AI, with the shared reactions.</summary>
/// <param name="species">The Gunship species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the top hull occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="surroundings">The room, Samus, controller and camera the population load sets the ship up in.</param>
internal sealed partial class GunshipTopEntity(GunshipBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    EnemyPopulationSurroundings surroundings)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The constructor's <c>surroundings</c>, kept so a debugger state rebuilds the object through it (#2602).</summary>
    private readonly EnemyPopulationSurroundings surroundings = surroundings;

    /// <summary>The ship the top hull holds from its setup (#2554).</summary>
    internal GunshipTop State { get; } = species.InitializeGunshipTop(place, surroundings.Samus);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunGunshipMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GunshipBehavior.AdvanceGunshipAnimation(Place, State.Hull);
}
