namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The shape of the cartridge's Samus body placement tables and the fixed placements its draw selects (#1517), which
/// <see cref="SamusBodyPlacementDecoding"/> decodes once into each installed frame's placement.
/// </summary>
internal static class SamusBodyPlacementSourceDefinitions
{
    /// <summary>The landing table's bytes: four landing poses, the right- and left-facing normal and spin landings.</summary>
    internal const int LandingByteCount = 16;

    /// <summary>The landing table's bytes per landing pose, one per animation frame.</summary>
    internal const int LandingPoseByteCount = 4;

    /// <summary>The posture-transition table's bytes: twelve transition poses from the right-facing crouching transition.</summary>
    internal const int PostureByteCount = 24;

    /// <summary>The posture-transition table's bytes per transition pose, one per animation frame.</summary>
    internal const int PosturePoseByteCount = 2;

    /// <summary>The drained-body table's bytes, read directly by the drained crouch's animation frame.</summary>
    internal const int DrainedByteCount = 32;

    /// <summary>The first forward-facing frame the draw pins to <see cref="ForwardFacingLaterRise"/> instead of the pose's art origin.</summary>
    internal const int ForwardFacingFirstLaterFrame = 2;

    /// <summary>How many pixels above Samus's center the later forward-facing frames place the body.</summary>
    internal const int ForwardFacingLaterRise = 1;

    /// <summary>The first drained standing frame the draw places by <see cref="DrainedStandingLaterRise"/> instead of the pose's art origin.</summary>
    internal const int DrainedStandingFirstLaterFrame = 5;

    /// <summary>How many pixels above Samus's center the later drained standing frames place the body.</summary>
    internal const int DrainedStandingLaterRise = 3;
}
