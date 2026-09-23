namespace SuperMetroid.Core.Game;

/// <summary>Immutable cartridge data and fixed hardware layout shared by Samus palettes.</summary>
/// <remarks>
/// Palette state machines retain their mutable timers and phase transitions. This catalog
/// names the ROM tables, object-program words, CGRAM ranges, and fixed-color endpoints those
/// machines consume, so a hexadecimal address never masquerades as gameplay behavior.
/// </remarks>
public static class SamusPaletteRomData
{
    /// <summary>Native banks containing Samus palette pointers, colors, and FX programs.</summary>
    public static class Banks
    {
        /// <summary>Bank <c>$91</c>, which owns Samus palette pointer lists.</summary>
        public const int Movement = 0x910000;
        /// <summary>Bank <c>$9B</c>, which owns Samus BGR555 color data.</summary>
        public const int Palette = 0x9b0000;
        /// <summary>Bank <c>$8D</c>, which owns generic palette-FX programs.</summary>
        public const int PaletteFx = 0x8d0000;
    }

    /// <summary>Shared full-body OBJ palette layout and suit selection table.</summary>
    public static class Common
    {
        /// <summary><c>$91:D727</c>, Power/Varia/Gravity normal-palette pointers.</summary>
        /// <remarks>
        /// Issue #859 / #625: the pinned NTSC J/U v1.0 ROM has three little-endian
        /// words <c>$9400,$9520,$9800</c> at byte offsets 0, 2, 4. They select
        /// the authored normal Power, Varia, and Gravity palettes in bank
        /// <c>$9B</c>. Every production suit selector reaches only those even
        /// offsets; Gravity has priority over Varia when both equipment bits
        /// are set. The X-ray and projectile catalogs alias this same physical
        /// table. These three distinct palette identities are retained as
        /// authored selectors; a numeric stride would not reproduce them.
        /// </remarks>
        public const int NormalSuitPointers = 0x91d727;
        /// <summary>First CGRAM color of Samus OBJ palette four.</summary>
        public const int SamusObjPaletteStart = 192;
        /// <summary>First CGRAM color of suitless Samus OBJ palette seven.</summary>
        public const int SuitlessObjPaletteStart = 240;
        /// <summary>Number of colors in one complete SNES palette.</summary>
        public const int ColorsPerObjPalette = 16;
        /// <summary>Samus-palette-relative visor color index.</summary>
        public const int VisorColorOffset = 4;
    }

    /// <summary>Ordinary damage-flash and intro-restoration palettes.</summary>
    public static class HurtFlash
    {
        /// <summary><c>$9B:A380</c>, the sixteen-color hurt-flash palette.</summary>
        /// <remarks>
        /// Issue #863 / #625: the pinned NTSC J/U v1.0 ROM's sixteen BGR555
        /// words are copied unchanged to Samus OBJ CGRAM 192..207 on odd
        /// hurt-counter calls 1, 3, and 5. Color zero is <c>$0000</c>. For
        /// each color index <c>c=1..15</c> and each five-bit channel <c>q</c>,
        /// <c>q(hurt[c]) = floor((2*q(intro[c]) + 5*31)/7)</c>, where
        /// <c>intro</c> is the adjacent palette at <c>$9B:A3A0</c>.
        /// This bounded 5/7 blend toward white matches all sixteen ROM words
        /// exactly; it describes the stored relationship, not a native runtime
        /// calculation. The palette remains live cartridge data.
        /// </remarks>
        public const int Colors = 0x9ba380;
        /// <summary><c>$9B:A3A0</c>, the sixteen-color cinematic Samus palette.</summary>
        /// <remarks>
        /// Issue #864 / #625: all sixteen BGR555 words match the pinned NTSC
        /// J/U v1.0 ROM and native bank-$9B listing. Color zero is
        /// <c>$3800</c>; the remaining positions select seven authored grey
        /// shades with red = green = <c>4,8,13,18,22,27,31</c>. Blue equals
        /// red minus two except at the darkest shade, where all channels are
        /// four. The slot-to-shade choices remain authored data; even a uniform
        /// seven-step <c>31*k/7</c> ramp misses levels 4 and 6 with floor,
        /// or level 2 with nearest rounding. The hurt palette (#863) is an exact
        /// forward blend from these colors, but its integer floor loses source values.
        /// Production copies indices 0..15 unchanged to Samus OBJ CGRAM
        /// 192..207 on cinematic hurt-counter calls 2, 4, and 6.
        /// </remarks>
        public const int IntroColors = 0x9ba3a0;
    }

    /// <summary>Shared visor colors used by room animation and the X-ray Scope.</summary>
    public static class Visor
    {
        /// <summary><c>$9B:A3C0</c>, six widening/cycling visor colors.</summary>
        /// <remarks>
        /// Issue #865 / #625: all six BGR555 words match the pinned NTSC J/U
        /// v1.0 ROM. For widening index <c>k=0..2</c>, each five-bit channel
        /// is <c>floor(((2-k)*q($3BE0) + k*31 + 1)/2)</c>, yielding
        /// <c>$3BE0,$5FF0,$7FFF</c>. For full-beam index <c>k=0..2</c>,
        /// each channel is <c>q($43FF)-5*k</c>, yielding
        /// <c>$43FF,$2F5A,$1AB5</c>. X-ray reads even byte offsets
        /// 0, 2, 4 while widening and 6, 8, 10 while fully open; room visor
        /// animation cycles the latter three. Both write OBJ CGRAM color 196.
        /// An externally edited packed timer/index can cause a raw bus read
        /// outside these six words; this formula describes only the ordinary
        /// bounded offsets and does not invent adjacent values.
        /// </remarks>
        public const int Colors = 0x9ba3c0;
        /// <summary>Packed offset-six/timer-one reset used outside animated rooms.</summary>
        public const ushort NormalRoomReset = 0x0601;
        /// <summary>Timer reloaded after one visor color is copied.</summary>
        public const ushort FrameDelay = 5;
        /// <summary>First byte offset in the three-color steady visor cycle.</summary>
        public const byte CycleFirstByteOffset = 6;
        /// <summary>Exclusive byte offset ending the three-color steady visor cycle.</summary>
        public const byte CycleEndByteOffset = 12;
    }

    /// <summary>Pointer tables used by special full-body palette handlers.</summary>
    public static class FullBodyCycles
    {
        /// <summary><c>$91:D998</c>, suit-indexed Speed Booster flash palettes.</summary>
        /// <remarks>
        /// Issue #866 / #625: the pinned NTSC J/U v1.0 ROM's three
        /// little-endian pointers at byte offsets <c>2*i</c>, <c>i=0..2</c>,
        /// are exactly <c>$9B80+$0200*i</c>. They select the Power, Varia,
        /// and Gravity speed-boost shades that also appear in loading
        /// palette programs #856–#858. The Metroid-attachment palette
        /// caller reaches only byte offsets 0, 2, 4, with Gravity priority,
        /// and copies sixteen target colors into Samus OBJ CGRAM 192..207.
        /// This is a bounded pointer relationship; the target colors remain
        /// authored cartridge data.
        /// </remarks>
        public const int SpeedBoostPointers = 0x91d998;
        /// <summary><c>$91:DA4A</c>, suit-indexed Screw Attack palette lists.</summary>
        /// <remarks>
        /// Issue #869 / #625: the pinned NTSC J/U v1.0 ROM stores three
        /// little-endian pointers <c>$DA50+$000C*i</c> for Power, Varia,
        /// and Gravity suit index <c>i=0..2</c>. The production selector
        /// reaches only byte offsets 0, 2, 4, with Gravity priority.
        /// Each target is a separate six-word bank-$91 palette-pointer
        /// list, indexed by byte offsets 0, 2, 4, 6, 8, 10; this proof
        /// covers only the top-level three-word selector.
        ///
        /// Issue #870 / #625: all eighteen nested words in
        /// <c>$91:DA50..DA73</c> match
        /// <c>$9CA0+$0200*s+$0020*min(p,6-p)</c> for suit <c>s=0..2</c>
        /// and phase <c>p=0..5</c>. The six phases visit shade offsets
        /// 0, 1, 2, 3, 2, 1 before wrapping. The selected bank-$9B target
        /// remains a live authored sixteen-color palette; this formula
        /// describes only the bounded pointer matrix.
        ///
        /// Issue #871 / #625: all 64 Power Suit BGR555 target words at
        /// <c>$9B:9CA0..9D1F</c> match the pinned ROM/native listing.
        /// Shade zero is exactly the normal Power palette at <c>$9B:9400</c>.
        /// A per-slot, per-channel clipped linear step from shade zero
        /// reproduces 34 of 48 four-shade component sequences; fourteen
        /// require authored exceptions, mostly a blue-channel jump in the
        /// final shade. The four distinct rows remain live authored colors
        /// for shade 0..3, color 0..15, then the six-phase pointer matrix
        /// reuses shades 2 and 1. Encoding slopes and exceptions would
        /// obscure these deliberately chosen colors.
        ///
        /// Issue #872 / #625: all 64 Varia Suit BGR555 target words at
        /// <c>$9B:9EA0..9F1F</c> match the pinned ROM/native listing.
        /// Shade zero matches normal Varia <c>$9B:9520</c> at fifteen
        /// colors; color zero is deliberately <c>$3800</c> here instead of
        /// <c>$0000</c>. A per-slot, per-channel clipped linear step fits
        /// 35 of 48 four-shade component sequences, leaving thirteen
        /// authored exceptions. Retain the four live color rows for shade
        /// 0..3, color 0..15, with shades 2 and 1 reused by the pointer
        /// matrix's six-phase cycle.
        ///
        /// Issue #873 / #625: all 64 Gravity Suit BGR555 target words at
        /// <c>$9B:A0A0..A11F</c> match the pinned ROM/native listing.
        /// Shade zero differs from normal Gravity <c>$9B:9800</c> at
        /// color one (<c>$00CE</c> versus <c>$0108</c>) and color twelve
        /// (<c>$0216</c> versus <c>$0274</c>). A per-slot, per-channel
        /// clipped linear step fits only 32 of 48 four-shade component
        /// sequences; sixteen need authored exceptions, mostly a
        /// blue-channel jump at the final shade. Retain the four live
        /// color rows for shade 0..3, color 0..15; the pointer matrix
        /// reuses shades 2 and 1 in its six-phase cycle.
        /// </remarks>
        public const int ScrewAttackLists = 0x91da4a;
        /// <summary><c>$91:DAA9</c>, suit-indexed active Speed Booster palette lists.</summary>
        /// <remarks>
        /// Issue #874 / #625: the pinned NTSC J/U v1.0 ROM's three
        /// little-endian words are exactly <c>$DAAF+8*s</c> for suit
        /// index <c>s=0..2</c>, matching the native bank-$91 listing.
        /// The caller selects Power, Varia, or Gravity with byte offset
        /// <c>2*s</c>, reads four bank-$9B palette pointers from the chosen
        /// bank-$91 list at phase offsets <c>0,2,4,6</c>, and then pins
        /// the last phase. This stride describes only the three list
        /// addresses; the nested pointers and colors are separate data.
        ///
        /// Issue #875 / #625: the twelve nested words at
        /// <c>$91:DAAF..DAC6</c> are exactly
        /// <c>$9B20+$0200*s+$0020*p</c> for suit index <c>s=0..2</c>
        /// and phase <c>p=0..3</c>. Every word matches the pinned ROM and
        /// native bank-$91 listing. The caller indexes the four phase
        /// pointers with byte offsets <c>0,2,4,6</c> and pins phase three;
        /// each target is a complete sixteen-color bank-$9B palette.
        /// The formula describes target addresses, not target colors.
        ///
        /// Issue #876 / #625: all 64 Power Suit BGR555 target words at
        /// <c>$9B:9B20..9B9F</c> match the pinned ROM/native listing.
        /// Phase zero matches normal Power <c>$9B:9400</c> at fifteen
        /// colors; color zero is <c>$0000</c> here instead of
        /// <c>$3800</c>. The complete first row also matches the first
        /// stored-shine row at <c>$9B:9BA0</c>; across all four rows only
        /// 17 of 64 words match the corresponding stored-shine positions.
        /// A per-slot, per-channel clipped first-step rule fits only
        /// 20 of 48 four-phase component sequences. Retain the four
        /// authored sixteen-color rows, including the final row that
        /// remains selected during sustained Speed Booster running.
        ///
        /// Issue #877 / #625: all 64 Varia Suit BGR555 target words at
        /// <c>$9B:9D20..9D9F</c> match the pinned ROM/native listing.
        /// Phase zero exactly equals normal Varia <c>$9B:9520</c> and
        /// the first stored-shine row at <c>$9B:9DA0</c>; across all
        /// four rows, only 17 of 64 positions match stored shine.
        /// A per-slot, per-channel clipped first-step rule fits only
        /// 23 of 48 four-phase component sequences. Retain all four
        /// authored sixteen-color rows, with the final row selected
        /// continuously after the active Speed Booster ramp.
        ///
        /// Issue #878 / #625: all 64 Gravity Suit BGR555 target words at
        /// <c>$9B:9F20..9F9F</c> match the pinned ROM/native listing and
        /// exactly duplicate the earlier ROM block at
        /// <c>$9B:9540..95BF</c>, word for word. The active pointer
        /// matrix still targets <c>$9F20..9F9F</c>. Phase zero differs
        /// from normal Gravity <c>$9B:9800</c> at colors zero, one,
        /// and twelve. Only 16 of 64 corresponding words match the
        /// stored-shine rows, and a per-slot, per-channel clipped
        /// first-step rule fits 24 of 48 four-phase sequences. Retain
        /// the four live authored rows; phase three stays selected
        /// during sustained Speed Booster running.
        /// </remarks>
        public const int SpeedBoosterLists = 0x91daa9;
        /// <summary><c>$91:DB10</c>, suit-indexed stored-shine palette lists.</summary>
        /// <remarks>
        /// Issue #879 / #625: the pinned NTSC J/U v1.0 ROM's three
        /// little-endian words are exactly <c>$DB16+12*s</c> for suit
        /// index <c>s=0..2</c>, matching the native bank-$91 listing.
        /// Palette handler one selects Power, Varia, or Gravity with
        /// byte offset <c>2*s</c>, reads the selected bank-$91 list at
        /// six phase offsets <c>0,2,4,6,8,10</c>, and wraps to zero.
        /// The caller copies sixteen colors from each bank-$9B target.
        /// This formula describes list addresses only; the nested
        /// pointers and target colors are separate proof targets.
        ///
        /// Issue #880 / #625: the eighteen nested words at
        /// <c>$91:DB16..DB39</c> are exactly
        /// <c>$9BA0+$0200*s+$0020*min(p,6-p)</c> for suit index
        /// <c>s=0..2</c> and phase <c>p=0..5</c>. Every word matches
        /// the pinned ROM and native bank-$91 listing. Palette handler
        /// one cycles six byte offsets <c>0,2,4,6,8,10</c>, so each
        /// suit visits color rows <c>0,1,2,3,2,1</c> before wrapping.
        /// Each target is a complete sixteen-color bank-$9B palette;
        /// the formula does not describe its authored colors.
        ///
        /// Issue #881 / #625: the 192 BGR555 words in four distinct
        /// stored-shine rows per suit at <c>$9B:9BA0..9C1F</c>,
        /// <c>$9B:9DA0..9E1F</c>, and <c>$9B:9FA0..A01F</c> follow an
        /// exact reuse rule. For suit <c>s=0..2</c>, row <c>p=0..3</c>,
        /// and color <c>c=1..15</c>, the word equals the same color in
        /// the death-sequence/beam-charge row at
        /// <c>$9B:9820+$0100*s+$0040*p</c>; color zero is always
        /// <c>$0000</c>. Direct comparison of all 192 pinned ROM words
        /// has zero mismatches, consistent with the native bank-$9B
        /// listing. The pointer matrix visits rows <c>0,1,2,3,2,1</c>.
        /// The source rows remain authored cartridge palettes.
        /// </remarks>
        public const int StoredShineLists = 0x91db10;
        /// <summary><c>$91:DB75</c>, suit-indexed active-shinespark palette lists.</summary>
        /// <remarks>
        /// Issue #882 / #625: the pinned NTSC J/U v1.0 ROM's three
        /// little-endian words are exactly <c>$DB7B+8*s</c> for suit
        /// index <c>s=0..2</c>, matching the native bank-$91 listing.
        /// Palette handler six selects Power, Varia, or Gravity with
        /// byte offset <c>2*s</c>, then reads four phase pointers at
        /// offsets <c>0,2,4,6</c> and wraps to zero. Each selected
        /// bank-$9B target supplies sixteen Samus OBJ colors. This
        /// formula describes list addresses; nested pointers and
        /// colors are separate proof targets.
        ///
        /// Issue #883 / #625: the twelve nested words at
        /// <c>$91:DB7B..DB92</c> are exactly
        /// <c>$9C20+$0200*s+$0020*p</c> for suit index
        /// <c>s=0..2</c> and phase <c>p=0..3</c>. Every word matches
        /// the pinned ROM and native bank-$91 listing. Palette handler
        /// six cycles byte offsets <c>0,2,4,6</c> and wraps; each
        /// target supplies sixteen bank-$9B colors. The formula
        /// describes target addresses, not their authored colors.
        ///
        /// Issue #884 / #625: all 64 Power Suit active-shinespark
        /// BGR555 words at <c>$9B:9C20..9C9F</c> match the pinned ROM
        /// and native bank-$9B listing. Phase zero exactly duplicates
        /// normal Power <c>$9B:9400</c> and Screw Attack shade zero
        /// <c>$9B:9CA0</c>; the other three rows have no exact
        /// sixteen-color duplicate among the nearby Samus palettes.
        /// Only 21 of 64 corresponding words equal the Screw Attack
        /// rows. A per-slot, per-channel clipped first-step rule fits
        /// 24 of 48 four-phase component sequences. Retain these four
        /// live authored rows; palette handler six repeats them in
        /// order while the shinespark palette is active.
        ///
        /// Issue #885 / #625: all 64 Varia Suit active-shinespark
        /// BGR555 words at <c>$9B:9E20..9E9F</c> match the pinned ROM
        /// and native bank-$9B listing. Phase zero exactly duplicates
        /// Screw Attack shade zero <c>$9B:9EA0</c> and differs from
        /// normal Varia <c>$9B:9520</c> only at color zero
        /// (<c>$3800</c> versus <c>$0000</c>). The remaining three
        /// rows have no exact sixteen-color duplicate among nearby
        /// Samus palettes. Only 25 of 64 corresponding words equal
        /// Screw Attack; a clipped first-step channel rule fits 25 of
        /// 48 four-phase sequences. Retain the four live authored rows,
        /// which palette handler six repeats while active.
        ///
        /// Issue #886 / #625: all 64 Gravity Suit active-shinespark
        /// BGR555 words at <c>$9B:A020..A09F</c> match the pinned ROM
        /// and native bank-$9B listing and exactly duplicate the
        /// earlier block at <c>$9B:95C0..963F</c>, word for word.
        /// The active pointer matrix still targets <c>$A020..A09F</c>.
        /// Phase zero differs from normal Gravity <c>$9B:9800</c> at
        /// colors one and twelve. Only 23 of 64 corresponding words
        /// equal the Screw Attack rows, while a clipped first-step
        /// channel rule fits 16 of 48 four-phase sequences. Retain
        /// these four live authored rows; palette handler six repeats
        /// them while active.
        /// </remarks>
        public const int ActiveShinesparkLists = 0x91db75;
        /// <summary><c>$91:D99E</c>, ten full-body Hyper Beam palette pointers.</summary>
        /// <remarks>
        /// Issue #867 / #625: the pinned NTSC J/U v1.0 ROM's ten
        /// little-endian words are exactly <c>$A360-$0020*i</c> for
        /// <c>i=0..9</c>, ending at <c>$A240</c>. Each points to a distinct
        /// sixteen-color bank-$9B palette. Rainbow Samus starts at zero,
        /// reads byte offset <c>2*i</c>, and wraps after index nine; the
        /// managed caller also bounds restored index values with modulo ten.
        /// The stride describes pointer selection, while target colors stay
        /// live authored cartridge data.
        ///
        /// Issue #868 / #625: all 160 BGR555 target words at
        /// <c>$9B:A240..A37F</c> match the native bank-$9B listing. In each
        /// 16-color frame, slots 2 and 6, 3 and 15, and 10 and 12 are equal,
        /// leaving thirteen distinct colors. Color zero follows
        /// <c>$3800,$7FFF,$0000,$0400</c>, then six <c>$0000</c> words.
        /// A fixed integer shade offset per nonzero slot from slot 1 misses
        /// 124 of 450 channel values; exact residuals need context-dependent
        /// correction classes as well as ten distinct hue bases. Those
        /// authored hue and shade choices are clearer as the live palette
        /// cycle. The bounded domain is frame 0..9 and color 0..15; this
        /// describes stored data, not a native runtime color generator.
        /// </remarks>
        public const int HyperBeamPointers = 0x91d99e;
        /// <summary>Number of full-body Hyper Beam palettes.</summary>
        public const int HyperBeamPaletteCount = 10;
    }

    /// <summary>Bank-$8D palette object spawned with the Hyper Beam.</summary>
    public static class HyperBeamFx
    {
        /// <summary><c>$8D:E1F0</c>, two-word Hyper Beam palette-FX definition.</summary>
        /// <remarks>
        /// Issue #887 / #625: the pinned NTSC J/U v1.0 ROM stores
        /// <c>$C685,$D900</c> here, selecting the no-op setup and
        /// bank-$8D program. Its entry at <c>$D900</c> is
        /// <c>$C655,$01C2</c>. Ten records start at
        /// <c>$D904+20*i</c>, <c>i=0..9</c>; every duration is two,
        /// every terminator at record offset eighteen is
        /// <c>$C595</c>, and the terminal <c>$D9CC</c> command is
        /// <c>$C61E,$D904</c>. All 26 control words match the pinned
        /// ROM and native bank-$8D listing. The compiled resolver
        /// accepts only entry, aligned records, and terminal loop;
        /// it rejects adjacent or unaligned restored pointers.
        /// The caller loads eight live color words at record offset
        /// two into OBJ palette six, color one, displays each record
        /// for two handler calls, and loops after record nine.
        /// Color payloads are a separate authored table.
        /// </remarks>
        public const int ObjectDefinition = 0x8de1f0;
        /// <summary>Expected no-op setup callback in the object definition.</summary>
        public const ushort SetupCallback = PaletteFxSetupCodes.Null;
        /// <summary>Initial instruction list stored by the object definition.</summary>
        public const ushort InitialList = 0xd900;
        /// <summary>First timed color record after the destination-selection command.</summary>
        public const ushort FirstFrame = 0xd904;
        /// <summary>Palette-buffer byte index selecting OBJ palette six, color one.</summary>
        public const ushort DestinationByteIndex = 0x01c2;
        /// <summary>Instruction <c>$C655</c>: select palette-buffer byte index from Y.</summary>
        public const ushort SetColorIndex = PaletteFxInstructionCodes.SetColorIndex;
        /// <summary>Instruction <c>$C595</c>: finish the current timed palette record.</summary>
        public const ushort Done = PaletteFxInstructionCodes.Wait;
        /// <summary>Instruction <c>$C61E</c>: jump to the instruction pointer in Y.</summary>
        public const ushort Goto = PaletteFxInstructionCodes.Goto;
        /// <summary>Number of timed color records in the loop.</summary>
        public const int FrameCount = 10;
        /// <summary>Number of colors written by each record.</summary>
        public const int ColorsPerFrame = 8;
        /// <summary>Bytes occupied by a duration, eight colors, and the done opcode.</summary>
        public const int FrameByteCount = 20;
    }

    /// <summary>Palette tables consumed by the fatal-damage sequence.</summary>
    public static class Death
    {
        /// <summary><c>$9B:B7D3</c>, three families of ten suited palette pointers.</summary>
        public const int SuitPointers = 0x9bb7d3;
        /// <summary><c>$9B:B80F</c>, ten suitless palette pointers.</summary>
        public const int SuitlessPointers = 0x9bb80f;
        /// <summary><c>$9B:B823</c>, nine interleaved timer/palette-index records.</summary>
        public const int ExplosionTimingAndPaletteIndices = 0x9bb823;
        /// <summary><c>$9B:B835</c>, 22 whiteout shades from black through white.</summary>
        public const int WhiteoutShades = 0x9bb835;
        /// <summary>Number of suited and suitless explosion palette variants.</summary>
        public const int PaletteCount = 10;
        /// <summary>Number of shades in the inclusive whiteout ramp.</summary>
        public const int WhiteoutShadeCount = 22;
    }

    /// <summary>Two independently timed palette streams used by Crystal Flash.</summary>
    public static class CrystalFlash
    {
        /// <summary><c>$90:C3C9</c>, twelve beam-loadout palette pointers.</summary>
        public const int BeamPalettePointers = 0x90c3c9;
        /// <summary>Bank <c>$90</c>, containing the restored projectile palettes.</summary>
        public const int BeamPaletteBank = 0x900000;
        /// <summary><c>$91:DC00</c>, ten body-palette pointer/timer records.</summary>
        /// <remarks>
        /// The stock bank-$9B pointers at four-byte record offsets are exactly
        /// <c>$96C0 + $20 * (i &lt;= 5 ? i : 10 - i)</c> for record index
        /// <c>i = 0..9</c>; all ten match the pinned NTSC J/U v1.0 ROM.
        /// Keep the pointer words as live presentation data so installed
        /// palette content can select its authored colours. The adjacent
        /// duration words are compiled separately. Investigation: #625 / #669.
        /// </remarks>
        public const int BodyRecords = 0x91dc00;
        /// <summary><c>$91:DC28</c>, six bubble-palette pointers.</summary>
        /// <remarks>
        /// The six stock bank-$9B pointers are exactly
        /// <c>$96D4 + $20 * i</c> for even byte offsets <c>2*i</c>,
        /// <c>i = 0..5</c>; all match the pinned NTSC J/U v1.0 ROM.
        /// The next word at <c>$91:DC34</c> is instruction bytes, not a
        /// seventh pointer. Keep the six words as live presentation data for
        /// installed palettes; the bubble cursor wraps at byte offset 12.
        /// Investigation: #625 / #670.
        /// </remarks>
        public const int BubblePointers = 0x91dc28;
        /// <summary>Number of beam-loadout palette pointers.</summary>
        public const int BeamPaletteCount = 12;
        /// <summary>Number of Crystal Flash body records.</summary>
        public const int BodyRecordCount = 10;
        /// <summary>Bytes occupied by one body pointer/timer record.</summary>
        public const int BodyRecordByteCount = 4;
        /// <summary>Number of bubble palette pointers.</summary>
        public const int BubblePaletteCount = 6;
        /// <summary>Number of colors copied for the body portion.</summary>
        public const int BodyColorCount = 10;
        /// <summary>Number of colors copied for the bubble portion.</summary>
        public const int BubbleColorCount = 6;
        /// <summary>First CGRAM color of OBJ palette six.</summary>
        public const int BodyCgramStart = 0xe0;
        /// <summary>First CGRAM color of the bubble portion in OBJ palette six.</summary>
        public const int BubbleCgramStart = 0xea;
    }

    /// <summary>Raw COLDATA endpoints used by the shared Varia/Gravity pickup sequence.</summary>
    public static class SuitPickup
    {
        /// <summary>Initial red byte shared by both transformations.</summary>
        public const byte InitialRed = 48;
        /// <summary>Initial/terminal Varia green byte.</summary>
        public const byte VariaGreen = 80;
        /// <summary>Initial/terminal Gravity green byte.</summary>
        public const byte GravityGreen = 73;
        /// <summary>Initial Varia blue component-enable byte.</summary>
        public const byte VariaBlue = 0x80;
        /// <summary>Initial/terminal Gravity blue component-enable byte.</summary>
        public const byte GravityBlue = 0x90;
        /// <summary>White-ramp red endpoint.</summary>
        public const byte WhiteRed = 63;
        /// <summary>White-ramp green endpoint.</summary>
        public const byte WhiteGreen = 95;
        /// <summary>White-ramp blue endpoint including component enable.</summary>
        public const byte WhiteBlue = 0x9f;
        /// <summary>Varia orange-ramp green endpoint.</summary>
        public const byte VariaOrangeGreen = 77;
        /// <summary>Varia orange-ramp blue endpoint including component enable.</summary>
        public const byte VariaOrangeBlue = 0x83;
        /// <summary>Final fixed-color register reset red byte.</summary>
        public const byte ResetRed = 32;
        /// <summary>Final fixed-color register reset green byte.</summary>
        public const byte ResetGreen = 64;
        /// <summary>Final fixed-color register reset blue byte.</summary>
        public const byte ResetBlue = 0x80;
    }

    /// <summary>Fixed-color tables used by Power Bomb and Crystal Flash HDMA.</summary>
    public static class PowerBomb
    {
        /// <summary><c>$88:9079</c>, sixteen RGB triplets for the pre-explosion.</summary>
        public const int PreExplosionColors = 0x889079;
        /// <summary><c>$88:8D85</c>, radius-indexed RGB triplets for the explosion.</summary>
        public const int ExplosionColors = 0x888d85;
        /// <summary>Each fixed-color record stores red, green, and blue bytes.</summary>
        public const int BytesPerColor = 3;
        /// <summary>Five-bit component payload mask in a COLDATA byte.</summary>
        public const byte ComponentMask = 0x1f;
        /// <summary>Number of pre-explosion fixed-color triplets.</summary>
        public const int PreExplosionColorCount = 16;
        /// <summary>Number of explosion fixed-color triplets addressed by radius.</summary>
        public const int ExplosionColorCount = 32;
    }
}
