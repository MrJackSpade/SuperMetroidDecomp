namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed actor metadata and motion parameters for Samus's flight toward Ceres.</summary>
internal static class CeresFlightActorDefinitions
{
    /// <summary>Bank containing the native actor definitions and initializer immediates.</summary>
    public const int NativeBank = 0x8b0000;

    /// <summary>Number of actors allocated by <c>$8B:BE3B-$8B:BE5C</c>.</summary>
    public const int RearViewActorCount = 5;

    /// <summary>
    /// Native bank-$8B X/Y initializer operands for the five rear-view actors.
    /// The final X word is signed -32 represented in its native 16-bit form.
    /// </summary>
    /// <remarks>Explicit role selection in native spawn order $BE3B..BE5C. Addresses identify
    /// X/Y operands in each role's initializer, including the nonzero-parameter vortex/star branches.
    /// #1165 independently verifies the five original entries and ROM operands; unsupported indices
    /// preserve the former array's IndexOutOfRangeException contract.</remarks>
    public static (string Id, ushort XAddress, ushort YAddress) RearViewPlacementSource(int index) => index switch
    {
        0 => ("large-asteroid", 0xbf23, 0xbf29),
        1 => ("station-under-attack", 0xbf4d, 0xbf53),
        2 => ("small-asteroid", 0xbf77, 0xbf7d),
        3 => ("vortex", 0xbfb4, 0xbfba),
        4 => ("rear-stars", 0xbea3, 0xbea9),
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary><c>$8B:BEC2</c>, signed-8.8 acceleration applied to the opening star field.</summary>
    public const ushort FrontStarAcceleration = 0x0080;

    /// <summary>
    /// <c>$8B:CF0F</c> with initializer parameter zero: opening star field and its
    /// signed-8.8 motion accumulator.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #1014: front stars and RearViewActor(4) alias the
    /// same native $8B:CF0F definition and list $8B:CDA3..CDAA. Its four
    /// words match pinned NTSC J/U v1.0 ROM and bank_8B.asm: duration
    /// $000A, bank-$8C star spritemap $9478, goto $94BC, target $CDA3.
    /// The initializer parameter changes actor motion, not this list.
    /// The exact bounded rule displays the same authored star sheet for
    /// ten handler calls and repeats while its scene owns the actor.
    /// IntroDiscoverySprite.Step never falls into adjacent list $CDAB.
    /// Both aliases select the same installed visual frame.
    /// </remarks>
    public static CeresFlightActorDefinition FrontStars => new(
        Pointer: 0xcf0f,
        Initialization: 0xbe7e,
        DefinitionPreInstruction: 0xbeb5,
        ActivePreInstruction: 0xbeb5,
        InstructionList: 0xcda3,
        X: 0x0070,
        Y: 0x0057,
        Attributes: 0x0800,
        InitialTimer: 0xfc00,
        HorizontalDelta: 0,
        WrapX: false);

    /// <summary>Selects the five named actors spawned by $8B:BE3B..BE5C.</summary>
    /// <remarks>#1165 independently verifies each native spawn identity, definition triple,
    /// initializer branch and motion callback. This is actor-type dispatch: large debris,
    /// station, small debris, vortex and rear stars select their own native behavior and
    /// placement. The selected initializer parameters are not samples of a numerical curve.
    /// Named cases replace the earlier blanket retention claims; no output table or cache
    /// is retained. Inputs outside 0..4 still throw. The instruction programs have separate
    /// existing original-byte proofs shared across both Ceres scenes.</remarks>
    public static CeresFlightActorDefinition RearViewActor(int index) => index switch
    {
        0 => LargeAsteroids,
        1 => StationUnderAttack,
        2 => SmallAsteroids,
        3 => MovingVortex,
        4 => RearStars,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    /// <summary>$8B:CF39, CinematicSpriteObjectDefs_CeresExplosionLargeAsteroids.
    /// BF22 initializes (80,159), palette0800; BF35 adds 1/4 pixel and masks X to nine bits;
    /// CE4B selects the large-asteroid loop. The destruction scene substitutes CE7F/CC3F.</summary>
    private static CeresFlightActorDefinition LargeAsteroids =>
        new(0xcf39, 0xbf22, 0xbf35, 0xbf35, 0xce4b,
            80, 159, 0x0800, 0, 0x4000, true);

    /// <summary>$8B:CE85, CinematicSpriteObjectDefinitions_CeresUnderAttack.
    /// BF4C initializes (116,160), palette0C00; BF5F adds 1/16 pixel with nine-bit X wrap;
    /// CC47 selects the station-under-attack loop.</summary>
    private static CeresFlightActorDefinition StationUnderAttack =>
        new(0xce85, 0xbf4c, 0xbf5f, 0xbf5f, 0xcc47,
            116, 160, 0x0c00, 0, 0x1000, true);

    /// <summary>$8B:CE8B, CinematicSpriteObjectDefinitions_CeresSmallAsteroids.
    /// BF76 initializes (128,96), palette0800; BF89 adds 1/32 pixel with nine-bit X wrap;
    /// CC4F selects the small-asteroid loop. Both Ceres scenes share this identity.</summary>
    private static CeresFlightActorDefinition SmallAsteroids =>
        new(0xce8b, 0xbf76, 0xbf89, 0xbf89, 0xcc4f,
            128, 96, 0x0800, 0, 0x0800, true);

    /// <summary>$8B:CE91, CinematicSpriteObjectDefinitions_CeresPurpleSpaceVortex, parameter1.
    /// BFA0 takes the nonzero branch to (224,87), palette0800, preserving BFC6 movement;
    /// CC57 alternates two frames. Parameter0 is the stationary destruction-scene variant.</summary>
    private static CeresFlightActorDefinition MovingVortex =>
        new(0xce91, 0xbfa0, VortexMotionCallback, VortexMotionCallback, 0xcc57,
            224, 87, 0x0800, 0, VortexHorizontalDelta, false);

    /// <summary>$8B:CF0F, CinematicSpriteObjectDefinitions_CeresStars, parameter1.
    /// BE7E takes the nonzero branch: X=-32, Y87, palette0800, timer remains cleared;
    /// BE9C replaces BEB5 with BFC6. The definition and CDA3 program alias FrontStars.</summary>
    private static CeresFlightActorDefinition RearStars => FrontStars with
    {
        ActivePreInstruction = VortexMotionCallback,
        X = unchecked((ushort)-32),
        InitialTimer = 0,
        HorizontalDelta = VortexHorizontalDelta,
    };

    /// <summary>$8B:BFC6, PreInstruction_CinematicSpriteObject_CeresPurpleSpaceVortex;
    /// also installed by the rear-star initializer at BE9C.</summary>
    private const ushort VortexMotionCallback = 0xbfc6;
    /// <summary>$8B:BFCB, subtraction immediate2000: signed16.16 movement of -1/8 pixel,
    /// with borrow into the whole X word and no nine-bit mask.</summary>
    private const int VortexHorizontalDelta = -0x2000;
}

/// <summary>One Ceres cinematic actor definition plus its fixed initializer result.</summary>
internal readonly record struct CeresFlightActorDefinition(
    ushort Pointer,
    ushort Initialization,
    ushort DefinitionPreInstruction,
    ushort ActivePreInstruction,
    ushort InstructionList,
    ushort X,
    ushort Y,
    ushort Attributes,
    ushort InitialTimer,
    int HorizontalDelta,
    bool WrapX);
