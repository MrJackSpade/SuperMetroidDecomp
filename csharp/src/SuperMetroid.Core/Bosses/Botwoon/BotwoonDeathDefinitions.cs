namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>The random choices of Botwoon's death: where its drops fall and where its wall explosions burst.</summary>
internal static class BotwoonDeathDefinitions
{
    /// <summary>A death drop's X across its 128-pixel strip from X 64: the sample modulo 128.</summary>
    internal static readonly RandomSampleField DropX = new(0, 7);

    /// <summary>A death drop's Y down its 64-pixel band from Y 128: the sample divided by 256, modulo 64.</summary>
    internal static readonly RandomSampleField DropY = new(8, 6);

    /// <summary>A large wall explosion's X across its 32-pixel strip from X 232: the sample modulo 32.</summary>
    internal static readonly RandomSampleField LargeExplosionX = new(0, 5);

    /// <summary>A small wall explosion's X across its 64-pixel strip from X 224: the sample modulo 64.</summary>
    internal static readonly RandomSampleField SmallExplosionX = new(0, 6);

    /// <summary>
    /// A wall explosion's height jitter, 0 through 31 pixels added to the wall sequence's frame less eight: the sample
    /// modulo 32, drawn for large and small explosions alike.
    /// </summary>
    internal static readonly RandomSampleField ExplosionYJitter = new(0, 5);
}
