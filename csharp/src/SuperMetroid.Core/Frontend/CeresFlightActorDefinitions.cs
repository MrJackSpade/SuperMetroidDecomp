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
    ///
    /// Issues #625 and #1006: native spawn order at $8B:BE3B..BE5C
    /// selects the five initial X words at $8B:BF23/$BF4D/$BF77/$BFB4/
    /// $BEA3. They match pinned NTSC J/U v1.0 ROM and bank_8B.asm:
    /// $0050, $0074, $0080, $00E0, $FFE0 for index 0..4. The fourth and
    /// fifth initializers take their nonzero-parameter branches. The last
    /// word is signed -32, stored as wrapped 16-bit X. These unrelated
    /// asteroid, station, vortex and star placements do not share a useful
    /// exact stride or geometry rule; retain the five authored positions.
    /// RearViewActor rejects indices outside this bounded native spawn set.
    ///
    /// Issues #625 and #1007: the five independent Y immediates at
    /// $8B:BF29/$BF53/$BF7D/$BFBA/$BEA9 match pinned NTSC J/U v1.0 ROM
    /// and bank_8B.asm: $009F, $00A0, $0060, $0057, $0057 in that same
    /// native spawn order. The first three actor types occupy distinct
    /// station/debris heights, while vortex and rear stars share $0057.
    /// These are unsigned scene placements, without an exact useful
    /// progression across all five actor types. Retain the bounded Y choices
    /// rather than fit arithmetic to unrelated visual roles.
    ///
    /// Issues #625 and #1008: the five initializer attribute words at
    /// $8B:BF2F/$BF59/$BF83/$BFC0/$BEAF match pinned NTSC J/U v1.0 ROM
    /// and bank_8B.asm: $0800, $0C00, $0800, $0800, $0800. For bounded
    /// actor index i=0..4, the exact selector is $0800 OR ($0400 when
    /// i=1, otherwise zero). Index one is the station-under-attack actor;
    /// its palette bits differ from the asteroid, vortex and star actors.
    /// IntroDiscoverySprite.Draw applies the selected attributes to OAM.
    /// The rule covers only the five native spawn rows, without assigning
    /// meaning to an invalid index or altering their authored palette.
    ///
    /// Issues #625 and #1009: pinned NTSC J/U v1.0 ROM and bank_8B.asm
    /// give fractional X increments $4000, $1000, $0800 at
    /// $8B:BF3A/$BF64/$BF8E for rows 0..2. Each native pre-instruction
    /// then masks whole X with $01FF at $BF46/$BF70/$BF9A. Rows 3 and 4
    /// share callback $BFC6: its immediate at $BFCB subtracts $2000 from
    /// the fractional word with borrow into whole X, without that mask.
    /// Thus bounded row deltas in signed 16.16 are +$4000, +$1000,
    /// +$0800, -$2000, -$2000. The positive rates vary by actor and do
    /// not follow a useful common step; retain these authored motion choices
    /// and their native wrap policy instead of fitting an index formula.
    ///
    /// Issues #625 and #1010: all fifteen definition words at
    /// $8B:CF39/CE85/CE8B/CE91/CF0F match pinned NTSC J/U v1.0 ROM and
    /// bank_8B.asm. Their (initializer, definition pre-instruction, list)
    /// triples in spawn order are (BF22,BF35,CE4B), (BF4C,BF5F,CC47),
    /// (BF76,BF89,CC4F), (BFA0,BFC6,CC57), (BE7E,BEB5,CDA3).
    /// Definition addresses for rows 1..3 step by six bytes, but their
    /// callback/list identities do not derive from that stride; rows 0
    /// and 4 live elsewhere. Row 4 aliases the front-star definition;
    /// initializer parameter one replaces its active BEB5 callback with
    /// BFC6. Retain these five bounded actor identities and that override.
    ///
    /// Issues #625 and #1011: row one's list at $8B:CC47..CC4E has four
    /// words $000A, $9150, $94BC, $CC47 in pinned NTSC J/U v1.0 ROM and
    /// bank_8B.asm. Its exact bounded rule is to show bank-$8C under-attack
    /// spritemap $9150 for ten handler calls, then repeat from $CC47 while
    /// the scene owns the actor. IntroDiscoverySprite.Step follows the
    /// goto, so it cannot run into the adjacent small-asteroid list $CC4F.
    /// Keep the authored visual frame ROM-backed; no table is needed to
    /// describe this constant-period loop.
    ///
    /// Issues #625 and #1012: row two's list at $8B:CC4F..CC56 has four
    /// words $000A, $90FE, $94BC, $CC4F in pinned NTSC J/U v1.0 ROM and
    /// bank_8B.asm. It displays the authored bank-$8C small-asteroid
    /// spritemap $90FE for ten handler calls and jumps back to $CC4F.
    /// IntroDiscoverySprite.Step confines the cursor to that loop in both
    /// Ceres scenes; $CC57 is the neighboring vortex list. The exact period
    /// needs no table, while the visual spritemap remains ROM-backed.
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
