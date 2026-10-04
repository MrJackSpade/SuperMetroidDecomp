namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed actor metadata and physical motion for Ceres destruction and Zebes reveal.</summary>
internal static class CeresDestructionActorDefinitions
{
    /// <summary>Bank containing the native cinematic-object definitions.</summary>
    public const int NativeBank = 0x8b0000;

    /// <summary>Number of persistent scene actors allocated by <c>$8B:C27C-$C295</c>.</summary>
    public const int InitialActorCount = 3;

    /// <summary>Stable presentation roles of the three persistent explosion-scene actors.</summary>
    /// <remarks>Named roles follow $C27C..C295 spawn order. #1165 verifies the original identity
    /// mapping; invalid indices preserve the former array's IndexOutOfRangeException.</remarks>
    public static string InitialPlacementId(int index) => index switch
    {
        0 => "large-asteroid",
        1 => "small-asteroid",
        2 => "vortex",
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>Number of reveal actors allocated by <c>$8B:C810-$C831</c>.</summary>
    public const int ZebesActorCount = 6;

    /// <summary>Native reveal actor placement operands in bank $8B, in spawn order.</summary>
    /// <remarks>Role cases follow $C810..C831 spawn order and identify X/Y immediates in each
    /// initializer. #1165 verifies all six original entries and ROM operands. Invalid indices
    /// preserve the former array's IndexOutOfRangeException contract.</remarks>
    public static (string Id, ushort XAddress, ushort YAddress) ZebesPlacementSource(int index) => index switch
    {
        0 => ("planet", 0xc83c, 0xc842),
        1 => ("stars-upper-left", 0xc944, 0xc94a),
        2 => ("stars-upper-right", 0xc958, 0xc95e),
        3 => ("stars-lower-left", 0xc96c, 0xc972),
        4 => ("stars-lower-right", 0xc980, 0xc986),
        5 => ("planet-zebes-title", 0xc993, 0xc999),
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>Returns the three persistent actors behind the station explosion.</summary>
    /// <remarks>
    /// Issues #625 and #1015: row zero's distinct large-asteroid list at
    /// $8B:CC3F..CC46 has words $000A, $909D, $94BC, $CC3F in pinned
    /// NTSC J/U v1.0 ROM and bank_8B.asm. It reuses the flight asteroid's
    /// native initializer and motion callback, but displays bank-$8C
    /// under-attack spritemap $909D for ten handler calls before the goto
    /// repeats from $CC3F. IntroDiscoverySprite.Step confines the cursor
    /// to this single-frame loop; $CC47 starts another actor's list.
    /// The constant-period rule needs no table, while the authored visual
    /// frame remains ROM-backed.
    ///
    /// Issues #625 and #1016: $8B:C27C..C295 spawns definition pointers
    /// $CE7F, $CE8B, $CE91 in that order. Their three-word definitions
    /// match pinned NTSC J/U v1.0 ROM and bank_8B.asm: (BF22,BF35,CC3F),
    /// (BF76,BF89,CC4F), (BFA0,BFC6,CC57). For bounded index i=0..2,
    /// the reused flight row is i==0 ? 0 : i+1. Row zero selects its
    /// distinct under-attack frame list; row one keeps the flight small
    /// asteroids. Row two passes initializer parameter zero, which sets
    /// X=$0070 and replaces active callback $BFC6 with no-op $BFD9;
    /// therefore its horizontal delta is zero. The pointer mapping is
    /// exact, but retain the three authored actor identities and overrides.
    /// </remarks>
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
            0 => new(0xce7f, flight.Initialization, flight.DefinitionPreInstruction,
                flight.ActivePreInstruction, 0xcc3f, flight.X, flight.Y, flight.Attributes,
                flight.HorizontalDelta, flight.WrapX, 0, 0, false),
            1 => new(flight.Pointer, flight.Initialization, flight.DefinitionPreInstruction,
                flight.ActivePreInstruction, flight.InstructionList, flight.X, flight.Y,
                flight.Attributes, flight.HorizontalDelta, flight.WrapX, 0, 0, false),
            // Parameter zero makes BFA0 replace BFC6 with the BFD9 no-op and use X=$70.
            2 => new(flight.Pointer, flight.Initialization, flight.DefinitionPreInstruction,
                0xbfd9, flight.InstructionList, 0x0070, flight.Y, flight.Attributes,
                0, false, 0, 0, false),
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
        new(0xcea3, 0xc83b, 0xc84e, 0xc84e, CeresDestructionSpriteInstructionDefinitions.PlanetStart,
            136, 111, 0x0e00, 0, false, 64, 0xc85d, false);

    /// <summary>$8B:CEAF, CinematicSpriteObjectDefinitions_PlanetZebesText: initializer C992,
    /// no-op 93D9, list CCBB. Operands C993/C999/C99F center the title at (128,186), palette zero.
    /// Its program deletes before scene sliding, so it has no slide callback or acceleration.</summary>
    private static CeresDestructionActorDefinition ZebesTitle =>
        new(0xceaf, 0xc992, 0x93d9, 0x93d9, CeresDestructionSpriteInstructionDefinitions.TitleStart,
            128, 186, 0, 0, false, 0, 0, false);

    /// <summary>$8B:CEF7, first six-byte definition, CinematicSpriteObjectDefinitions_ZebesStars2.</summary>
    private const ushort FirstZebesStarDefinition = 0xcef7;
    /// <summary>$8B:C942, first twenty-byte initializer, InitFunction_CinematicSpriteObject_ZebesStars2.</summary>
    private const ushort FirstZebesStarInitializer = 0xc942;
    /// <summary>$8B:C8F9, PreInstruction_CinematicSpriteObject_ZebesStars_2_3_4 waits for scene sliding.</summary>
    private const ushort StarWaitCallback = 0xc8f9;
    /// <summary>$8B:C8AA, PreInstruction_CinematicSpriteObject_ZebesStars5 waits for completion-owner sliding.</summary>
    private const ushort CompletionStarWaitCallback = 0xc8aa;
    /// <summary>$8B:C908, PreInstruction_ZebesStars_2_3_4_SlideSceneAway moves and deletes ordinary sheets.</summary>
    private const ushort StarSlideCallback = 0xc908;
    /// <summary>$8B:C8B9, PreInstruction_ZebesStars5_SlideSceneAway additionally installs game load at C8F2.</summary>
    private const ushort CompletionStarSlideCallback = 0xc8b9;

    /// <summary>Constructs a star-sheet definition for an already validated row-major quadrant 0..3.</summary>
    private static CeresDestructionActorDefinition ZebesStarSheet(int quadrant)
    {
        bool completesScene = quadrant == 3;
        ushort wait = completesScene ? CompletionStarWaitCallback : StarWaitCallback;
        return new(
            (ushort)(FirstZebesStarDefinition + 6 * quadrant),
            (ushort)(FirstZebesStarInitializer + 20 * quadrant),
            wait, wait,
            (ushort)(CeresDestructionSpriteInstructionDefinitions.StarSheetsStart + 8 * quadrant),
            (ushort)(48 + 160 * (quadrant & 1)),
            (ushort)(47 + 160 * (quadrant >> 1)),
            0x0800, 0, false, 32,
            completesScene ? CompletionStarSlideCallback : StarSlideCallback,
            completesScene);
    }
}

/// <summary>One cinematic-object definition, initializer result, and translated motion policy.</summary>
internal readonly record struct CeresDestructionActorDefinition(
    ushort Pointer,
    ushort Initialization,
    ushort DefinitionPreInstruction,
    ushort ActivePreInstruction,
    ushort InstructionList,
    ushort X,
    ushort Y,
    ushort Attributes,
    int HorizontalDelta,
    bool WrapX,
    ushort SlideAcceleration,
    ushort SlidePreInstruction,
    bool CompletesScene);
