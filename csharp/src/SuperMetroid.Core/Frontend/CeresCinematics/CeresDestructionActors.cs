using SuperMetroid.Core.Frontend.Intro;
using static SuperMetroid.Core.Frontend.CeresCinematics.CeresDestructionActorDefinitions;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Selects the Ceres destruction and Zebes reveal actors (<see cref="CeresDestructionActorDefinitions"/>) in native spawn order,
/// with their installed layout ids.
/// </summary>
internal static class CeresDestructionActors
{
    /// <summary>Stable presentation roles of the three persistent explosion-scene actors.</summary>
    /// <remarks>Named roles follow $C27C..C295 spawn order. #1165 verifies the original identity
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

    /// <summary>Selects large debris, small debris and stationary vortex in native spawn order.</summary>
    /// <remarks>#1165 independently confirms $C27C..C295 selecting CE7F/CE8B/CE91.
    /// This existing explicit actor dispatch is preserved: large debris aliases flight row0
    /// with its distinct definition/list, small debris aliases flight row2 unchanged, and
    /// vortex aliases flight row3 with the native parameter-zero initializer overrides.
    /// BFA5 sets X112 and BFAB installs the BFD9 no-op, so this vortex has no horizontal
    /// movement or wrap. The definition still names BFC6 before initialization. No slide
    /// policy applies to these actors. One original-ROM proof covers the complete mapping,
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
            0 => new(                flight.ActivePreInstruction, CinematicSpriteProgram.CeresDestructionLargeAsteroid, flight.Position, flight.Colouring,
                flight.HorizontalDelta, flight.WrapX, 0, null, false),
            1 => new(                flight.ActivePreInstruction, flight.Program, flight.Position,
                flight.Colouring, flight.HorizontalDelta, flight.WrapX, 0, null, false),
            // Parameter zero makes BFA0 replace BFC6 with the BFD9 no-op and use X=$70.
            2 => new(                CinematicSpritePreInstruction.CeresStationaryVortexNoOp, flight.Program, flight.Position.WithX(112), flight.Colouring,
                0, false, 0, null, false),
            _ => throw new InvalidOperationException("Validated Ceres actor index became invalid."),
        };
    }

    /// <summary>Returns planet, four grid-aligned star sheets and title in native spawn order.</summary>
    /// <remarks>
    /// #1165: $C810..C831 selects these six roles. Star quadrant q=index-1 is bounded to 0..3:
    /// X=48+160*(q mod 2), Y=47+160*(q/2). The native initializers $C942..C991 place
    /// all four sheets on this two-by-two grid with the same palette and acceleration.
    /// Their definitions are six bytes apart, initializers twenty bytes apart (including
    /// each leading NOP), and single-frame loop programs eight bytes apart. Only the
    /// lower-right sheet installs the completion callback. Planet and title are separate
    /// named roles with their own initializer operands and programs, not curve exceptions.
    /// One original-ROM metadata proof checks every field and invalid-index rejection.
    /// Instruction programs have their existing separate byte/interpreter proofs.
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
        CinematicSpritePreInstruction wait = completesScene ? CinematicSpritePreInstruction.ZebesStarsCompletionWait : CinematicSpritePreInstruction.ZebesStarsWait;
        return new(
wait,
            ZebesStarSheets[quadrant],
            new(WordWrap.Unsigned(48 + 160 * (quadrant & 1)), WordWrap.Unsigned(47 + 160 * (quadrant >> 1))),
            ScreenColourGroup.CeresFlightSpaceObjects, 0, false, 32,
            completesScene ? CinematicSpritePreInstruction.ZebesStarsCompletionSlideSceneAway : CinematicSpritePreInstruction.ZebesStarsSlideSceneAway,
            completesScene);
    }
}
