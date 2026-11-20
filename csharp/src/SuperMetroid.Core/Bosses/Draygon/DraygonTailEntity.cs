using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's tail (#2146), which his body's fight animates: it takes no action of its own.</summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the tail occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class DraygonTailEntity(DraygonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : DraygonEntity(species, occupant, place, common, bossArtwork)
{
    /// <inheritdoc/>
    public override void Initialize() => Species.Parts.InitializeDraygonTail(Place);

    /// <summary>The tail's part of the encounter it joined as it set up.</summary>
    internal override DraygonPart Part => Place.RequireSpeciesState<DraygonPart>();

    /// <summary>Takes no action of its own; his body's fight animates the tail.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
