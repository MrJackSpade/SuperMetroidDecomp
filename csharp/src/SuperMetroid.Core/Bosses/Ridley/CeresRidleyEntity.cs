using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley (#2146): the Ceres encounter his body holds from its initialization (#2231), its flight, which resolves its
/// own shots, and his own reaction to a bomb that selects his shot reaction. Once he has got away his body neither meets
/// Samus nor draws his tail and wings.
/// </summary>
/// <param name="ceres">Ceres Ridley's encounter behaviour.</param>
/// <param name="escape">The station's escape progress, which ends his body's touch and drawing.</param>
/// <param name="warning">The room's foreground warning, which the self-destruct his getaway starts writes.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place Ridley occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed partial class CeresRidleyEntity(CeresRidleyBehavior ceres, ICeresEscapeStatus escape, RoomWarning warning, RoomEnemyOccupant occupant,
    RoomEnemyPlace place, EnemyCommonParticipation common)
    : RidleyEntity(ceres.Body, occupant, place, common), IEnemyShotAdmission
{
    /// <summary>The <c>warning</c> the object was made with, which it keeps so it can be made again from what it holds (#2602).</summary>
    private readonly RoomWarning warning = warning;

    /// <summary>The Ceres encounter his body holds from its initialization.</summary>
    internal override CeresRidleyState Encounter { get; } = ceres.Flight.InitializeCeresRidley(place, warning);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) =>
        ceres.RunCeresRidleyMain(Place, Encounter, frame.Samus, frame.SamusProjectiles);

    /// <summary>Ceres Ridley's lift-off starts his rise toward his hover.</summary>
    public override void LiftOff() => CeresRidleyFlight.LiftOff(Encounter);

    /// <summary>Ceres Ridley cuts his fireballing short when Samus is low on energy.</summary>
    /// <param name="samus">Samus, whose energy decides the cut.</param>
    public override RidleyBodySegment? CutFireballingShort(SamusState samus) =>
        CeresRidleyFlight.CutFireballingShort(Encounter, samus);

    /// <summary>His body meets Samus until he gets away.</summary>
    /// <param name="samus">Samus.</param>
    /// <param name="pseudoScrew">Samus's held charge, which her Pseudo Screw contact spends.</param>
    public override void ResolveAfterSamusContact(SamusState samus, PseudoScrewCharge pseudoScrew)
    {
        if (escape.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            base.ResolveAfterSamusContact(samus, pseudoScrew);
    }

    /// <summary>His tail and wings draw until he gets away; the getaway's turning floor shows him then.</summary>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="camera">The camera.</param>
    public override void DrawBeforeBody(SpriteFrameBuffer sprites, Position16<RoomPixels> camera)
    {
        if (escape.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            base.DrawBeforeBody(sprites, camera);
    }

    /// <summary>Runs Ceres Ridley's own reaction to a bomb that selects his shot reaction, before the ignoring check.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (context.SelectedShot != RidleyReactions.Shot)
            return base.TryReactToBomb(context);
        // Ceres Ridley takes no damage: an exploding bomb counts toward his hits like any shot, the bomb already marked
        // as having struck, while Ridley alternates his hurt timing and moves on toward the escape his last hit brings.
        CeresRidleyHits.ResolveCeresRidleyShotAfterCollision(Place, Encounter);
        return true;
    }

    /// <summary>Ceres Ridley never enters the ordinary shot pass; its encounter resolves its own shots.</summary>
    public bool AdmitsShots() => false;
}
