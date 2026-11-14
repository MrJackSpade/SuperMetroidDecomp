namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Endpoint animation, distinct from the rope's angle selection.</summary>
internal static class GrapplePointAnimationDefinitions
{
    /// <summary>The endpoint animation shows four frames before returning to the first.</summary>
    internal const int FrameCount = 4;
    /// <summary>The endpoint's frame delay: five, decremented through zero for a six-call interval.</summary>
    internal const int Delay = 5;
}
