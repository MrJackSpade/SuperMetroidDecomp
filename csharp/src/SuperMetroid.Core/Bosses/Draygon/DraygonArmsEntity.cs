using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's arms (#2146), which his body's fight animates: they take no action of their own.</summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the arms occupy.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class DraygonArmsEntity(DraygonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : DraygonEntity(species, occupant, place, common, bossArtwork)
{
    /// <summary>The arms' part of the encounter they joined as they set up.</summary>
    internal override DraygonPart Part { get; } = species.Parts.InitializeDraygonArms(place);

    /// <summary>Takes no action of their own; his body's fight animates the arms.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
