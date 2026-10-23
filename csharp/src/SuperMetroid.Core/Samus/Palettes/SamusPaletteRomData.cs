using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Abilities;
namespace SuperMetroid.Core.Samus.Palettes;

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
        /// <summary>Bank <c>$9B</c>, which owns Samus BGR555 color data.</summary>
        public const int Palette = 0x9b0000;
    }

    /// <summary>Shared full-body OBJ palette layout and suit selection table.</summary>
    public static class Common
    {
        /// <summary>First CGRAM color of Samus OBJ palette four.</summary>
        public const int SamusObjPaletteStart = 192;
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
        /// exactly. The corresponding installed color payload still requires
        /// conversion; this address member grants no retention disposition.
        /// </remarks>
        public const int Colors = 0x9ba380;
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
        /// <summary>Timer reloaded after one visor color is copied.</summary>
        public const ushort FrameDelay = 5;
        /// <summary>First byte offset in the three-color steady visor cycle.</summary>
        public const byte CycleFirstByteOffset = 6;
        /// <summary>Exclusive byte offset ending the three-color steady visor cycle.</summary>
        public const byte CycleEndByteOffset = 12;
    }

    /// <summary>The suit-and-phase lists the special full-body palette handlers select their cycle palettes from.</summary>
    public static class FullBodyCycles
    {
        /// <summary>$91:D998: the cycle palette the special Super-palette flash alternates with normal suit colors, the Speed Booster family's last shade.</summary>
        /// <param name="suitByteOffset">Even byte offset 0 (Power), 2 (Varia) or 4 (Gravity) into the native suit table.</param>
        internal static SamusFullBodyCyclePalette SpeedBoostPalette(ushort suitByteOffset) =>
            new(SamusFullBodyCycleFamily.SpeedBooster, Suit(suitByteOffset), 3);

        /// <summary>
        /// Reads the Screw Attack palette anywhere in the compiled three-suit list allocation, including a restored phase
        /// that runs on into the next suit's list.
        /// </summary>
        internal static SamusFullBodyCyclePalette ReadScrewAttackPalette(
            ushort suitByteOffset, ushort phaseByteOffset) =>
            ReadCompiledList(suitByteOffset, phaseByteOffset, phaseCount: 6,
                SamusFullBodyCycleFamily.ScrewAttack, PaletteCycleShape.PingPong);

        /// <summary>
        /// Reads the active Speed Booster palette anywhere in the compiled three-suit list allocation. Gravity phases
        /// eight and ten, inherited from the shared Screw Attack phase word, instead read the instruction bytes after
        /// the lists.
        /// </summary>
        internal static SpeedBoosterPaletteSource ReadActiveSpeedBoosterPalette(
            ushort suitByteOffset, ushort phaseByteOffset) =>
            new SpeedBoosterPaletteOffsets(suitByteOffset, phaseByteOffset) switch
            {
                (4, 8) => SpeedBoosterPaletteSource.Of(SpeedBoosterPaletteOverrun.Expansion),
                (4, 10) => SpeedBoosterPaletteSource.Of(SpeedBoosterPaletteOverrun.GrappleCode),
                _ => SpeedBoosterPaletteSource.Of(ReadCompiledList(suitByteOffset, phaseByteOffset, phaseCount: 4,
                    SamusFullBodyCycleFamily.SpeedBooster, PaletteCycleShape.Linear)),
            };

        /// <summary>
        /// Resolves the bounded $91:DB10/$91:DB16 stored-shine suit and phase lists to a cycle palette.
        /// </summary>
        public static bool TryStoredShinePalette(
            ushort suitByteOffset, ushort phaseByteOffset, out SamusFullBodyCyclePalette palette) =>
            TryPalette(suitByteOffset, phaseByteOffset, 10, SamusFullBodyCycleFamily.StoredShine,
                PaletteCycleShape.PingPong, out palette);

        /// <summary>
        /// Resolves the bounded $91:DB75/$91:DB7B active-shinespark suit and phase lists to a cycle palette.
        /// </summary>
        public static bool TryActiveShinesparkPalette(
            ushort suitByteOffset, ushort phaseByteOffset, out SamusFullBodyCyclePalette palette) =>
            TryPalette(suitByteOffset, phaseByteOffset, 6, SamusFullBodyCycleFamily.ActiveShinespark,
                PaletteCycleShape.Linear, out palette);

        /// <summary>
        /// Resolves a valid suit/phase pair to a family's palette, mirroring later phases back toward the first shade
        /// for ping-pong cycles.
        /// </summary>
        private static bool TryPalette(
            ushort suitByteOffset,
            ushort phaseByteOffset,
            ushort lastPhaseByteOffset,
            SamusFullBodyCycleFamily family,
            PaletteCycleShape shape,
            out SamusFullBodyCyclePalette palette)
        {
            if (suitByteOffset > 4 || (suitByteOffset & 1) != 0 ||
                phaseByteOffset > lastPhaseByteOffset || (phaseByteOffset & 1) != 0)
            {
                palette = default;
                return false;
            }

            palette = new(family, suitByteOffset / 2, Shade(phaseByteOffset / 2, shape));
            return true;
        }

        /// <summary>
        /// Reads the selected entry of a family's contiguous three-suit list allocation. The native two-level lookup
        /// adds the full 16-bit phase word to the selected suit's list, so a phase can run on into the next suit's
        /// list; a read that leaves the allocation, or straddles two entries, selects no cycle palette.
        /// </summary>
        private static SamusFullBodyCyclePalette ReadCompiledList(
            ushort suitByteOffset,
            ushort phaseByteOffset,
            int phaseCount,
            SamusFullBodyCycleFamily family,
            PaletteCycleShape shape)
        {
            if (suitByteOffset > 4 || (suitByteOffset & 1) != 0)
                throw new ArgumentOutOfRangeException(nameof(suitByteOffset));

            int byteOffset = suitByteOffset / 2 * phaseCount * sizeof(ushort) + phaseByteOffset;
            if (byteOffset + 1 >= 3 * phaseCount * sizeof(ushort))
                throw new InvalidDataException(
                    $"{family} palette list read at byte {byteOffset} is outside the compiled list allocation.");
            if ((byteOffset & 1) != 0)
                throw new InvalidDataException(
                    $"{family} palette list read at odd byte {byteOffset} straddles two entries and selects no cycle palette.");

            int entry = byteOffset / sizeof(ushort);
            return new(family, entry / phaseCount, Shade(entry % phaseCount, shape));
        }

        /// <summary>The shade a cycle phase shows: linear cycles step through their shades, ping-pong cycles return.</summary>
        private static int Shade(int phase, PaletteCycleShape shape) =>
            shape == PaletteCycleShape.PingPong ? Math.Min(phase, 6 - phase) : phase;

        /// <summary>The suit an even native suit-table byte offset selects.</summary>
        private static int Suit(ushort suitByteOffset) => suitByteOffset is 0 or 2 or 4
            ? suitByteOffset / 2
            : throw new ArgumentOutOfRangeException(nameof(suitByteOffset));

        /// <summary>Number of full-body Hyper Beam palettes.</summary>
        public const int HyperBeamPaletteCount = 10;
    }

    /// <summary>Bank-$8D palette object spawned with the Hyper Beam.</summary>
    public static class HyperBeamFx
    {
        /// <summary>Expected no-op setup callback in the object definition.</summary>
        public const PaletteFxSetup SetupCallback = PaletteFxSetup.Null;
    }

    /// <summary>Palette tables consumed by the fatal-damage sequence.</summary>
    public static class Death
    {
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
        public const int ExplosionTimingAndPaletteIndices = 0x9bb823;
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
        public const int WhiteoutShades = 0x9bb835;
        /// <summary>Number of suited and suitless explosion palette variants.</summary>
        public const int PaletteCount = 10;
        /// <summary>Number of shades in the inclusive whiteout ramp.</summary>
        public const int WhiteoutShadeCount = 22;
    }

    /// <summary>Two independently timed palette streams used by Crystal Flash.</summary>
    /// <remarks>
    /// The stock bank-$9B body colors are six ten-word subrows at
    /// <c>$96C0 + $20*r</c>, <c>r = 0..5</c>. Color zero is always
    /// <c>$3800</c>; colors 1..9 in each subrow are the same grayscale word
    /// <c>$0421 * (16 + floor(11*min(r,6-r)/3))</c>. This yields channel
    /// levels 16, 19, 23, 27, 23, 19 and matches all 60 words in the
    /// pinned NTSC J/U v1.0 ROM. The body pointer program selects rows
    /// 0,1,2,3,4,5,4,3,2,1 and copies only ten colors per transition;
    /// the following six bubble colors have a separate cursor and timer.
    /// Keep both streams live for ROM-backed presentation. Body-color
    /// investigation: #625 / #902.
    /// The six bubble colors per row start at <c>$96D4 + $20*r</c>.
    /// For bubble color <c>c = 0..5</c>, let
    /// <c>k = (c-r+6)%6</c> and <c>q = k + (k &gt;= 3 ? 1 : 0)</c>.
    /// The stock BGR555 word is <c>$7FFF - $0420*q</c>, except
    /// <c>r = 5, c = 0</c> is <c>$7FFF</c> rather than <c>$7BDF</c>.
    /// All 36 words, including that authored exception, match the pinned
    /// ROM and native listing. The bubble cursor visits rows 0..5 and wraps
    /// independently of the body records. Bubble-color investigation:
    /// #625 / #903.
    /// </remarks>
    public static class CrystalFlash
    {
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
        public const int PreExplosionColors = 0x889079;
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
