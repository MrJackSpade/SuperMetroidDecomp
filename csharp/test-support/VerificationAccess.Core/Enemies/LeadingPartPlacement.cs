using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.Powamp;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Gives a check's place the placement settings a room would give the leading part of <c>definition</c> before it binds,
/// so a definition whose species builds a part's own entity from its placement (#2146, #2163, #2164, #2175, #2283, #2342)
/// binds as that part.
/// </summary>
internal static class LeadingPartPlacement
{
    /// <summary>Records on <paramref name="place"/> the settings a room gives <paramref name="definition"/>'s leading part, if it has any.</summary>
    /// <param name="place">The place about to bind <paramref name="definition"/>.</param>
    /// <param name="definition">The definition about to bind.</param>
    internal static void Prepare(RoomEnemyPlace place, EnemyDefinitionId definition)
    {
        Ensure.NotNull(place);
        // An ordinary Ceres door faces the way its placement says, and a getaway chamber wall stands on its side (#1989).
        if (definition == EnemyDefinitionId.CeresDoor)
            place.BindPlaced(definition, new CeresDoorSpawnSettings(HorizontalDirection.Right));
        if (definition == EnemyDefinitionId.CeresGetawayChamberWall)
            place.BindPlaced(definition, new CeresGetawayChamberWallSpawnSettings(HorizontalDirection.Left));
        // An Evir placement names whether it is the body or the arms (#2146); the body owns the Evir's members.
        if (definition == EnemyDefinitionId.Evir)
            place.BindPlaced(definition, new EvirSpawnSettings(EvirBodyPart.Body, SignedSixteenSixteen.FromSixteenthsOfPixel(0), 0));
        // A Powamp placement names whether it is the body or the balloon (#2163); the body owns the Powamp's members.
        if (definition == EnemyDefinitionId.Powamp)
            place.BindPlaced(definition, new PowampBodySpawnSettings(PowampContact.Accepts));
        // A Morph Ball eye placement names the eye or its beam mount, and a Rio's or a lava jumper's whether it leads or
        // follows (#2164); the eye and the leading part carry the species' members.
        if (definition == EnemyDefinitionId.MorphBallEye)
            place.BindPlaced(definition, new MorphBallEyeBodySpawnSettings(HorizontalDirection.Right));
        if (definition is EnemyDefinitionId.Geruta or EnemyDefinitionId.Holtz or EnemyDefinitionId.Squeept)
            place.BindPlaced(definition, new EnemyFollowerSpawnSettings(EnemyFollowerRole.Leader));
        // A Dragon placement names whether it is the body or the wing (#2175); the body carries the species' members.
        if (definition == EnemyDefinitionId.Dragon)
            place.BindPlaced(definition, new DragonSpawnSettings(DragonPart.Body));
        // A Dachora placement names the runner or one of its speed-boost echoes (#2283); the runner carries the species' members.
        if (definition == EnemyDefinitionId.Dachora)
            place.BindPlaced(definition, new DachoraSpawnSettings(HorizontalDirection.Right, DachoraRole.Runner));
        // A Hibashi placement names the flame or its collision part (#2342); the flame carries the species' members.
        if (definition == EnemyDefinitionId.Hibashi)
            place.BindPlaced(definition, new HibashiFlameSpawnSettings(InactiveTime: 0));
        // A Magdollite placement names its head, rising body or tracking overlay; the head carries the species' members.
        if (definition == EnemyDefinitionId.Magdollite)
            place.BindPlaced(definition, new MagdolliteHeadSpawnSettings(SignedSixteenSixteen.Zero, Range: 0));
    }
}
