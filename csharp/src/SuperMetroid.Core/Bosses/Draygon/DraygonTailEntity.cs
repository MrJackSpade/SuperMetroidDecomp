using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's tail (#2146), which his body's fight animates: it takes no action of its own.</summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="place">The place the tail occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
/// <param name="draygon">The encounter his body began, which the part joins.</param>
internal sealed class DraygonTailEntity(DraygonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork,
    DraygonEnemyState draygon)
    : DraygonEntity(species, place, common, bossArtwork)
{
    /// <summary>The tail' part of the encounter, which it joins as it is built (#2554).</summary>
    internal override DraygonPart Part { get; } = DraygonParts.InitializeDraygonTail(place, draygon);

    /// <summary>Takes no action of its own; his body's fight animates the tail.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
