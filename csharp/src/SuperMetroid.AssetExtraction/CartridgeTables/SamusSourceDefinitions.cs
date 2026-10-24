using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps each table Samus's codecs read (#1411): the import pipeline's private knowledge of the
/// cartridge, decoded once, as typed source addresses with their meaning named.
/// </summary>
[CpuBusBoundary("catalog: Samus cartridge sources")]
internal static class SamusSourceDefinitions
{
    /// <summary>$90:C9D9, where the arm cannon drawing window starts in bank $90: its first pose descriptor.</summary>
    internal const int ArmCannonDrawingData = 0xc9d9;

    /// <summary>$90:CC21, CostOfSBAsInPowerBombs: twelve word costs by beam index, the arm cannon drawing window's last bytes.</summary>
    internal static readonly SnesAddress SpecialBeamAttackCosts = new(0x90, 0xcc21);

    /// <summary>Bank <c>$91</c>, which holds the pose and projectile-driven suit-palette lists.</summary>
    internal static readonly SnesAddress PoseBank = new(0x91, 0x0000);

    /// <summary>Bank <c>$93</c>, which holds the projectile definitions and animation lists.</summary>
    internal static readonly SnesAddress ProjectileDataBank = new(0x93, 0x0000);

    /// <summary>$91:B20A, where Samus's pose delay programs start, after the pose pointer table.</summary>
    internal static readonly SnesAddress PoseDelayPrograms = new(0x91, 0xb20a);

    /// <summary>$91:B5D1, where the running cadences start: the word naming the ordinary stream.</summary>
    internal static readonly SnesAddress RunningCadences = new(0x91, 0xb5d1);

    /// <summary>$90:C487, where the charge flare's delay programs start.</summary>
    internal static readonly SnesAddress ChargeFlarePrograms = new(0x90, 0xc487);

    /// <summary>$90:C204, where the firing tables <see cref="SuperMetroid.Core.Samus.Projectiles.SamusFiringTableDefinitions"/> lays out start.</summary>
    internal static readonly SnesAddress FiringTables = new(0x90, 0xc204);

    /// <summary>$90:C1A8: the charge flare's DefaultX offsets, by direction.</summary>
    internal static readonly SnesAddress ChargeFlareDefaultXOffsets = new(0x90, 0xc1a8);

    /// <summary>$90:C1C2: the charge flare's DefaultY offsets, by direction.</summary>
    internal static readonly SnesAddress ChargeFlareDefaultYOffsets = new(0x90, 0xc1c2);

    /// <summary>$90:C1DC: the charge flare's RunningX offsets, by direction.</summary>
    internal static readonly SnesAddress ChargeFlareRunningXOffsets = new(0x90, 0xc1dc);

    /// <summary>$90:C1F0: the charge flare's RunningY offsets, by direction.</summary>
    internal static readonly SnesAddress ChargeFlareRunningYOffsets = new(0x90, 0xc1f0);

    /// <summary>$9A:D900 contains OBJ tiles $38..$3F: ice and wave trail frames.</summary>
    internal static readonly SnesAddress ProjectileTrailIceWaveTiles = new(0x9a, 0xd900);

    /// <summary>$9A:DB00 contains OBJ tiles $48..$4B: missile and Super Missile trail frames.</summary>
    internal static readonly SnesAddress ProjectileTrailMissileTiles = new(0x9a, 0xdb00);

    /// <summary>$94:B13D: immediate endpoint attributes in DrawGrappleBeamEnd_NotConnected.</summary>
    internal static readonly SnesAddress GrappleEndpointAttributes = new(0x94, 0xb13d);

    /// <summary>Bank <c>$9B</c>, which owns Samus BGR555 color data.</summary>
    internal static readonly SnesAddress PaletteBank = new(0x9b, 0x0000);

    /// <summary><c>$9B:A380</c>, the sixteen-color hurt-flash palette.</summary>
    /// <remarks>
    /// Issue #863 / #625: the pinned NTSC J/U v1.0 ROM's sixteen BGR555
    /// words are copied unchanged to Samus OBJ CGRAM 192..207 on odd
    /// hurt-counter calls 1, 3, and 5. Color zero is <c>$0000</c>. For
    /// each color index <c>c=1..15</c> and each five-bit channel <c>q</c>,
    /// <c>q(hurt[c]) = floor((2*q(intro[c]) + 5*31)/7)</c>, where
    /// <c>intro</c> is the adjacent palette at <c>$9B:A3A0</c>.
    /// This bounded 5/7 blend toward white matches all sixteen ROM words
    /// exactly. The corresponding installed color payload still requires
    /// conversion; this address member grants no retention disposition.
    /// </remarks>
    internal static readonly SnesAddress HurtFlashColors = new(0x9b, 0xa380);

    /// <summary><c>$9B:A3A0</c>, the sixteen-color cinematic Samus palette.</summary>
    /// <remarks>
    /// Issue #864 / #625: all sixteen BGR555 words match the pinned NTSC
    /// J/U v1.0 ROM and native bank-$9B listing. Color zero is
    /// <c>$3800</c>; the remaining positions select seven authored grey
    /// shades with red = green = <c>4,8,13,18,22,27,31</c>. Blue equals
    /// red minus two except at the darkest shade, where all channels are
    /// four. Slot-to-shade selection and the base shade payload still require
    /// conversion or concrete impossible/nonsense evidence. The hurt palette
    /// (#863) is an exact forward blend from these colors; that relation does
    /// not by itself resolve the independent intro palette.
    /// Production copies indices 0..15 unchanged to Samus OBJ CGRAM
    /// 192..207 on cinematic hurt-counter calls 2, 4, and 6.
    /// </remarks>
    internal static readonly SnesAddress IntroHurtFlashColors = new(0x9b, 0xa3a0);

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
    internal static readonly SnesAddress VisorColors = new(0x9b, 0xa3c0);

    /// <summary><c>$9B:B823</c>, nine interleaved timer/palette-index records.</summary>
    /// <remarks>
    /// Issue #891 / #625: the nine timer bytes at even offsets
    /// match the pinned NTSC J/U v1.0 ROM and native bank-$9B
    /// listing: <c>21,6,3,4,5,5,6,6,80</c> for explosion index
    /// <c>0..8</c>. Issue1165 traces these countdowns to the nine fixed
    /// right/left drawings selected at $92:EDBE, all at the captured position.
    /// The 21-call entry delay, seven individual drawing dwell times, and
    /// 80-call final hold specify this cinematic's chosen timing. No motion,
    /// distance or measured-brightness quantity determines those durations;
    /// inventing such a relationship would replace the intended drawing rhythm.
    /// This is the concrete animation/nonsense case documented on
    /// SamusDeathExplosionTimingDefinitions, not an exemption for authored data
    /// in general. Only the nine timer bytes receive that disposition.
    /// The caller decrements
    /// the active timer before advancing, loads the next duration
    /// only for indices below nine, and terminates without a tenth
    /// explosion frame. Odd-offset palette-selector bytes are a
    /// separate mapping, calculated by the death artwork catalog under1165
    /// with independently supplied asset overrides.
    ///
    /// Issue #892 / #625: the nine odd-offset selector bytes
    /// match the pinned ROM/native listing and are exactly
    /// <c>0</c> for explosion index zero and <c>i+1</c> for
    /// <c>i=1..8</c>, yielding <c>0,2,3,4,5,6,7,8,9</c>.
    /// The caller installs palette zero directly at explosion
    /// entry, then reads byte offset <c>2*i+1</c> only for
    /// <c>i=1..8</c>. Each result indexes both bounded ten-word
    /// suited and suitless pointer lists. The selector is a byte
    /// immediately following its timer, not an aligned word read.
    /// </remarks>
    internal static readonly SnesAddress DeathExplosionTimingAndPaletteIndices = new(0x9b, 0xb823);

    /// <summary><c>$9B:B835</c>, 22 grayscale whiteout shades ending at white.</summary>
    /// <remarks>
    /// Issue #893 / #625: all 22 little-endian words at
    /// <c>$9B:B835..B860</c> match the pinned NTSC J/U v1.0 ROM
    /// and native bank-$9B listing. Each BGR555 word is
    /// <c>$0421*level(i)</c> for index <c>i=0..21</c>, where
    /// <c>level=2*i+1</c> for <c>i=0..6</c>, <c>16</c> at
    /// <c>i=7</c>, and <c>i+10</c> for <c>i=8..21</c>.
    /// This yields grayscale levels
    /// <c>1,3,5,7,9,11,13,16,18..31</c> exactly, including
    /// the skipped levels. The caller suppresses whiteout for
    /// explosion index zero, reads shade indices 0..20 during
    /// later frames, then explicitly reads index 21 to finish
    /// at <c>$7FFF</c>; it excludes both Samus OBJ palettes.
    /// </remarks>
    internal static readonly SnesAddress DeathWhiteoutShades = new(0x9b, 0xb835);

    /// <summary><c>$91:DC00</c>, ten body-palette pointer/timer records.</summary>
    /// <remarks>
    /// The stock bank-$9B pointers at four-byte record offsets are exactly
    /// <c>$96C0 + $20 * (i &lt;= 5 ? i : 10 - i)</c> for record index
    /// <c>i = 0..9</c>; all ten match the pinned NTSC J/U v1.0 ROM.
    /// Keep the pointer words as live presentation data so installed
    /// palette content can select its authored colours. The adjacent
    /// duration words are compiled separately. Investigation: #625 / #669.
    /// </remarks>
    internal static readonly SnesAddress CrystalFlashBodyRecords = new(0x91, 0xdc00);

    /// <summary><c>$88:9079</c>, sixteen RGB triplets for the pre-explosion.</summary>
    /// <remarks>
    /// For radius-derived index <c>i = 0..15</c>, stock red and green
    /// are both 16 at zero, <c>2*i+2</c> at 1..12, and
    /// <c>50-2*i</c> at 13..15. Blue is 16 at zero, the same as red
    /// at 1..5, 10 at 6, 8 at 7..11, 10 at 12, and
    /// <c>34-2*i</c> at 13..15. All 48 five-bit payload bytes match
    /// the pinned NTSC J/U v1.0 ROM and native listing. The caller
    /// indexes by <c>(radius &gt;&gt; 11) &amp; $0F</c>, then reads R/G/B
    /// in order; the following <c>$88:90A9</c> bytes form another table.
    /// Target-color ownership and conversion are reviewed separately from this address. Investigation: #625 / #904.
    /// </remarks>
    internal static readonly SnesAddress PowerBombPreExplosionColors = new(0x88, 0x9079);

    /// <summary><c>$88:8D85</c>, radius-indexed RGB triplets for the explosion.</summary>
    /// <remarks>
    /// The 32 stock R/G/B triplets occupy <c>$88:8D85..8DE4</c> and
    /// match all 96 bytes in the pinned NTSC J/U v1.0 ROM and native
    /// listing. For yellow-phase index <c>i = 0..13</c>, red and green
    /// are <c>min(14+i,26)</c> and blue is <c>max(10-i,0)</c>.
    /// White-crest indices 14..18 use equal channels 26+(i-14)/2;
    /// indices 19..20 use 46-i. The installed color catalog calculates
    /// both phases on demand and preserves independent supplied edits.
    /// Indices 21..31 still require their own conversion or concrete
    /// impossible/nonsense evidence; they have no retention disposition.
    /// Ordinary Power Bomb and Crystal Flash explosion callers derive
    /// <c>i = (radius &gt;&gt; 11) &amp; $1F</c> from a 16-bit radius and
    /// read the three components in order. The next word at
    /// <c>$88:8DE5</c> is radius-speed data. Investigation: #625 / #905.
    /// </remarks>
    internal static readonly SnesAddress PowerBombExplosionColors = new(0x88, 0x8d85);

    /// <summary>Each fixed-color record stores red, green, and blue bytes.</summary>
    internal const int PowerBombBytesPerColor = 3;


    /// <summary>Bank $9A, which holds the decompressed-source characters the arm cannon's cover copies.</summary>
    internal static readonly SnesAddress CharacterDataBank = new(0x9a, 0x0000);

    /// <summary>Non-running flare X origins.</summary>
    internal static readonly SnesAddress GrappleDefaultFlareX = new(0x9b, 0xc14a);

    /// <summary>Non-running flare Y origins.</summary>
    internal static readonly SnesAddress GrappleDefaultFlareY = new(0x9b, 0xc15e);

    /// <summary>Running flare X origins.</summary>
    internal static readonly SnesAddress GrappleRunningFlareX = new(0x9b, 0xc19a);

    /// <summary>Running flare Y origins.</summary>
    internal static readonly SnesAddress GrappleRunningFlareY = new(0x9b, 0xc1ae);

    /// <summary>Animation frame selected by each high byte of swing angle.</summary>
    internal static readonly SnesAddress GrappleSwingFrameByAngle = new(0x9b, 0xc1c2);

    /// <summary><c>$9B:9400</c>, ordinary Power Suit palette.</summary>
    /// <remarks>
    /// Issue #860 / #625: sixteen authored BGR555 words in the pinned NTSC
    /// J/U v1.0 ROM, indexed only by color 0..15 and copied unchanged to
    /// Samus OBJ CGRAM 192..207. All sixteen match the normal row of the
    /// Power loading palette-FX program (#856) and the duplicate bank-$9B
    /// palettes at <c>$9C20</c> and <c>$9CA0</c>. Eleven color slots are
    /// shared with the normal Varia and Gravity palettes; slots
    /// 0, 2, 10, 11, 12 vary by suit. These deliberately selected colors
    /// remain a palette table rather than a computed channel sequence.
    /// </remarks>
    internal static readonly SnesAddress PowerSuitPalette = new(0x9b, 0x9400);

    /// <summary><c>$9B:9520</c>, ordinary Varia Suit palette.</summary>
    /// <remarks>
    /// Issue #861 / #625: sixteen authored BGR555 words in the pinned NTSC
    /// J/U v1.0 ROM, indexed only by color 0..15 and copied unchanged to
    /// Samus OBJ CGRAM 192..207. All sixteen match the normal row of the
    /// Varia loading palette-FX program (#857) and duplicate bank-$9B
    /// palettes at <c>$9920</c>, <c>$9D20</c>, and <c>$9DA0</c>. Eleven
    /// color slots are shared with normal Power and Gravity; slots
    /// 0, 2, 10, 11, 12 vary by suit. These authored choices remain a
    /// palette table rather than a computed channel sequence.
    /// </remarks>
    internal static readonly SnesAddress VariaSuitPalette = new(0x9b, 0x9520);

    /// <summary><c>$9B:9800</c>, ordinary Gravity Suit palette.</summary>
    /// <remarks>
    /// Issue #862 / #625: sixteen authored BGR555 words in the pinned NTSC
    /// J/U v1.0 ROM, indexed only by color 0..15 and copied unchanged to
    /// Samus OBJ CGRAM 192..207. All sixteen match the normal row of the
    /// Gravity loading palette-FX program (#858). The nearby speed-boost
    /// base <c>$9B:9F20</c> is not an alias: colors 0, 1, and 12 differ.
    /// Eleven color slots are shared with normal Power and Varia; slots
    /// 0, 2, 10, 11, 12 vary by suit. These authored choices remain a
    /// palette table rather than a computed channel sequence.
    /// </remarks>
    internal static readonly SnesAddress GravitySuitPalette = new(0x9b, 0x9800);

    /// <summary><c>$90:8D28</c>, packed landing-frame vertical offsets.</summary>
    internal static readonly SnesAddress LandingVerticalOffsets = new(0x90, 0x8d28);


    /// <summary><c>$90:8D80</c>, crouch/morph transition vertical offsets.</summary>
    internal static readonly SnesAddress PostureTransitionVerticalOffsets = new(0x90, 0x8d80);


    /// <summary><c>$90:8DEF</c>, drained-body vertical offsets.</summary>
    internal static readonly SnesAddress DrainedVerticalOffsets = new(0x90, 0x8def);


    /// <summary><c>$90:C791</c>, packed OBJ attributes for ten directions.</summary>
    internal static readonly SnesAddress ArmCannonSpriteAttributes = new(0x90, 0xc791);

    /// <summary>Bank $90, which holds Samus's physics constants, speed tables and effects metadata.</summary>
    internal static readonly SnesAddress MovementBank = new(0x90, 0x0000);

    /// <summary><c>$91:B629</c>: Samus's pose definitions, eight bytes per pose.</summary>
    internal static readonly SnesAddress PoseDefinitions = new(0x91, 0xb629);

    /// <summary>The bytes of one pose-definition record.</summary>
    internal const int PoseDefinitionByteCount = 8;

    /// <summary><c>$90:8C0F</c>: the retail type-one direct small-OBJ atmospheric attribute list, bank-local.</summary>
    internal const ushort TypeOneAtmosphericAttributes = 0x8c0f;

    /// <summary><c>$90:8C17</c>: the shared type-four/six/seven atmospheric attribute list, bank-local.</summary>
    internal const ushort SharedAtmosphericAttributes = 0x8c17;

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

    /// <summary>
    /// $9B:8000-$9B:93FF: Samus's death-explosion characters, in the order its spritemaps name them. The death
    /// sequence uploads them as five $400-byte pages listed at $9B:B7BF (sources) and $9B:B7C9 (destinations).
    /// </summary>
    internal static readonly SnesAddress DeathExplosionCharacters = new(0x9b, 0x8000);
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
    internal static readonly SnesAddress HyperBeamPalettes = new(0x9b, 0xa360);
    /// <summary>Byte distance between descending Hyper Beam frames.</summary>
    internal const int HyperBeamPaletteStride = 0x20;

    /// <summary>$91:0000: the bank of the X-ray revealed-block tables and their commands.</summary>
    internal static readonly SnesAddress XrayRevealTableBank = new(0x91, 0x0000);
    /// <summary>$91:D2D6 RevealedBlockTable_0: the X-ray reveal table keyed by the level word's block-type nibble.</summary>
    internal const ushort XrayRevealBlockTypeTable = 0xd2d6;

    /// <summary>$92:90ED: the first Samus spritemap; spritemap pointers below it are invalid.</summary>
    internal const int FirstSpritemap = 0x90ed;

    /// <summary>$92:808D: the spritemap pointer of each pose-half table entry.</summary>
    internal static readonly SnesAddress SpritemapPointers = new(0x92, 0x808d);

    /// <summary>$92:9263: each pose's first upper-half table entry.</summary>
    internal static readonly SnesAddress SpritemapTopBases = new(0x92, 0x9263);

    /// <summary>$92:945D: each pose's first lower-half table entry.</summary>
    internal static readonly SnesAddress SpritemapBottomBases = new(0x92, 0x945d);

    /// <summary>$92:CBEE: the first seven-byte body DMA definition record; the rest follow it.</summary>
    internal const int FirstBodyDefinition = 0xcbee;

    /// <summary>Bytes in one body DMA definition record.</summary>
    internal const int BodyDefinitionBytes = 7;

    /// <summary>$92:DB48: the first four-byte body frame selector; the rest follow it.</summary>
    internal static readonly SnesAddress BodyFrames = new(0x92, 0xdb48);

    /// <summary>The bank-$92 address of the first body frame selector.</summary>
    internal const int FirstBodyFrame = 0xdb48;

    /// <summary>$84:839D, eight pointers to frame-zero item draw instructions.</summary>
    internal static readonly SnesAddress ItemDrawPointers = new(0x84, 0x839d);

    /// <summary><c>$90:C7DF</c>, pose-indexed drawing-data pointers.</summary>
    internal static readonly SnesAddress PoseDrawingDataPointers = new(0x90, 0xc7df);

    /// <summary><c>$90:C7A5</c>, direction-indexed tile-list pointers.</summary>
    internal static readonly SnesAddress TileListPointers = new(0x90, 0xc7a5);

    /// <summary><c>$92:D94E</c>, animation-record-list pointer for each pose.</summary>
    internal static readonly SnesAddress AnimationDefinitionListPointers = new(0x92, 0xd94e);

    /// <summary><c>$92:D91E</c>, top-definition-list pointer for each graphics set.</summary>
    internal static readonly SnesAddress TopDefinitionListPointers = new(0x92, 0xd91e);

    /// <summary><c>$92:D938</c>, bottom-definition-list pointer for each graphics set.</summary>
    internal static readonly SnesAddress BottomDefinitionListPointers = new(0x92, 0xd938);

    /// <summary>Direct OAM attribute-list pointers indexed by atmospheric-effect type.</summary>
    internal static readonly SnesAddress AtmosphericSpriteAttributeListPointers = new(0x90, 0x8bff);

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
    internal static readonly SnesAddress SuitPointers = new(0x9b, 0xb7d3);

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
    internal static readonly SnesAddress SuitlessPointers = new(0x9b, 0xb80f);

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
    internal static readonly SnesAddress BubblePointers = new(0x91, 0xdc28);

    /// <summary>Suit-indexed pointers to charged-beam palette instruction lists.</summary>
    internal static readonly SnesAddress BeamChargePointers = new(0x91, 0xd7d5);

    /// <summary>Suit-indexed pointers to pseudo-Screw palette instruction lists.</summary>
    internal static readonly SnesAddress PseudoScrewPointers = new(0x91, 0xd7ff);

    /// <summary>Hyper Beam shot palette pointers indexed by flash frame.</summary>
    internal static readonly SnesAddress HyperBeamShotPointers = new(0x91, 0xd829);

    /// <summary>Beam-combination-indexed character-data pointers.</summary>
    internal static readonly SnesAddress TilePointers = new(0x90, 0xc3b1);

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
    internal static readonly SnesAddress PalettePointers = new(0x90, 0xc3c9);

    /// <summary>Bank $84: the bank of the item draw instruction pointers the X-ray item overlay follows.</summary>
    internal static readonly SnesAddress XrayItemDrawBank = new(0x84, 0x0000);

    /// <summary>Bank $8F: the bank of the room-specific X-ray overlay records, four bytes per record.</summary>
    internal static readonly SnesAddress XrayRoomOverlayBank = new(0x8f, 0x0000);
}
