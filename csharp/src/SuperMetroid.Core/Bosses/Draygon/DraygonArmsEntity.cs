using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's arms (#2146), which his body's fight animates: they take no action of their own.</summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="place">The place the arms occupy.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class DraygonArmsEntity(DraygonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : DraygonEntity(species, place, common, bossArtwork)
{
    /// <inheritdoc/>
    public override void Initialize() => Species.Parts.InitializeDraygonArms(Place);

    /// <summary>The arms' part of the encounter they joined as they set up.</summary>
    internal override DraygonPart Part => Place.RequireSpeciesState<DraygonPart>();

    /// <summary>Takes no action of their own; his body's fight animates the arms.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
