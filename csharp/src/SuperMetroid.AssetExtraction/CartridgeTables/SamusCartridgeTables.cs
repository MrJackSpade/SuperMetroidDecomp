using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Cartridge regions whose bank-local pointers Samus and projectile codecs resolve.</summary>
[CpuBusBoundary("catalog: Samus cartridge tables")]
internal static class SamusCartridgeTables
{
    /// <summary>
    /// $92:8000-$92:FFFF: Samus animation data — seven-byte body DMA definitions and Samus
    /// spritemaps addressed by bank-local pointers.
    /// </summary>
    internal static readonly CpuBusCell AnimationBank = CpuBusCell.CartridgeBytes(nameof(AnimationBank), 0x928000, 0x8000,
        "body DMA definitions (7-byte records) and spritemaps (u16 count + 5-byte parts)");

    /// <summary>
    /// $93:8000-$93:FFFF: projectile definitions and spritemaps. Projectile spritemap identifiers are
    /// bank-local pointers to a u16 part count followed by five-byte OAM parts.
    /// </summary>
    internal static readonly CpuBusCell ProjectileBank = CpuBusCell.CartridgeBytes(nameof(ProjectileBank), 0x938000, 0x8000,
        "projectile spritemaps (u16 count + 5-byte parts)");

    /// <summary>$9B:9B20: the Power Suit's first Speed Booster cycle shade.</summary>
    internal const ushort SpeedBoosterCyclePalettes = 0x9b20;
    /// <summary>$9B:9BA0: the Power Suit's first stored-shine cycle shade.</summary>
    internal const ushort StoredShineCyclePalettes = 0x9ba0;
    /// <summary>$9B:9C20: the Power Suit's first active-shinespark cycle shade.</summary>
    internal const ushort ActiveShinesparkCyclePalettes = 0x9c20;
    /// <summary>$9B:9CA0: the Power Suit's first Screw Attack cycle shade.</summary>
    internal const ushort ScrewAttackCyclePalettes = 0x9ca0;
    /// <summary>Byte distance between one suit's cycle palettes and the next suit's.</summary>
    internal const int CyclePaletteSuitStride = 0x200;
    /// <summary>Byte distance between adjacent sixteen-color shades.</summary>
    internal const int CyclePaletteShadeStride = 0x20;
    /// <summary>$9B:A360: the first sixteen-color Hyper Beam frame; later frames descend.</summary>
    internal const int HyperBeamPalettes = 0x9ba360;
    /// <summary>Byte distance between descending Hyper Beam frames.</summary>
    internal const int HyperBeamPaletteStride = 0x20;

    /// <summary>$91:0000: the bank of the X-ray revealed-block tables and their commands.</summary>
    internal const int XrayRevealTableBank = 0x910000;
    /// <summary>$91:D2D6 RevealedBlockTable_0: the X-ray reveal table keyed by the level word's block-type nibble.</summary>
    internal const ushort XrayRevealBlockTypeTable = 0xd2d6;

    /// <summary>$92:90ED: the first Samus spritemap; spritemap pointers below it are invalid.</summary>
    internal const int FirstSpritemap = 0x90ed;

    /// <summary>$92:808D: the spritemap pointer of each pose-half table entry.</summary>
    internal const int SpritemapPointers = 0x92808d;

    /// <summary>$92:9263: each pose's first upper-half table entry.</summary>
    internal const int SpritemapTopBases = 0x929263;

    /// <summary>$92:945D: each pose's first lower-half table entry.</summary>
    internal const int SpritemapBottomBases = 0x92945d;

    /// <summary>$92:CBEE: the first seven-byte body DMA definition record; the rest follow it.</summary>
    internal const int FirstBodyDefinition = 0xcbee;

    /// <summary>Bytes in one body DMA definition record.</summary>
    internal const int BodyDefinitionBytes = 7;

    /// <summary>$92:DB48: the first four-byte body frame selector; the rest follow it.</summary>
    internal const int BodyFrames = 0x92db48;

    /// <summary>The bank-$92 address of the first body frame selector.</summary>
    internal const int FirstBodyFrame = 0xdb48;

    /// <summary>$84:839D, eight pointers to frame-zero item draw instructions.</summary>
    internal const int ItemDrawPointers = 0x84839d;

    /// <summary><c>$90:C7DF</c>, pose-indexed drawing-data pointers.</summary>
    internal const int PoseDrawingDataPointers = 0x90c7df;

    /// <summary><c>$90:C7A5</c>, direction-indexed tile-list pointers.</summary>
    internal const int TileListPointers = 0x90c7a5;

    /// <summary><c>$92:D94E</c>, animation-record-list pointer for each pose.</summary>
    internal const int AnimationDefinitionListPointers = 0x92d94e;

    /// <summary><c>$92:D91E</c>, top-definition-list pointer for each graphics set.</summary>
    internal const int TopDefinitionListPointers = 0x92d91e;

    /// <summary><c>$92:D938</c>, bottom-definition-list pointer for each graphics set.</summary>
    internal const int BottomDefinitionListPointers = 0x92d938;

    /// <summary>Direct OAM attribute-list pointers indexed by atmospheric-effect type.</summary>
    internal const int AtmosphericSpriteAttributeListPointers = 0x908bff;

    /// <summary><c>$9B:B7D3</c>, three families of ten suited palette pointers.</summary>
    /// <remarks>
    /// Issue #889 / #625: all thirty little-endian words at
    /// <c>$9B:B7D3..B80E</c> match the pinned NTSC J/U v1.0 ROM
    /// and native bank-$9B listing. For suit <c>s=0..2</c> and
    /// palette index <c>k=0..9</c>, index zero points to
    /// <c>$9820+$0100*s</c>, index one to the shared yellow flash
    /// at <c>$9420</c>, indices two through eight to
    /// <c>$9820+$0100*s+$0020*(k-1)</c>, and index nine to the
    /// shared final palette at <c>$A220</c>. The caller selects
    /// Power, Varia, or Gravity with native suit offsets
    /// <c>0,2,4</c> (Gravity priority), indexes zero/one during
    /// flashing and zero/two through nine during explosion, then
    /// copies all sixteen target colors. This formula describes
    /// pointers; the target palettes remain separate data.
    ///
    /// Issue #894 / #625: all 128 Power Suit BGR555 words in
    /// eight death/beam-charge rows at <c>$9B:9820..991F</c>
    /// match the pinned ROM and native bank-$9B listing. For
    /// frame <c>f=0..7</c> and color <c>c=0..15</c>, take each
    /// five-bit component <c>b</c> from the authored first row
    /// at <c>$9820</c>; the corresponding component is exactly
    /// <c>floor((b*(8-f)+31*f)/8)</c>. Direct comparison found
    /// zero mismatches across all 128 words. Pointer index zero
    /// selects frame zero and indices two through eight select
    /// frames one through seven. Issue1165 implements this shared fade in
    /// SamusPaletteFade for both charge and death. Base-input review remains separate.
    ///
    /// Issue #895 / #625: all 128 Varia Suit BGR555 words in
    /// eight death/beam-charge rows at <c>$9B:9920..9A1F</c>
    /// match the pinned ROM and native bank-$9B listing. For
    /// frame <c>f=0..7</c>, each five-bit component <c>b</c> from
    /// the authored first row at <c>$9920</c> becomes exactly
    /// <c>floor((b*(8-f)+31*f)/8)</c>. Direct comparison of all
    /// 128 words found zero mismatches. Pointer index zero selects
    /// frame zero and indices two through eight select frames
    /// one through seven; the first four frames are also used
    /// for beam charging, and alternate rows feed stored shine.
    /// The base row remains authored presentation data.
    ///
    /// Issue #896 / #625: all 128 Gravity Suit BGR555 words in
    /// eight death/beam-charge rows at <c>$9B:9A20..9B1F</c>
    /// match the pinned ROM and native bank-$9B listing. For
    /// frame <c>f=0..7</c>, each five-bit component <c>b</c> from
    /// the authored first row at <c>$9A20</c> becomes exactly
    /// <c>floor((b*(8-f)+31*f)/8)</c>. Direct comparison of all
    /// 128 words found zero mismatches, including the distinct
    /// Gravity source colors.
    /// Pointer index zero selects frame zero and indices two
    /// through eight select frames one through seven. The first
    /// four rows also serve beam charging; alternate rows feed
    /// stored shine. The base row remains authored data.
    ///
    /// Issue #898 / #625: the shared yellow-flash palette at
    /// <c>$9B:9420..943F</c> contains exactly sixteen
    /// <c>$03FF</c> BGR555 words in the pinned ROM/native listing.
    /// Suited pointer index one selects it for Power, Varia, and
    /// Gravity while the death flash alternates with index zero.
    /// The suitless pointer at index one instead repeats its
    /// ordinary first row at <c>$A120</c>. The uniform yellow
    /// row needs no per-color authored exceptions.
    /// </remarks>
    internal const int SuitPointers = 0x9bb7d3;

    /// <summary><c>$9B:B80F</c>, ten suitless palette pointers.</summary>
    /// <remarks>
    /// Issue #890 / #625: all ten little-endian words at
    /// <c>$9B:B80F..B822</c> match the pinned NTSC J/U v1.0 ROM
    /// and native bank-$9B listing. For palette index
    /// <c>k=0..9</c>, the pointer is exactly
    /// <c>$A120+$0020*max(0,k-1)</c>: indices zero and one
    /// share the first suitless row, while index nine selects
    /// <c>$A220</c>, also the suited table's final palette.
    /// The death caller reaches zero/one during flashing and
    /// zero/two through nine during explosion and copies all
    /// sixteen colors to suitless OBJ palette seven. Target
    /// colors remain separate authored data.
    ///
    /// Issue #897 / #625: all 128 suitless Samus BGR555 words in
    /// eight death rows at <c>$9B:A120..A21F</c> match the pinned
    /// ROM and native bank-$9B listing. For frame <c>f=0..7</c>,
    /// each five-bit component <c>b</c> from the authored first
    /// row at <c>$A120</c> becomes exactly
    /// <c>floor((b*(8-f)+31*f)/8)</c>. Direct comparison found
    /// zero mismatches; color five stays <c>$7FFF</c> in every
    /// frame. Pointer indices zero and one both select frame
    /// zero, and indices two through eight select frames one
    /// through seven. The death setup also directly copies
    /// frame zero to suitless OBJ palette seven. The base row
    /// remains authored presentation data.
    ///
    /// Issue #899 / #625: the final shared palette at
    /// <c>$9B:A220..A23F</c> contains exactly sixteen
    /// <c>$77BD</c> BGR555 words in the pinned ROM/native listing,
    /// a uniform grayscale level of 29 on each five-bit channel.
    /// Palette index nine in all three suited pointer families
    /// and the suitless list aliases this same physical row.
    /// Explosion record eight selects index nine with its
    /// eighty-call duration before the terminal full-screen
    /// whiteout; no separate final colors are authored by suit.
    /// </remarks>
    internal const int SuitlessPointers = 0x9bb80f;

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
    internal const int BubblePointers = 0x91dc28;

    /// <summary>Suit-indexed pointers to charged-beam palette instruction lists.</summary>
    internal const int BeamChargePointers = 0x91d7d5;

    /// <summary>Suit-indexed pointers to pseudo-Screw palette instruction lists.</summary>
    internal const int PseudoScrewPointers = 0x91d7ff;

    /// <summary>Hyper Beam shot palette pointers indexed by flash frame.</summary>
    internal const int HyperBeamShotPointers = 0x91d829;

    /// <summary>Beam-combination-indexed character-data pointers.</summary>
    internal const int TilePointers = 0x90c3b1;

    /// <summary>Beam-combination-indexed palette pointers at <c>$90:C3C9..C3E0</c>.</summary>
    /// <remarks>
    /// The twelve stock little-endian words select one of five bank-$90
    /// sixteen-color palettes. For combination index <c>i = 0..11</c>,
    /// Ice (<c>i &amp; 2</c>) selects <c>$C401</c>; otherwise Plasma
    /// (<c>i &amp; 8</c>) selects <c>$C441</c>; otherwise Wave
    /// (<c>i &amp; 1</c>) selects <c>$C421</c>; otherwise Spazer
    /// (<c>i &amp; 4</c>) selects <c>$C461</c>; otherwise Power selects
    /// <c>$C3E1</c>. All twelve match the pinned NTSC J/U v1.0 ROM and
    /// native listing. The native caller masks equipped beams with
    /// <c>$0FFF</c> and doubles the index; indices beyond eleven read
    /// adjacent physical data, including the Chainsaw path, rather than
    /// extending this rule. Keep the words available to ROM-backed
    /// consumers and the intentional adjacent-data overread. Selected ordinary
    /// beam colors are supplied by <see cref="SuperMetroid.Core.Samus.Artwork.BeamPaletteCatalog"/>.
    /// Investigation: #625 / #900.
    /// </remarks>
    internal const int PalettePointers = 0x90c3c9;

    /// <summary>$90:DD05: the movement-type weapon-admission handler pointer table.</summary>
    internal const int HudMovementHandlers = 0x90dd05;

    /// <summary>$90:DDAA: the posture-transition admission flags; zero enters the standard handler.</summary>
    internal const int HudTransitionFlags = 0x90ddaa;

    /// <summary>Bank $84: the bank of the item draw instruction pointers the X-ray item overlay follows.</summary>
    internal const int XrayItemDrawBank = 0x840000;

    /// <summary>Bank $8F: the bank of the room-specific X-ray overlay records, four bytes per record.</summary>
    internal const int XrayRoomOverlayBank = 0x8f0000;
}
