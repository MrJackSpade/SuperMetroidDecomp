namespace SuperMetroid.Core.Rendering;

/// <summary>Display-coordinate definitions from the bank-$91 eye/X-ray window calculation.</summary>
internal static class EyeWindowRenderDefinitions
{
    /// <summary>$91:C5FF and sibling on-screen branches write the apex at r24-1, one line above the supplied body Y.</summary>
    internal const int ApexYOffset = 1;
    /// <summary>The $00FF fractional remainder of the 8.8 tangent accumulators rounds boundaries out to their containing pixels.</summary>
    internal const int SubpixelTolerance = 0x00ff;
}
