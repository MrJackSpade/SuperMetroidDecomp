using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's mouth (#2146), a drawing part that plays its own animation, and takes no action of its own.</summary>
/// <param name="species">The Phantoon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the mouth occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
/// <param name="phantoon">The encounter his body began, which the part joins (#2252).</param>
internal sealed class PhantoonMouthEntity(PhantoonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork,
    PhantoonEnemyState phantoon)
    : PhantoonEntity(species, occupant, place, common, bossArtwork)
{
    /// <inheritdoc/>
    /// <remarks>The part sets up as it is built, joining the encounter (#2554).</remarks>
    internal override PhantoonPart Part { get; } = PhantoonBehavior.InitializePhantoonMouth(phantoon);

    /// <summary>Takes no action of its own: the mouth plays its own animation.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
