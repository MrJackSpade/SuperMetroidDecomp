using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's eye (#2146): inert until his body faces a way, then it tracks Samus.</summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the eye occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
/// <param name="draygon">The encounter his body began, which the part joins.</param>
internal sealed class DraygonEyeEntity(DraygonBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork,
    DraygonEnemyState draygon)
    : DraygonEntity(species, occupant, place, common, bossArtwork)
{
    /// <summary>The eye's part of the encounter, which it joins as it is built (#2554).</summary>
    internal override DraygonPart Part { get; } = DraygonParts.InitializeDraygonEye(place, draygon);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Parts.RunDraygonEyeMain(Encounter, Ensure.NotNull(frame).Samus);
}
