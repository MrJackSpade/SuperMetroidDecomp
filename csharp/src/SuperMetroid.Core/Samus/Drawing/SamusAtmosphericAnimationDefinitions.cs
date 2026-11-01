namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Calculated uniform/progressive frame cadence with narrowly retained original atmospheric exposure choreography. These selected animation holds are not a fluid or damage integration law.</summary>
internal static class SamusAtmosphericAnimationDefinitions
{
    /// <summary>Uniform footstep hold3; the exact selected exposure is retained under the reviewed original-animation disposition.</summary>
    internal const int FootstepFrameTicks = 3;
    /// <summary>Uniform lava-surface hold2; the exact selected exposure is retained under the reviewed original-animation disposition.</summary>
    internal const int LavaSurfaceFrameTicks = 2;
    /// <summary>Uniform bubble hold5; the exact selected exposure is retained under the reviewed original-animation disposition.</summary>
    internal const int BubbleFrameTicks = 5;
    /// <summary>Dust holds start at3; this exact initial exposure is retained under the reviewed original-animation disposition.</summary>
    internal const int DustInitialFrameTicks = 3;
    /// <summary>Dust holds advance by1 tick per frame; the exact progression increment is retained under the reviewed original-animation disposition.</summary>
    internal const int DustFrameTickIncrement = 1;
    /// <summary>Nine retained exposure choices for the drawn ripple/jet/droplet choreography; discrete frame art does not determine these holds. Preserve old-frame reload, slot admission/sound-RNG consequences and terminal stale7.</summary>
    internal static readonly ushort[] RetainedDivingSplashTimers = [2, 2, 3, 3, 3, 5, 5, 6, 7];
}
