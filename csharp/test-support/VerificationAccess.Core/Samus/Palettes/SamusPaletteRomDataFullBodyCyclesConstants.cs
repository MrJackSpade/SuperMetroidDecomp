
namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Cartridge values of <see cref="SamusPaletteRomData.FullBodyCycles"/> that only verification reads.</summary>
internal static class SamusPaletteRomDataFullBodyCyclesConstants
{
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
    /// Issue #884 / #625: Power active-shinespark colors at $9B:9C20..9C9F; shade zero equals normal Power and Screw Attack shade zero.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// Issue #885 / #625: Varia active-shinespark colors at $9B:9E20..9E9F; shade zero equals Screw Attack shade zero.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// Issue #886 / #625: Gravity active-shinespark colors at $9B:A020..A09F; these duplicate the earlier $9B:95C0..963F allocation.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
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
    /// The stride describes pointer selection; installed artwork now supplies
    /// the authored target colors without reading those cartridge bytes at runtime.
    ///
    /// Hyper Beam target colors at $9B:A240..A37F have an independent
    /// #1165 review in SamusHyperBeamColorCatalog. This pointer mapping
    /// provides no retention justification for their remaining color inputs.
    /// </remarks>
    public const int HyperBeamPointers = 0x91d99e;
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
    /// Issue #871 / #625: Power Screw Attack colors at $9B:9CA0..9D1F; shade zero matches normal Power $9B:9400.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// Issue #872 / #625: Varia Screw Attack colors at $9B:9EA0..9F1F; shade zero differs from normal Varia only at transparent index zero.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// Issue #873 / #625: Gravity Screw Attack colors at $9B:A0A0..A11F; shade zero differs from normal Gravity at color slots1/12.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// </remarks>
    public const int ScrewAttackLists = 0x91da4a;
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
    /// Issue #876 / #625: Power Speed Booster colors at $9B:9B20..9B9F; the three bright rows also supply the loading palette.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// Issue #877 / #625: Varia Speed Booster colors at $9B:9D20..9D9F; the three bright rows also supply the loading palette.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
    /// Issue #878 / #625: Gravity Speed Booster colors at $9B:9F20..9F9F; these duplicate the earlier $9B:9540..95BF allocation.
    /// Color relationships require independent review under #1165; this
    /// pointer catalog makes no retention decision for the target colors.
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

    /// <summary>Provides full-body Samus palette lists for shinespark, Hyper Beam, Screw Attack, and speed states.</summary>
    extension(SamusPaletteRomData.FullBodyCycles)
    {
        /// <inheritdoc cref="SamusPaletteRomDataFullBodyCyclesConstants.ActiveShinesparkLists"/>
        internal static int ActiveShinesparkLists => SamusPaletteRomDataFullBodyCyclesConstants.ActiveShinesparkLists;
        /// <inheritdoc cref="SamusPaletteRomDataFullBodyCyclesConstants.HyperBeamPointers"/>
        internal static int HyperBeamPointers => SamusPaletteRomDataFullBodyCyclesConstants.HyperBeamPointers;
        /// <inheritdoc cref="SamusPaletteRomDataFullBodyCyclesConstants.ScrewAttackLists"/>
        internal static int ScrewAttackLists => SamusPaletteRomDataFullBodyCyclesConstants.ScrewAttackLists;
        /// <inheritdoc cref="SamusPaletteRomDataFullBodyCyclesConstants.SpeedBoostPointers"/>
        internal static int SpeedBoostPointers => SamusPaletteRomDataFullBodyCyclesConstants.SpeedBoostPointers;
        /// <inheritdoc cref="SamusPaletteRomDataFullBodyCyclesConstants.SpeedBoosterLists"/>
        internal static int SpeedBoosterLists => SamusPaletteRomDataFullBodyCyclesConstants.SpeedBoosterLists;
        /// <inheritdoc cref="SamusPaletteRomDataFullBodyCyclesConstants.StoredShineLists"/>
        internal static int StoredShineLists => SamusPaletteRomDataFullBodyCyclesConstants.StoredShineLists;
    }
}
