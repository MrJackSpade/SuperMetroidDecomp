using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's arms (#2146), which his body's fight animates: they take no action of their own.</summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the arms occupy.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
/// <param name="part">What the enemy's setup built (#2554).</param>
internal sealed partial class DraygonArmsEntity(DraygonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, BossArtworkBinding bossArtwork, DraygonPart part)
    : DraygonEntity(species, occupant, place, common, bossArtwork)
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The Draygon species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the arms occupy.</param>
    /// <param name="common">The shared participation.</param>
    /// <param name="bossArtwork">The session's installed boss artwork.</param>
    /// <param name="draygon">The encounter his body began, which the arms joins as it is built.</param>
    internal DraygonArmsEntity(DraygonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
        BossArtworkBinding bossArtwork, DraygonEnemyState draygon)
        : this(species, occupant, place, common, bossArtwork, DraygonParts.InitializeDraygonArms(place, draygon))
    {
    }

    /// <summary>The arms' part of the encounter they joined as they set up.</summary>
    internal override DraygonPart Part { get; } = part;

    /// <summary>Takes no action of their own; his body's fight animates the arms.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
