using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Fixed actor metadata and physical motion for Ceres destruction and Zebes reveal.</summary>
internal static class CeresDestructionActorDefinitions
{
    /// <summary>Bank containing the native cinematic-object definitions.</summary>
    public const int NativeBank = 0x8b0000;

    /// <summary>Number of persistent scene actors allocated by <c>$8B:C27C-$C295</c>.</summary>
    public const int InitialActorCount = 3;

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

    /// <summary>Number of reveal actors allocated by <c>$8B:C810-$C831</c>.</summary>
    public const int ZebesActorCount = 6;

    /// <summary>Native reveal actor placement operands in bank $8B, in spawn order.</summary>
    /// <remarks>Role cases follow $C810..C831 spawn order and identify X/Y immediates in each
    /// initializer. #1165 verifies all six original entries and ROM operands. Invalid indices
    /// throw ArgumentOutOfRangeException.</remarks>
    public static CinematicPlacementSourceDefinition ZebesPlacementSource(int index) => index switch
    {
        0 => new("planet", 0xc83c, 0xc842),
        1 => new("stars-upper-left", 0xc944, 0xc94a),
        2 => new("stars-upper-right", 0xc958, 0xc95e),
        3 => new("stars-lower-left", 0xc96c, 0xc972),
        4 => new("stars-lower-right", 0xc980, 0xc986),
        5 => new("planet-zebes-title", 0xc993, 0xc999),
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
            0 => CeresFlightActorDefinitions.RearViewActor(0),
            1 => CeresFlightActorDefinitions.RearViewActor(2),
            2 => CeresFlightActorDefinitions.RearViewActor(3),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        return index switch
        {
            0 => new(                flight.ActivePreInstruction, CinematicSpriteProgram.CeresDestructionLargeAsteroid, flight.Position, flight.Attributes,
                flight.HorizontalDelta, flight.WrapX, 0, null, false),
            1 => new(                flight.ActivePreInstruction, flight.Program, flight.Position,
                flight.Attributes, flight.HorizontalDelta, flight.WrapX, 0, null, false),
            // Parameter zero makes BFA0 replace BFC6 with the BFD9 no-op and use X=$70.
            2 => new(                CinematicSpritePreInstruction.CeresStationaryVortexNoOp, flight.Program, flight.Position.WithX(112), flight.Attributes,
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

    /// <summary>$8B:CEA3, CinematicSpriteObjectDefinitions_Zebes: initializer C83B,
    /// waiting callback C84E, list CCAB; operands C83C/C842/C848 set its placement/palette.
    /// C857 selects slide callback C85D, whose C862 operand accelerates by 64/256 pixels.</summary>
    private static CeresDestructionActorDefinition ZebesPlanet =>
        new(CinematicSpritePreInstruction.Zebes, CinematicSpriteProgram.ZebesPlanet,
            new(136, 111), 0x0e00, 0, false, 64, CinematicSpritePreInstruction.ZebesSlideSceneAway, false);

    /// <summary>$8B:CEAF, CinematicSpriteObjectDefinitions_PlanetZebesText: initializer C992,
    /// no-op 93D9, list CCBB. Operands C993/C999/C99F center the title at (128,186), palette zero.
    /// Its program deletes before scene sliding, so it has no slide callback or acceleration.</summary>
    private static CeresDestructionActorDefinition ZebesTitle =>
        new(CinematicSpritePreInstruction.NoOp, CinematicSpriteProgram.ZebesTitle,
            new(128, 186), 0, 0, false, 0, null, false);

    /// <summary>The four star sheets' programs, indexed by row-major quadrant.</summary>
    private static readonly CinematicSpriteProgram[] ZebesStarSheets =
    [
        CinematicSpriteProgram.ZebesStarSheet0, CinematicSpriteProgram.ZebesStarSheet1,
        CinematicSpriteProgram.ZebesStarSheet2, CinematicSpriteProgram.ZebesStarSheet3,
    ];

    /// <summary>Constructs a star-sheet definition for an already validated row-major quadrant 0..3.</summary>
    private static CeresDestructionActorDefinition ZebesStarSheet(int quadrant)
    {
        bool completesScene = quadrant == 3;
        CinematicSpritePreInstruction wait = completesScene ? CinematicSpritePreInstruction.ZebesStarsCompletionWait : CinematicSpritePreInstruction.ZebesStarsWait;
        return new(
wait,
            ZebesStarSheets[quadrant],
            new((ushort)(48 + 160 * (quadrant & 1)), (ushort)(47 + 160 * (quadrant >> 1))),
            0x0800, 0, false, 32,
            completesScene ? CinematicSpritePreInstruction.ZebesStarsCompletionSlideSceneAway : CinematicSpritePreInstruction.ZebesStarsSlideSceneAway,
            completesScene);
    }
}
