using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The Grapple Beam's definitions: its palette, sounds, swing physics, firing and rope drawing (#1411).</summary>
/// <remarks>
/// Grapple runtime state deliberately remains elsewhere. This type names the tables,
/// callback offsets, physics coefficients, and fixed artwork destinations that define
/// the subsystem's data contract.
/// </remarks>
public static class GrappleBeamDefinitions
{
    /// <summary>Palette operands shared by grapple initialization and cleanup tails.</summary>
    public static class Palettes
    {
        /// <summary>Firing selects the Ice beam's colours before entering the firing function.</summary>
        public const SamusBeamCombination FiringSelection = SamusBeamCombination.Ice;
    }

    /// <summary>Exact sound queue commands issued by grapple functions.</summary>
    public static class Sounds
    {
        /// <summary>Stop the old beam before pose-change refiring.</summary>
        public static readonly SoundRequest RestartStop = new(SamusSound.GrappleEnd, SoundRequestPriority.Normal);
        /// <summary>Firing tail: start the extending beam, only into an idle queue.</summary>
        public static readonly SoundRequest Fire = new(SamusSound.GrappleStart, SoundRequestPriority.WhenIdle);
        /// <summary>Accepted-contact tail: attached beam sound.</summary>
        public static readonly SoundRequest Attach = new(SamusSound.Grappling, SoundRequestPriority.Normal);
        /// <summary>Every grapple teardown: stop the active grapple sound, at essential priority.</summary>
        public static readonly SoundRequest Stop = new(SamusSound.GrappleEnd, SoundRequestPriority.Essential);
    }
    /// <summary>Angular-physics coefficients embedded in behavior.</summary>
    public static class Physics
    {
        /// <summary>Firing handler's accepted-connection tail: automatically retract eight pixels per frame after the connection helper clears its delta.</summary>
        public const int InitialConnectionRetraction = -8;
        /// <summary>How far the rope grows each update while the beam is fired.</summary>
        public const int FiringExtension = 12;
        /// <summary>The rope length at which a fired beam that has met nothing is cancelled.</summary>
        public const int FiringCancelLength = 128;
        /// <summary>
        /// The longest the rope becomes: firing extends a rope shorter than <see cref="FiringCancelLength"/> by
        /// <see cref="FiringExtension"/> and then cancels it, and connected motion keeps it shorter still.
        /// </summary>
        public const int LongestRopeLength = FiringCancelLength - 1 + FiringExtension;
        /// <summary>Per-frame downward angular acceleration.</summary>
        public const int GravityMagnitude = 24;
        /// <summary>Per-frame angular acceleration contributed by directional input.</summary>
        public const int DirectionInputMagnitude = 12;
        /// <summary>Correction applied when velocity opposes the current swing direction.</summary>
        public const int VelocityCorrectionMagnitude = 5;
        /// <summary>Maximum signed angular velocity.</summary>
        public const int MaximumAngularVelocity = 1152;
        /// <summary>Angular impulse applied by Jump during a swing.</summary>
        public const int JumpImpulseMagnitude = 768;
    }

    /// <summary>Direction-indexed firing velocity, origin, and flare tables.</summary>
    public static class Firing
    {
        /// <summary>Firing initializes the grapple's pose-change auto-fire timer to ten; the pose-change check decrements before checking the pose.</summary>
        public const int PoseChangeAutoFireFrames = 10;
        /// <summary>Firing uses a six-pixel graphics Y offset for moving Draygon-held poses.</summary>
        public const int DraygonMovingGraphicsYOffset = 6;
        /// <summary>Number of direction records represented by every firing table.</summary>
        public const int DirectionCount = 10;
    }

    /// <summary>Swing-frame, body-offset, rope-character, and fixed upload definitions.</summary>
    public static class Rendering
    {
        /// <summary>The rope's distance between consecutive rope objects in pixels.</summary>
        public const int SegmentSpacing = 8;
        /// <summary>Convert a signed 8.8 sample into an eight-pixel 16.16 displacement.</summary>
        public const int SegmentSampleToFixedPoint = SegmentSpacing * 256;
        /// <summary>The rope's segment counts the length quotient wraps through, one per animation slot.</summary>
        public const int SegmentCountCycle = 16;
        /// <summary>The rope's first animation slot, traversed downward.</summary>
        public const int FirstSegmentSlot = 15;
        /// <summary>The rope centers an eight-pixel character on its beam coordinate.</summary>
        public const int CharacterCenterOffset = 4;
        /// <summary>A rope segment is off screen when either whole coordinate reaches this extent.</summary>
        public const int ViewportExtent = 256;
        /// <summary>Each rope animation record lasts five visits to its slot.</summary>
        public const int SegmentAnimationDelay = 5;
        /// <summary>Four successive rope animation records before the goto.</summary>
        public const int SegmentAnimationFrameCount = 4;
    }

}
