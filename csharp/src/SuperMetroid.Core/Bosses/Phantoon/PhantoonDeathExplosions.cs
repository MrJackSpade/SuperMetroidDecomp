namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Reads Phantoon's death explosion requests in record order.</summary>
public static class PhantoonDeathExplosions
{
    /// <summary>Signed X/Y offsets, misc-dust explosion animation, and delay until the next request.</summary>
    public static PhantoonDeathExplosionDefinition Read(int index) =>
        (uint)index < (uint)PhantoonDeathExplosionDefinitions.Explosions.Length
            ? PhantoonDeathExplosionDefinitions.Explosions[index]
            : throw new ArgumentOutOfRangeException(nameof(index));
}
