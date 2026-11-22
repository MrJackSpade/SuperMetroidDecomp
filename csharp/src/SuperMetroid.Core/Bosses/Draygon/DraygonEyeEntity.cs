using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's eye (#2146): inert until his body faces a way, then it tracks Samus.</summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the eye occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
/// <param name="part">What the enemy's setup built (#2554).</param>
internal sealed partial class DraygonEyeEntity(DraygonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, BossArtworkBinding bossArtwork, DraygonPart part)
    : DraygonEntity(species, occupant, place, common, bossArtwork)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Draygon species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the eye occupies.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="bossArtwork">The session's installed boss artwork.</param>
    /// <param name="draygon">The encounter his body began, which the eye joins as it is built.</param>
    internal DraygonEyeEntity(DraygonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        BossArtworkBinding bossArtwork, DraygonEnemyState draygon)
        : this(species, occupant, place, common, bossArtwork, DraygonParts.InitializeDraygonEye(place, draygon))
    {
    }

    /// <summary>The eye's part of the encounter it joined as it set up.</summary>
    internal override DraygonPart Part { get; } = part;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Parts.RunDraygonEyeMain(Encounter, Ensure.NotNull(frame).Samus);
}
