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
    /// <summary>The arms' part of the encounter, which it joins as it is built (#2554).</summary>
    internal override DraygonPart Part { get; } = species.Parts.InitializeDraygonArms(place);

    /// <summary>Takes no action of their own; his body's fight animates the arms.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
