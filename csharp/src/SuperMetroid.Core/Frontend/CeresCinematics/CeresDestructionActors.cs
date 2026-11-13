using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using static SuperMetroid.Core.Frontend.CeresCinematics.CeresDestructionActorDefinitions;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Selects the Ceres destruction and Zebes reveal actors (<see cref="CeresDestructionActorDefinitions"/>) in spawn order,
/// with their installed layout ids.
/// </summary>
internal static class CeresDestructionActors
{
    /// <summary>Stable presentation roles of the three persistent explosion-scene actors.</summary>
    /// <remarks>Named roles follow spawn order. #1165 verifies the authored identity
    /// mapping; invalid indices throw ArgumentOutOfRangeException.</remarks>
    public static string InitialPlacementId(int index) => index switch
    {
        0 => "large-asteroid",
        1 => "small-asteroid",
        2 => "vortex",
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>The installed layout id of each of the six reveal actors, in spawn order.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the six actors.</exception>
    public static string ZebesPlacementId(int index) => index switch
    {
        0 => "planet",
        1 => "stars-upper-left",
        2 => "stars-upper-right",
        3 => "stars-lower-left",
        4 => "stars-lower-right",
        5 => "planet-zebes-title",
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>Selects large debris, small debris and stationary vortex in spawn order.</summary>
    /// <remarks>#1165 independently confirms the spawn order and the definitions it selects.
    /// This existing explicit actor dispatch is preserved: large debris aliases flight row0
    /// with its distinct definition/list, small debris aliases flight row2 unchanged, and
    /// vortex aliases flight row3 with the parameter-zero initializer overrides.
    /// Its initializer installs an empty callback, so this vortex has no horizontal
    /// movement or wrap. The definition still names the moving callback before initialization. No slide
    /// policy applies to these actors. One proof against the authored data covers the complete mapping,
    /// initializer operands, shared aliases and invalid-index rejection. The prior authored-
    /// identity retention claim is superseded by verified semantic case selection; no table
    /// or generated cache remains. Animation programs have their separate existing proofs.</remarks>
    public static CeresDestructionActorDefinition InitialActor(int index)
    {
        CeresFlightActorDefinition flight = index switch
        {
            0 => CeresFlightActors.RearViewActor(0),
            1 => CeresFlightActors.RearViewActor(2),
            2 => CeresFlightActors.RearViewActor(3),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        return index switch
        {
            0 => new(                flight.ActiveFrameBehavior, CinematicSpriteProgram.CeresDestructionLargeAsteroid, flight.Colouring,
                flight.HorizontalDelta, flight.WrapX, SignedEightEight.Zero, null, false),
            1 => new(                flight.ActiveFrameBehavior, flight.Program,
                flight.Colouring, flight.HorizontalDelta, flight.WrapX, SignedEightEight.Zero, null, false),
            // Parameter zero makes the vortex's setup replace its moving callback with the no-op.
            2 => new(                CinematicSpriteFrameBehavior.CeresStationaryVortexNoOp, flight.Program, flight.Colouring,
                SignedSixteenSixteen.Zero, false, SignedEightEight.Zero, null, false),
            _ => throw new InvalidOperationException("Validated Ceres actor index became invalid."),
        };
    }

    /// <summary>Returns planet, four grid-aligned star sheets and title in spawn order.</summary>
    /// <remarks>
    /// #1165: the reveal's spawns select these six roles. Star quadrant q=index-1 is bounded to 0..3. The star
    /// initializers give all four sheets the same palette and acceleration; where each starts is installed (#1615).
    /// Their definitions, initializers and single-frame loop programs follow one another at fixed spacing. Only the
    /// lower-right sheet installs the completion callback. Planet and title are separate
    /// named roles with their own initializer operands and programs, not curve exceptions.
    /// One metadata proof against the authored data checks every field and invalid-index rejection.
    /// Animation programs have their existing separate proofs.
    /// </remarks>
    public static CeresDestructionActorDefinition ZebesActor(int index) => index switch
    {
        0 => ZebesPlanet,
        >= 1 and <= 4 => ZebesStarSheet(index - 1),
        5 => ZebesTitle,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>Constructs a star-sheet definition for an already validated row-major quadrant 0..3.</summary>
    private static CeresDestructionActorDefinition ZebesStarSheet(int quadrant)
    {
        bool completesScene = quadrant == 3;
        CinematicSpriteFrameBehavior wait = completesScene ? CinematicSpriteFrameBehavior.ZebesStarsCompletionWait : CinematicSpriteFrameBehavior.ZebesStarsWait;
        return new(
wait,
            ZebesStarSheets[quadrant],
            CeresSpriteColouring.SpaceObjects, SignedSixteenSixteen.Zero, false, SignedEightEight.FromParts(0, 32),
            completesScene ? CinematicSpriteFrameBehavior.ZebesStarsCompletionSlideSceneAway : CinematicSpriteFrameBehavior.ZebesStarsSlideSceneAway,
            completesScene);
    }
}
