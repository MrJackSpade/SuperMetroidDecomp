namespace SuperMetroid.Core.Frontend;

/// <summary>Fixed actor metadata and motion parameters for Samus's flight toward Ceres.</summary>
internal static class CeresFlightActorDefinitions
{
    /// <summary>Bank containing the native actor definitions and initializer immediates.</summary>
    public const int NativeBank = 0x8b0000;

    /// <summary>Number of actors allocated by <c>$8B:BE3B-$8B:BE5C</c>.</summary>
    public const int RearViewActorCount = 5;

    /// <summary><c>$8B:BEC2</c>, signed-8.8 acceleration applied to the opening star field.</summary>
    public const ushort FrontStarAcceleration = 0x0080;

    /// <summary>
    /// <c>$8B:CF0F</c> with initializer parameter zero: opening star field and its
    /// signed-8.8 motion accumulator.
    /// </summary>
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

    /// <summary>
    /// Returns the five actors spawned in native order by <c>$8B:BE3B-$8B:BE5C</c>.
    /// Their lists remain ROM-backed; constructor metadata and physical motion do not.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #1005: row zero selects the large-asteroid list at
    /// $8B:CE4B..CE52, reused by the Ceres destruction scene. All four
    /// words match pinned NTSC J/U v1.0 ROM and bank_8B.asm: duration $000A,
    /// bank-$8C spritemap $94F7, goto opcode $94BC, target $CE4B. The exact
    /// bounded rule is to display that same authored asteroid spritemap for
    /// ten handler calls, then repeat indefinitely while the owning scene
    /// keeps the actor alive. IntroDiscoverySprite.Step follows the goto;
    /// it never falls into the adjacent delete list at $CE53. The frame art
    /// remains ROM-backed, while this constant-period loop needs no table.
    /// </remarks>
    public static CeresFlightActorDefinition RearViewActor(int index) => index switch
    {
        0 => new(0xcf39, 0xbf22, 0xbf35, 0xbf35, 0xce4b,
            0x0050, 0x009f, 0x0800, 0, 0x0000_4000, true),
        1 => new(0xce85, 0xbf4c, 0xbf5f, 0xbf5f, 0xcc47,
            0x0074, 0x00a0, 0x0c00, 0, 0x0000_1000, true),
        2 => new(0xce8b, 0xbf76, 0xbf89, 0xbf89, 0xcc4f,
            0x0080, 0x0060, 0x0800, 0, 0x0000_0800, true),
        3 => new(0xce91, 0xbfa0, 0xbfc6, 0xbfc6, 0xcc57,
            0x00e0, 0x0057, 0x0800, 0, unchecked((int)0xffff_e000), false),
        // Parameter one makes the CF0F initializer replace BEB5 with the vortex callback.
        4 => new(0xcf0f, 0xbe7e, 0xbeb5, 0xbfc6, 0xcda3,
            0xffe0, 0x0057, 0x0800, 0, unchecked((int)0xffff_e000), false),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
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
