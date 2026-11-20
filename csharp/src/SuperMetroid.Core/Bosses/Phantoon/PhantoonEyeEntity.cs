using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's eye (#2146), which the body positions: it opens and closes by its own animation, and takes no action of its own.</summary>
/// <param name="species">The Phantoon species owner.</param>
/// <param name="place">The place the eye occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
/// <param name="phantoon">The encounter his body began, which the part joins (#2252).</param>
internal sealed class PhantoonEyeEntity(PhantoonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork,
    PhantoonEnemyState phantoon)
    : PhantoonEntity(species, place, common, bossArtwork)
{
    /// <inheritdoc/>
    /// <remarks>The part sets up as it is built, joining the encounter (#2554).</remarks>
    internal override PhantoonPart Part { get; } = species.InitializePhantoonEye(place, phantoon);

    /// <summary>Takes no action of its own: the eye plays its own animation.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
