using SuperMetroid.Core.Fx.PaletteFx;
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
        /// <summary>The colours in each of Samus's colour sets: her suit's, her beams' and every palette her effects show.</summary>
        public const int ColoursPerRow = 16;
    }

    /// <summary>Shared visor colors used by room animation and the X-ray Scope.</summary>
    public static class Visor
    {
        /// <summary>Timer reloaded after one visor color is copied.</summary>
        public const int FrameDelay = 5;
        /// <summary>The first visor shade of the three-shade steady visor cycle; the shades before it widen the X-ray beam.</summary>
        public const int CycleFirstShade = 3;
        /// <summary>The visor shade count, which ends the steady visor cycle.</summary>
        public const int CycleEndShade = 6;
    }

    /// <summary>The suit-and-phase lists the special full-body palette handlers select their cycle palettes from.</summary>
    public static class FullBodyCycles
    {
        /// <summary>Number of full-body Hyper Beam palettes.</summary>
        public const int HyperBeamPaletteCount = 10;
        /// <summary>The steps in each suit's Screw Attack cycle list; the cycle wraps to its first step after the last.</summary>
        public const int ScrewAttackSteps = 6;
        /// <summary>The steps in each suit's active Speed Booster cycle list.</summary>
        public const int SpeedBoosterSteps = 4;
        /// <summary>The Speed Booster step the cycle holds once reached: its list's last.</summary>
        public const int SpeedBoosterLastStep = SpeedBoosterSteps - 1;
        /// <summary>The steps in each suit's stored-shine cycle list.</summary>
        public const int StoredShineSteps = 6;
        /// <summary>The steps in each suit's active-shinespark cycle list.</summary>
        public const int ActiveShinesparkSteps = 4;
        /// <summary>The step count a ping-pong cycle mirrors across: step <c>s</c> past the middle shows shade <c>PingPongSpan - s</c>.</summary>
        public const int PingPongSpan = 6;
        /// <summary>The Speed Booster shade the Metroid-attachment flash alternates with the normal suit: the family's brightest.</summary>
        public const int SpeedBoostFlashShade = 3;
        /// <summary>
        /// The Speed Booster step, inherited from a Screw Attack cycle, at which Gravity's read runs past the lists into
        /// the unconnected expansion window.
        /// </summary>
        public const int GravityExpansionOverrunStep = 4;
        /// <summary>
        /// The Speed Booster step, inherited from a Screw Attack cycle, at which Gravity's read runs past the lists into
        /// the grapple handler's code.
        /// </summary>
        public const int GravityGrappleCodeOverrunStep = 5;
    }

    /// <summary>Bank-$8D palette object spawned with the Hyper Beam.</summary>
    public static class HyperBeamFx
    {
        /// <summary>Expected no-op setup callback in the object definition.</summary>
        public const PaletteFxSetup SetupCallback = PaletteFxSetup.Null;
        /// <summary>The frames of the Hyper Beam's own colour cycle: red, yellow, green, blue and magenta, each followed by its blend into the next.</summary>
        public const int CycleFrameCount = 10;
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

    /// <summary>The light beam's colour endpoints in the shared Varia/Gravity pickup sequence, in five-bit channels.</summary>
    public static class SuitPickup
    {
        /// <summary>Initial red shared by both transformations.</summary>
        public const int InitialRed = 16;
        /// <summary>Initial/terminal Varia green.</summary>
        public const int VariaGreen = 16;
        /// <summary>Initial/terminal Gravity green.</summary>
        public const int GravityGreen = 9;
        /// <summary>Initial Varia blue.</summary>
        public const int VariaBlue = 0;
        /// <summary>Initial/terminal Gravity blue.</summary>
        public const int GravityBlue = 16;
        /// <summary>White-ramp red endpoint.</summary>
        public const int WhiteRed = 31;
        /// <summary>White-ramp green endpoint.</summary>
        public const int WhiteGreen = 31;
        /// <summary>White-ramp blue endpoint.</summary>
        public const int WhiteBlue = 31;
        /// <summary>Varia orange-ramp green endpoint.</summary>
        public const int VariaOrangeGreen = 13;
        /// <summary>Varia orange-ramp blue endpoint.</summary>
        public const int VariaOrangeBlue = 3;
        /// <summary>Final reset red.</summary>
        public const int ResetRed = 0;
        /// <summary>Final reset green.</summary>
        public const int ResetGreen = 0;
        /// <summary>Final reset blue.</summary>
        public const int ResetBlue = 0;
    }

    /// <summary>Fixed-color tables used by the Power Bomb and Crystal Flash explosion effects.</summary>
    public static class PowerBomb
    {
        /// <summary>The five-bit channel in each stored fixed-colour byte.</summary>
        public const int ComponentMask = 0x1f;
        /// <summary>Number of pre-explosion fixed-color triplets.</summary>
        public const int PreExplosionColorCount = 16;
        /// <summary>Number of explosion fixed-color triplets addressed by radius.</summary>
        public const int ExplosionColorCount = 32;
    }
}
