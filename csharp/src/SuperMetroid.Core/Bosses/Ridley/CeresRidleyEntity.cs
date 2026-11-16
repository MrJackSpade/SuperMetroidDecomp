using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley (#2146): the Ceres encounter's flight, which resolves its own shots, and his own reaction to a bomb that
/// selects his shot reaction.
/// </summary>
/// <param name="ridley">The Lower Norfair Ridley owner.</param>
/// <param name="ceres">The Ceres Ridley owner.</param>
/// <param name="encounter">The shared Ridley encounter.</param>
/// <param name="place">The place Ridley occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CeresRidleyEntity(
    RidleyBehavior ridley, CeresRidleyBehavior ceres, RidleyEncounter encounter, RoomEnemyPlace place, EnemyCommonParticipation common)
    : RidleyEntity(ridley, ceres, encounter, place, common), IEnemyShotAdmission
{
    /// <inheritdoc/>
    public override void Initialize() => Ceres.Flight.InitializeCeresRidley(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Ceres.RunCeresRidleyMainAi(Place, frame);

    /// <summary>Ceres Ridley's lift-off starts his rise toward his hover.</summary>
    public override void LiftOff() => CeresRidleyFlight.LiftOff(Encounter.RequireCeresRidley(Place));

    /// <summary>Ceres Ridley cuts his fireballing short when Samus is low on energy.</summary>
    /// <param name="samus">Samus, whose energy decides the cut.</param>
    public override RidleyBodySegment? CutFireballingShort(SamusState samus) =>
        CeresRidleyFlight.CutFireballingShort(Encounter.RequireCeresRidley(Place), samus);

    /// <summary>Runs Ceres Ridley's own reaction to a bomb that selects his shot reaction, before the ignoring check.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (context.SelectedShot != RidleyReactions.Shot)
            return base.TryReactToBomb(context);
        Ceres.Hits.ReactToCeresRidleyBomb(context);
        return true;
    }

    /// <summary>Ceres Ridley never enters the ordinary shot pass; its encounter resolves its own shots.</summary>
    public bool AdmitsShots() => false;
}
