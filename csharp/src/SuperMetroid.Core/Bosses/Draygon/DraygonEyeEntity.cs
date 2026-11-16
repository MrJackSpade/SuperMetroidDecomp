using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's eye (#2146): inert until his body faces a way, then it tracks Samus.</summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="place">The place the eye occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class DraygonEyeEntity(DraygonBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : DraygonEntity(species, place, common, bossArtwork)
{
    /// <inheritdoc/>
    public override void Initialize() => Species.Parts.InitializeDraygonEye(Place);

    /// <summary>The encounter the eye joined as it set up.</summary>
    internal override DraygonEnemyState Encounter => Place.RequireSpeciesState<DraygonEyeState>().Draygon;

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.Parts.RunDraygonEyeMain(Place, Ensure.NotNull(frame).Samus);
}
