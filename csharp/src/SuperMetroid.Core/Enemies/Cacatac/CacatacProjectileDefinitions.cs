namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>How fast Cacatac spikes fly.</summary>
internal static class CacatacProjectileDefinitions
{
    /// <summary>The speed a sideways or straight spike flies at along its one axis: two pixels per update.</summary>
    internal static readonly SignedEightEight CardinalSpeed = SignedEightEight.FromPixels(2);

    /// <summary>The speed a diagonal spike flies at along each of its two axes: one and a half pixels per update.</summary>
    internal static readonly SignedEightEight DiagonalSpeed = SignedEightEight.FromParts(1, 128);
}
