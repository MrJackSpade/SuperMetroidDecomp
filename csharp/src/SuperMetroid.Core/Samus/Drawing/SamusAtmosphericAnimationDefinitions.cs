namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Calculated uniform/progressive frame cadence with narrowly retained original atmospheric exposure choreography. These selected animation holds are not a fluid or damage integration law.</summary>
internal static class SamusAtmosphericAnimationDefinitions
{
    /// <summary>$90:8BA5/8BAD uniform footstep hold3; the exact selected exposure is retained under the reviewed original-animation disposition.</summary>
    internal const int FootstepFrameTicks = 3;
    /// <summary>$90:8BC7 uniform lava-surface hold2; the exact selected exposure is retained under the reviewed original-animation disposition.</summary>
    internal const int LavaSurfaceFrameTicks = 2;
    /// <summary>$90:8BCF uniform bubble hold5; the exact selected exposure is retained under the reviewed original-animation disposition.</summary>
    internal const int BubbleFrameTicks = 5;
    /// <summary>$90:8BDF/8BE7 starts dust holds at3; this exact initial exposure is retained under the reviewed original-animation disposition.</summary>
    internal const int DustInitialFrameTicks = 3;
    /// <summary>$90:8BDF..8BED advances dust holds by1 tick per frame; the exact progression increment is retained under the reviewed original-animation disposition.</summary>
    internal const int DustFrameTickIncrement = 1;
    /// <summary>$90:8BB5..8BC6, nine retained exposure choices for the drawn ripple/jet/droplet choreography; discrete frame art does not determine these holds. Preserve old-frame reload, slot admission/sound-RNG consequences and terminal stale7.</summary>
    internal static readonly ushort[] RetainedDivingSplashTimers = [2, 2, 3, 3, 3, 5, 5, 6, 7];
}
