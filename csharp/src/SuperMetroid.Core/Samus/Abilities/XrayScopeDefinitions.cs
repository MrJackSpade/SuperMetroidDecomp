using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The X-ray Scope's definitions: its visor cycle, animation angle bands and cone growth (#1411).</summary>
/// <remarks>
/// The room state's X-ray reveal choice remains room-authored data. This catalog owns the shared
/// visor shades, animation angle bands and cone widths used by every room.
/// </remarks>
public static class XrayScopeDefinitions
{

    /// <summary>
    /// Angle bands selecting which of the X-ray pose's five aims (<see cref="Aims"/>) the standing and crouching scope shows:
    /// a right-facing scope turns from up to down as its angle grows, a left-facing one as it shrinks.
    /// </summary>
    public static class AnimationAngles
    {
        /// <summary>The angle from which a right-facing scope aims raised rather than up.</summary>
        public static RotationAngle RightRaisedFrom { get; } = RotationAngle.FromSteps(25);
        /// <summary>The angle from which a right-facing scope aims level rather than raised.</summary>
        public static RotationAngle RightLevelFrom { get; } = RotationAngle.FromSteps(50);
        /// <summary>The angle from which a right-facing scope aims lowered rather than level.</summary>
        public static RotationAngle RightLoweredFrom { get; } = RotationAngle.FromSteps(75);
        /// <summary>The angle from which a right-facing scope aims down rather than lowered.</summary>
        public static RotationAngle RightDownFrom { get; } = RotationAngle.FromSteps(100);
        /// <summary>The angle below which a left-facing scope aims down rather than lowered.</summary>
        public static RotationAngle LeftDownBelow { get; } = RotationAngle.FromSteps(153);
        /// <summary>The angle below which a left-facing scope aims lowered rather than level.</summary>
        public static RotationAngle LeftLoweredBelow { get; } = RotationAngle.FromSteps(178);
        /// <summary>The angle below which a left-facing scope aims level rather than raised.</summary>
        public static RotationAngle LeftLevelBelow { get; } = RotationAngle.FromSteps(203);
        /// <summary>The angle below which a left-facing scope aims raised rather than up.</summary>
        public static RotationAngle LeftRaisedBelow { get; } = RotationAngle.FromSteps(228);
    }

    /// <summary>How long the X-ray pose's pictures the scope sets directly show for, in updates.</summary>
    public static class Holds
    {
        /// <summary>The activation picture, set as the scope takes Samus's pose.</summary>
        public const int Activation = 63;

        /// <summary>Each aim's picture, set again on every update the scope aims.</summary>
        public const int Aim = 15;
    }

    /// <summary>The X-ray pose's five aims, from up to down: the frames of its animation the scope's angle selects.</summary>
    internal static class Aims
    {
        /// <summary>The scope aimed up.</summary>
        internal static SamusAnimationFrame Up { get; } = new(SamusAnimationSegmentName.Main, 0);
        /// <summary>The scope aimed raised, between up and level.</summary>
        internal static SamusAnimationFrame Raised { get; } = new(SamusAnimationSegmentName.Main, 1);
        /// <summary>The scope aimed level, the aim its activation starts on.</summary>
        internal static SamusAnimationFrame Level { get; } = new(SamusAnimationSegmentName.Main, 2);
        /// <summary>The scope aimed lowered, between level and down.</summary>
        internal static SamusAnimationFrame Lowered { get; } = new(SamusAnimationSegmentName.Main, 3);
        /// <summary>The scope aimed down.</summary>
        internal static SamusAnimationFrame Down { get; } = new(SamusAnimationSegmentName.Main, 4);
    }

    /// <summary>How the X-ray scope's cone widens.</summary>
    public static class Cone
    {
        /// <summary>The widening speed's growth each widening update: a thirty-second of a step.</summary>
        public static readonly RotationAngle WideningStep = RotationAngle.FromSteps(0, 8);

        /// <summary>The half-width a full beam keeps: ten whole steps either side of its centre.</summary>
        public static readonly RotationAngle WidestHalfWidth = RotationAngle.FromSteps(10);
    }
}
