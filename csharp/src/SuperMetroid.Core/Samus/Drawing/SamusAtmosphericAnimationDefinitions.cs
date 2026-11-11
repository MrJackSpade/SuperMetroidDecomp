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
    /// <summary>The updates each of the diving splash's nine steps shows, in step order: nine retained exposure choices for the drawn ripple/jet/droplet choreography; discrete frame art does not determine these holds. Preserve old-frame reload and the final hold of seven, which no step shows.</summary>
    internal static IReadOnlyList<int> DivingSplashStepUpdates { get; } = [2, 2, 3, 3, 3, 5, 5, 6, 7];
    /// <summary>Dust rises one pixel on each update it is shown.</summary>
    internal static readonly Offset16<RoomPixels> DustDrift = new(0, -1);
    /// <summary>Lava spray rises one pixel on each update it is shown, as it drifts one pixel to its side.</summary>
    internal const int LavaSprayRise = -1;
    /// <summary>Half the width and height of a splash, spray or dust picture, which is drawn centred on its particle.</summary>
    internal const int SmallParticleHalfWidth = 4;
}
