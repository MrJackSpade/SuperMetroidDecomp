namespace SuperMetroid.Core.Samus.Colours;

/// <summary>Immutable game data shared by Samus shades.</summary>
/// <remarks>
/// Shade state machines retain their mutable timers and phase transitions. This catalog
/// names the colour tables, colour-animation steps, the colours each shade changes, and endpoint colours those
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
        /// <summary>Timer reloaded after each visor shade is shown.</summary>
        public const int FrameDelay = 5;
        /// <summary>The X-ray widening glow's shades, shown in order; the last is held.</summary>
        public const int XrayWideningShades = 3;
        /// <summary>The steady visor cycle's shades, shown in order and repeated from the first.</summary>
        public const int CycleShades = 3;
    }

    /// <summary>The suit-and-phase lists the special full-body suit colourings select their cycle shades from.</summary>
    public static class FullBodyCycles
    {
        /// <summary>Number of full-body Hyper Beam shades.</summary>
        public const int HyperBeamShadeCount = 10;
        /// <summary>The steps in each suit's Screw Attack cycle list; the cycle wraps to its first step after the last.</summary>
        public const int ScrewAttackSteps = 6;
        /// <summary>The steps in each suit's active Speed Booster cycle list.</summary>
        public const int SpeedBoosterSteps = 4;
        /// <summary>The Speed Booster step the cycle holds once reached: its list's last.</summary>
        public const int SpeedBoosterLastStep = SpeedBoosterSteps - 1;
        /// <summary>The shading updates each active Speed Booster step shows for.</summary>
        public const int SpeedBoosterStepUpdates = 4;
        /// <summary>The shading updates before the active Speed Booster cycle's first step when the boost starts: it starts on the next.</summary>
        public const int SpeedBoosterFirstStepWait = 1;
        /// <summary>The steps in each suit's stored-shine cycle list.</summary>
        public const int StoredShineSteps = 6;
        /// <summary>The steps in each suit's active-shinespark cycle list.</summary>
        public const int ActiveShinesparkSteps = 4;
        /// <summary>The step count a ping-pong cycle mirrors across: step <c>s</c> past the middle shows shade <c>PingPongSpan - s</c>.</summary>
        public const int PingPongSpan = 6;
        /// <summary>The Speed Booster shade the Metroid-attachment flash alternates with the normal suit: the family's brightest.</summary>
        public const int SpeedBoostFlashShade = 3;
    }

    /// <summary>Shade tables consumed by the fatal-damage sequence.</summary>
    public static class Death
    {
        /// <summary>Number of suited and suitless explosion shade variants.</summary>
        public const int ExplosionShadeCount = 10;
        /// <summary>Number of shades in the inclusive whiteout ramp.</summary>
        public const int WhiteoutShadeCount = 22;
    }

    /// <summary>Two independently timed colour cycles used by Crystal Flash.</summary>
    /// <remarks>
    /// Samus's body brightens through six grey shades and dims back again, ten steps in all; the bubble around her cycles
    /// its own six shades on a separate timer, wrapping independently of the body. Both cycles stay live while Crystal
    /// Flash runs. The shades themselves are installed colours (the import's Crystal Flash colour format describes
    /// how they derive from the original's data; #625, #902, #903).
    /// </remarks>
    public static class CrystalFlash
    {
        /// <summary>Number of Crystal Flash body steps.</summary>
        public const int BodyStepCount = 10;
        /// <summary>Number of bubble shades.</summary>
        public const int BubbleShadeCount = 6;
    }

    /// <summary>The light beam's colour endpoints in the shared Varia/Gravity pickup sequence, in levels.</summary>
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
