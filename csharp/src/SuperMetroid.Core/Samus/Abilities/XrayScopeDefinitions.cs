using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The X-ray Scope's definitions: its visor cycle, animation angle bands and window growth (#1411).</summary>
/// <remarks>
/// The room state's X-ray pointer remains room-authored data. This catalog owns the shared
/// palette, angle bands, tangent table, and cone-window units used by every room.
/// </remarks>
public static class XrayScopeDefinitions
{
    /// <summary>Palette words copied into Samus's visor colors while X-ray is active.</summary>
    public static class Palette
    {
        /// <summary>The last widening visor shade, held while the beam widens.</summary>
        public const int WideningFinalShade = 2;
        /// <summary>The first visor shade of the full-beam cycle.</summary>
        public const int FullCycleFirstShade = SamusPaletteDefinitions.Visor.CycleFirstShade;
        /// <summary>The visor shade count, which ends the full-beam cycle.</summary>
        public const int FullCycleEndShade = SamusPaletteDefinitions.Visor.CycleEndShade;
        /// <summary>Frames between visor color advances.</summary>
        public const int FrameDelay = SamusPaletteDefinitions.Visor.FrameDelay;
    }

    /// <summary>Angle bands selecting the five standing/crouching X-ray animation frames.</summary>
    public static class AnimationAngles
    {
        /// <summary>Right-facing boundary between frames zero and one.</summary>
        public const int RightFrameOne = 0x19;
        /// <summary>Right-facing boundary between frames one and two.</summary>
        public const int RightFrameTwo = 0x32;
        /// <summary>Right-facing boundary between frames two and three.</summary>
        public const int RightFrameThree = 0x4b;
        /// <summary>Right-facing boundary between frames three and four.</summary>
        public const int RightFrameFour = 0x64;
        /// <summary>Left-facing boundary between frames four and three.</summary>
        public const int LeftFrameFour = 0x99;
        /// <summary>Left-facing boundary between frames three and two.</summary>
        public const int LeftFrameThree = 0xb2;
        /// <summary>Left-facing boundary between frames two and one.</summary>
        public const int LeftFrameTwo = 0xcb;
        /// <summary>Left-facing boundary between frames one and zero.</summary>
        public const int LeftFrameOne = 0xe4;
    }

    /// <summary>How the X-ray scope's cone widens.</summary>
    public static class Window
    {
        /// <summary>Per-frame 16.16 angular-width growth applied while widening.</summary>
        public const uint WideningStep = 0x00000800u;
    }
}
