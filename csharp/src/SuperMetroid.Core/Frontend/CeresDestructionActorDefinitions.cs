namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed actor metadata and physical motion for Ceres destruction and Zebes reveal.</summary>
internal static class CeresDestructionActorDefinitions
{
    /// <summary>Bank containing the native cinematic-object definitions.</summary>
    public const int NativeBank = 0x8b0000;

    /// <summary>Number of persistent scene actors allocated by <c>$8B:C27C-$C295</c>.</summary>
    public const int InitialActorCount = 3;

    /// <summary>Number of reveal actors allocated by <c>$8B:C810-$C831</c>.</summary>
    public const int ZebesActorCount = 6;

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

    /// <summary>Returns the planet, four star sheets, and PLANET ZEBES title in spawn order.</summary>
    public static CeresDestructionActorDefinition ZebesActor(int index) => index switch
    {
        0 => new(0xcea3, 0xc83b, 0xc84e, 0xc84e, 0xccab,
            0x0088, 0x006f, 0x0e00, 0, false, 0x0040, 0xc85d, false),
        1 => new(0xcef7, 0xc942, 0xc8f9, 0xc8f9, 0xcd83,
            0x0030, 0x002f, 0x0800, 0, false, 0x0020, 0xc908, false),
        2 => new(0xcefd, 0xc956, 0xc8f9, 0xc8f9, 0xcd8b,
            0x00d0, 0x002f, 0x0800, 0, false, 0x0020, 0xc908, false),
        3 => new(0xcf03, 0xc96a, 0xc8f9, 0xc8f9, 0xcd93,
            0x0030, 0x00cf, 0x0800, 0, false, 0x0020, 0xc908, false),
        4 => new(0xcf09, 0xc97e, 0xc8aa, 0xc8aa, 0xcd9b,
            0x00d0, 0x00cf, 0x0800, 0, false, 0x0020, 0xc8b9, true),
        5 => new(0xceaf, 0xc992, 0x93d9, 0x93d9, 0xccbb,
            0x0080, 0x00ba, 0x0000, 0, false, 0, 0, false),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
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
