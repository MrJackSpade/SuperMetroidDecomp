using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Colours;

/// <summary>Immutable game data shared by Samus shades.</summary>
/// <remarks>
/// Shade state machines retain their mutable timers and phase transitions. This catalog
/// names the shade colours, colour-animation steps, the colours each shade changes, and endpoint colours those
/// machines consume, so no raw number masquerades as gameplay behavior.
/// </remarks>
public static class SamusShadeDefinitions
{
    /// <summary>
    /// The visor's two shade sequences (#2285): the X-ray widening glow, which brightens the visor while the beam widens,
    /// and the steady visor cycle, which the full X-ray beam and rooms that animate the visor show in turn.
    /// </summary>
    public static class Visor
    {
        /// <summary>The X-ray widening glow's shades, shown in order; the last is held.</summary>
        public const int XrayWideningShades = 3;
        /// <summary>The steady visor cycle's shades, shown in order and repeated from the first.</summary>
        public const int CycleShades = 3;
    }

    /// <summary>The charge glows Samus's suit pulses through while a beam is charged or the pseudo Screw Attack holds.</summary>
    public static class Charge
    {
        /// <summary>The shades each charge pulse shows in order, in every suit, before it starts again.</summary>
        public const int PulseShades = 6;
    }

    /// <summary>The Hyper Beam's own colour cycle, which its beams show.</summary>
    public static class HyperBeamCycle
    {
        /// <summary>The cycle's beam colourings: red, yellow, green, blue and magenta, each followed by its blend into the next.</summary>
        public const int Frames = 10;
    }

    /// <summary>The cycle shades the special full-body suit colourings show, by suit and phase.</summary>
    public static class FullBodyCycles
    {
        /// <summary>Number of full-body Hyper Beam shades.</summary>
        public const int HyperBeamShadeCount = 10;
        /// <summary>The shades each full-body cycle family authors in every suit: Speed Booster, Screw Attack and the shines.</summary>
        public const int FamilyShades = 4;
        /// <summary>The shading updates before the active Speed Booster cycle's first step when the boost starts: it starts on the next.</summary>
        public const int SpeedBoosterFirstStepWait = 1;
        /// <summary>The Speed Booster shade the Metroid-attachment flash alternates with the normal suit: the family's brightest.</summary>
        public const int SpeedBoostFlashShade = 3;
    }

    /// <summary>The shades the fatal-damage sequence shows.</summary>
    public static class Death
    {
        /// <summary>Number of suited and suitless explosion shade variants.</summary>
        public const int ExplosionShadeCount = 10;
        /// <summary>Number of shades in the inclusive whiteout ramp.</summary>
        public const int WhiteoutShadeCount = 22;
    }

    /// <summary>Two independently timed colour cycles used by Crystal Flash.</summary>
    /// <remarks>
    /// Samus's body brightens through its grey shades and dims back again (<see cref="BodyStepCount"/>); the bubble around her cycles
    /// its own shades (<see cref="BubbleShadeCount"/>) on a separate timer, wrapping independently of the body. Both cycles stay live while Crystal
    /// Flash runs. The shades themselves are installed colours (#625, #902, #903).
    /// </remarks>
    public static class CrystalFlash
    {
        /// <summary>Number of Crystal Flash body steps.</summary>
        public const int BodyStepCount = 10;
        /// <summary>Number of bubble shades.</summary>
        public const int BubbleShadeCount = 6;
    }

    /// <summary>
    /// The light beam's colours in the shared Varia/Gravity pickup sequence, on the game's fade scale: the beam starts in
    /// its suit's colour, each channel rises toward white while the beam widens, and each falls back toward the suit's
    /// settling colour while the beam shrinks, every channel at its own step until it reaches its target.
    /// </summary>
    public static class SuitPickup
    {
        /// <summary>The Varia beam's colour as it appears.</summary>
        public static readonly FadeLevelColour VariaBeam = new(16, 16, 0);
        /// <summary>The Gravity beam's colour as it appears, which its shrinking beam returns to.</summary>
        public static readonly FadeLevelColour GravityBeam = new(16, 9, 16);
        /// <summary>The white the widening beam brightens to by the reveal.</summary>
        public static readonly FadeLevelColour White = new(FadeLevelColour.FullLevel, FadeLevelColour.FullLevel, FadeLevelColour.FullLevel);
        /// <summary>The orange the shrinking Varia beam settles on.</summary>
        public static readonly FadeLevelColour VariaOrange = new(FadeLevelColour.FullLevel, 13, 3);
        /// <summary>The levels a channel rises each widening update, stopping at white's.</summary>
        public const int WhiteningLevelsPerUpdate = 2;
        /// <summary>The levels a channel falls each shrinking update, stopping at the settling colour's.</summary>
        public const int SettlingLevelsPerUpdate = 1;
    }

    /// <summary>The glow colour sequences of the Power Bomb and Crystal Flash explosions.</summary>
    public static class PowerBomb
    {
        /// <summary>Number of pre-explosion glow colours.</summary>
        public const int PreExplosionColorCount = 16;
        /// <summary>Number of explosion glow colours selected by radius.</summary>
        public const int ExplosionColorCount = 32;
    }
}
