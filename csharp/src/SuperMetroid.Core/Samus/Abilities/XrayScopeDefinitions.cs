using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The X-ray Scope's definitions: its visor cycle, animation angle bands and window growth (#1411).</summary>
/// <remarks>
/// The room state's X-ray reveal choice remains room-authored data. This catalog owns the shared
/// palette, angle bands, tangent table, and cone-window units used by every room.
/// </remarks>
public static class XrayScopeDefinitions
{
    /// <summary>Palette colours copied into Samus's visor colors while X-ray is active.</summary>
    public static class Palette
    {
        /// <summary>The last widening visor shade, held while the beam widens.</summary>
        public const int WideningFinalShade = 2;
        /// <summary>The first visor shade of the full-beam cycle.</summary>
        public const int FullCycleFirstShade = SamusPaletteDefinitions.Visor.CycleFirstShade;
        /// <summary>The visor shade count, which ends the full-beam cycle.</summary>
        public const int FullCycleEndShade = SamusPaletteDefinitions.Visor.CycleEndShade;
        /// <summary>Frames between visor shade advances.</summary>
        public const int FrameDelay = SamusPaletteDefinitions.Visor.FrameDelay;
    }

    /// <summary>Angle bands selecting the five standing/crouching X-ray animation frames.</summary>
    public static class AnimationAngles
    {
        /// <summary>Right-facing boundary between frames zero and one.</summary>
        public static RotationAngle RightFrameOne { get; } = RotationAngle.FromSteps(25);
        /// <summary>Right-facing boundary between frames one and two.</summary>
        public static RotationAngle RightFrameTwo { get; } = RotationAngle.FromSteps(50);
        /// <summary>Right-facing boundary between frames two and three.</summary>
        public static RotationAngle RightFrameThree { get; } = RotationAngle.FromSteps(75);
        /// <summary>Right-facing boundary between frames three and four.</summary>
        public static RotationAngle RightFrameFour { get; } = RotationAngle.FromSteps(100);
        /// <summary>Left-facing boundary between frames four and three.</summary>
        public static RotationAngle LeftFrameFour { get; } = RotationAngle.FromSteps(153);
        /// <summary>Left-facing boundary between frames three and two.</summary>
        public static RotationAngle LeftFrameThree { get; } = RotationAngle.FromSteps(178);
        /// <summary>Left-facing boundary between frames two and one.</summary>
        public static RotationAngle LeftFrameTwo { get; } = RotationAngle.FromSteps(203);
        /// <summary>Left-facing boundary between frames one and zero.</summary>
        public static RotationAngle LeftFrameOne { get; } = RotationAngle.FromSteps(228);
    }

    /// <summary>How the X-ray scope's cone widens.</summary>
    public static class Window
    {
        /// <summary>Per-frame angular-width growth applied while widening: 2048/65536 of a table unit.</summary>
        public static readonly SignedSixteenSixteen WideningStep = SignedSixteenSixteen.FromParts(0, 2048);
    }
}
