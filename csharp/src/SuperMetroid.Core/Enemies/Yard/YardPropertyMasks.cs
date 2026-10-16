namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>The Yard's facing, kept in its two low property bits.</summary>
internal static class YardPropertyMasks
{
    /// <summary>The two low property bits the Yard rewrites with its facing.</summary>
    internal const byte FacingBits = 3;
}
