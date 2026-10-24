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
public static class SamusPaletteDefinitions
{

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
    }

    /// <summary>Shared visor colors used by room animation and the X-ray Scope.</summary>
    public static class Visor
    {
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
        /// <summary>Five-bit component payload mask in a COLDATA byte.</summary>
        public const byte ComponentMask = 0x1f;
        /// <summary>Number of pre-explosion fixed-color triplets.</summary>
        public const int PreExplosionColorCount = 16;
        /// <summary>Number of explosion fixed-color triplets addressed by radius.</summary>
        public const int ExplosionColorCount = 32;
    }
}
