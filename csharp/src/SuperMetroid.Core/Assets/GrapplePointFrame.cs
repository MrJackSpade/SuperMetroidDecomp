namespace SuperMetroid.Core.Assets;

/// <summary>The four Grapple endpoint animation frames, in reviewed visual order.</summary>
internal enum GrapplePointFrame
{
    /// <summary>$9A:8200/$9B:BFBD first endpoint frame selects the filled diamond.</summary>
    FilledDiamond,
    /// <summary>$9A:8400 second endpoint frame selects the hollow-center diamond.</summary>
    HollowDiamond,
    /// <summary>$9A:8600 third endpoint frame selects the tipped square outline.</summary>
    TippedSquare,
    /// <summary>$9A:8800 fourth endpoint frame selects the clipped diamond/inner spark.</summary>
    ClippedDiamond,
}
