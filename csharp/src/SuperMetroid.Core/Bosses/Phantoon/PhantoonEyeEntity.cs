using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's eye (#2146), which the body positions: it opens and closes by its own animation, and takes no action of its own.</summary>
/// <param name="species">The Phantoon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the eye occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
/// <param name="part">What the enemy's setup built, which its base holds (#2554).</param>
internal sealed partial class PhantoonEyeEntity(PhantoonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, BossArtworkBinding bossArtwork, PhantoonPart part)
    : PhantoonEntity(species, occupant, place, common, bossArtwork, part)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Phantoon species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the eye occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="bossArtwork">The session's installed boss artwork.</param>
    /// <param name="encounter">The encounter his body began, which the eye joins as it is built.</param>
    internal PhantoonEyeEntity(PhantoonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        BossArtworkBinding bossArtwork, PhantoonEnemyState encounter)
        : this(species, occupant, place, common, bossArtwork, PhantoonBehavior.InitializePhantoonEye(encounter))
    {
    }

    /// <summary>Takes no action of its own: the eye plays its own animation.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
