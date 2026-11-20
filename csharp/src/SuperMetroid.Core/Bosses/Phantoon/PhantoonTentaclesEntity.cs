using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's tentacles (#2146), a drawing part that plays its own animation, and takes no action of its own.</summary>
/// <param name="species">The Phantoon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the tentacles occupy.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
/// <param name="encounter">The encounter his body began, which the tentacles joins as it is built.</param>
internal sealed class PhantoonTentaclesEntity(PhantoonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork,
    PhantoonEnemyState encounter)
    : PhantoonEntity(species, occupant, place, common, bossArtwork, PhantoonBehavior.InitializePhantoonTentacles(encounter))
{
    /// <summary>Takes no action of its own: the tentacles plays its own animation.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
