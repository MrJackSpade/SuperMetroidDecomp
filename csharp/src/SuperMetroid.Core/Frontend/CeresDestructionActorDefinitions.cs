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
            0 => new(                flight.ActivePreInstruction, CeresDestructionSpriteInstructionDefinitions.LargeAsteroidStart, flight.X, flight.Y, flight.Attributes,
                flight.HorizontalDelta, flight.WrapX, 0, 0, false),
            1 => new(                flight.ActivePreInstruction, flight.InstructionList, flight.X, flight.Y,
                flight.Attributes, flight.HorizontalDelta, flight.WrapX, 0, 0, false),
            // Parameter zero makes BFA0 replace BFC6 with the BFD9 no-op and use X=$70.
            2 => new(                StationaryVortexCallback, flight.InstructionList, 112, flight.Y, flight.Attributes,
                0, false, 0, 0, false),
            _ => throw new InvalidOperationException("Validated Ceres actor index became invalid."),
        };
    }

    /// <summary>$8B:CE7F, CinematicSpriteObjectDefs_CeresUnderAttackLargeAsteroids,
    /// sharing BF22/BF35 with CF39 but selecting CC3F instead of CE4B.</summary>
    private const ushort DestructionLargeAsteroidDefinition = 0xce7f;
    /// <summary>$8B:BFD9, RTS_8BBFD9 installed by the vortex parameter-zero initializer at BFAB.</summary>
    private const ushort StationaryVortexCallback = 0xbfd9;

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
        new(0xc84e, CeresDestructionSpriteInstructionDefinitions.PlanetStart,
            136, 111, 0x0e00, 0, false, 64, 0xc85d, false);

    /// <summary>$8B:CEAF, CinematicSpriteObjectDefinitions_PlanetZebesText: initializer C992,
    /// no-op 93D9, list CCBB. Operands C993/C999/C99F center the title at (128,186), palette zero.
    /// Its program deletes before scene sliding, so it has no slide callback or acceleration.</summary>
    private static CeresDestructionActorDefinition ZebesTitle =>
        new(0x93d9, CeresDestructionSpriteInstructionDefinitions.TitleStart,
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
wait,
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
