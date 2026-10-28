namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Reads Phantoon's death explosion requests in record order.</summary>
public static class PhantoonDeathExplosions
{
    /// <summary>$A7:DA1D..DA50: signed X/Y offsets, misc-dust explosion animation, and delay until the next request.</summary>
    public static PhantoonDeathExplosionDefinition Read(int index) =>
        (uint)index < (uint)PhantoonDeathExplosionDefinitions.Records.Length
            ? PhantoonDeathExplosionDefinitions.Records[index]
            : throw new ArgumentOutOfRangeException(nameof(index));
}
