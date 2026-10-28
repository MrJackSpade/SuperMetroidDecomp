using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The pose-half table's composition runs and the shapes of the animation commands it skips.</summary>
/// <remarks>The exact remaining 813 drawing choices, one observable zero-WRAM identity and named sequence memberships are selected composition content. They cannot be derived from physical pose quantities without restating the chosen pictures. Parts, pixels and timing have separate owners; see Stream 1 Batch 103 for exact native membership and consumer evidence.</remarks>
internal static class SamusSpritemapFrameDefinitions
{
    /// <summary>$92:8091/8151/81D1/8291: appearance lists contain96 indexed phases, including the initial body and mutable-memory slot.</summary>
    internal static int AppearancePhases =>
        SamusAnimationDelays.BytesToStreamEnd(SamusAnimationDelays.ListForPose(SamusPoseId.ForwardFacingPowerSuitPose)) - 2;
    /// <summary>$92:8095..812F: the initial electricity sequence alternates three distinct discharges with its shared empty composition.</summary>
    internal const int AppearanceDischargeEnd = 80;
    /// <summary>$92:86F3/8777: upper grapple composition repeats after one32-position turn; the two final entries select its center orientation.</summary>
    internal const int GrappleTurnPhases = GrappleSwingFrameDefinitions.FrameCount;
    /// <summary>$92:8EAD begins the right morph sequence's spin, memory, and bounce slots.</summary>
    internal const int FirstRightMorph = 0x710;
    /// <summary>$92:8EC1 begins the mirrored left morph sequence used to derive its phase span.</summary>
    internal const int FirstLeftMorph = 0x71a;

    /// <summary>The first of the consecutive bubble compositions.</summary>
    internal static readonly SamusSpritemapId FirstBubble = new(449);
    /// <summary>The first of the consecutive diving-splash compositions.</summary>
    internal static readonly SamusSpritemapId FirstDiving = new(458);
    /// <summary>The first of the right-facing suit-explosion compositions.</summary>
    internal static readonly SamusSpritemapId FirstDeathRight = new(440);
    /// <summary>The first of the left-facing suit-explosion compositions.</summary>
    internal static readonly SamusSpritemapId FirstDeathLeft = new(431);
    /// <summary>Starts right-facing grapple angles zero through eight, selected in reverse record order.</summary>
    internal static readonly SamusSpritemapId GrappleRightFirstQuadrant = new(168);
    /// <summary>Starts the middle right-facing grapple arc, selected in reverse order.</summary>
    internal static readonly SamusSpritemapId GrappleRightHalfTurn = new(136);
    /// <summary>Starts the final right-facing grapple quadrant.</summary>
    internal static readonly SamusSpritemapId GrappleRightLastQuadrant = new(177);
    /// <summary>Starts left-facing grapple angles in forward record order.</summary>
    internal static readonly SamusSpritemapId GrappleLeftFirstQuadrant = new(192);
    /// <summary>Starts the middle left-facing grapple arc.</summary>
    internal static readonly SamusSpritemapId GrappleLeftHalfTurn = new(152);
    /// <summary>Starts the final left-facing grapple quadrant.</summary>
    internal static readonly SamusSpritemapId GrappleLeftLastQuadrant = new(184);
    /// <summary>Top-half pointer index $186 begins the bubble OAM sequence.</summary>
    internal const int Bubbles = 0x186;
    /// <summary>Top-half pointer index $18F begins the diving OAM sequence.</summary>
    internal const int Diving = 0x18f;
    /// <summary>Pointer index $81C begins right-facing suit-explosion maps.</summary>
    internal const int DeathRight = 0x81c;
    /// <summary>Pointer index $825 begins left-facing suit-explosion maps.</summary>
    internal const int DeathLeft = 0x825;

    /// <summary>The bytes each animation command with operands occupies in a pose's delay list; every other byte is one.</summary>
    internal static readonly IReadOnlyDictionary<int, int> AnimationCommandSizes = new Dictionary<int, int>
    {
        [0xf8] = 2, [0xfd] = 2, [0xfe] = 2, [0xf9] = 7, [0xfa] = 3, [0xfc] = 5,
    };

    /// <summary>The animation-list bytes that are commands drawing no frame.</summary>
    internal static readonly IReadOnlySet<int> NonDrawingAnimationBytes = new HashSet<int> { 0xf6, 0xfb, 0xfd, 0xfe, 0xff };
}
