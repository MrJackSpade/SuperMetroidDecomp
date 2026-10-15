namespace SuperMetroid.Core.Enemies;

/// <summary>Masks the enemy draw list applies to sprite positions.</summary>
internal static class EnemyDrawMasks
{
    /// <summary>Any of these bits in a component origin offset by 128 puts it outside the drawable -128..383 range.</summary>
    internal const int OffscreenBits = 0xfe00;
}
