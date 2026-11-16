namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// How long each picture of the splashes, lava spray, bubbles and dust Samus raises shows, and how dust and spray drift.
/// </summary>
internal static class SamusAtmosphericAnimationDefinitions
{
    /// <summary>Each footstep frame shows for three ticks, as authored.</summary>
    internal const int FootstepFrameTicks = 3;
    /// <summary>Each lava-surface frame shows for two ticks, as authored.</summary>
    internal const int LavaSurfaceFrameTicks = 2;
    /// <summary>Each bubble frame shows for five ticks, as authored.</summary>
    internal const int BubbleFrameTicks = 5;
    /// <summary>A dust cloud's first frame shows for three ticks, as authored.</summary>
    internal const int DustInitialFrameTicks = 3;
    /// <summary>Each later dust frame shows one tick longer than the one before, as authored.</summary>
    internal const int DustFrameTickIncrement = 1;
    /// <summary>
    /// The updates each of the diving splash's nine steps lasts, in step order, as authored. A step that ends hands its
    /// own length to the step after it (<see cref="SamusParticle.Advance"/>), so each later step lasts as long as the one
    /// before it.
    /// </summary>
    internal static IReadOnlyList<int> DivingSplashStepUpdates { get; } = [2, 2, 3, 3, 3, 5, 5, 6, 7];
    /// <summary>Dust rises one pixel on each update it is shown.</summary>
    internal static readonly Offset16<RoomPixels> DustDrift = new(0, -1);
    /// <summary>Lava spray rises one pixel on each update it is shown, as it drifts one pixel to its side.</summary>
    internal const int LavaSprayRise = -1;
    /// <summary>Half the width and height of a splash, spray or dust picture, which is drawn centred on its particle.</summary>
    internal const int SmallParticleHalfWidth = 4;
}
