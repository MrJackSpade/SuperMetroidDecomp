namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>Compiled fixed behavior definitions for ordinary Metroids.</summary>
internal static class MetroidBehaviorDefinitions
{
    /// <summary>The first cry, selected by random slots 0 and 3.</summary>
    private static readonly SoundEffectId FirstCry = EnemySound.MetroidDrain;
    /// <summary>The second cry, selected by random slots 1, 4 and 6.</summary>
    private static readonly SoundEffectId SecondCry = EnemySound.ShotMochtroid;
    /// <summary>The third cry, selected by random slots 2, 5 and 7.</summary>
    private static readonly SoundEffectId ThirdCry = EnemySound.ShotMetroid;

    /// <summary>
    /// Four successive two-pixel cardinal displacements, right, up, left and
    /// down, by escape countdown phase.
    /// </summary>
    internal static readonly Offset16<RoomPixels>[] EscapeDisplacements = [new(2, 0), new(0, -2), new(-2, 0), new(0, 2)];

    /// <summary>
    /// The cry each of the eight random slots plays,
    /// preserving the 2/3/3 choice distribution and order.
    /// </summary>
    internal static readonly SoundEffectId[] RandomCries = [FirstCry, SecondCry, ThirdCry, FirstCry, SecondCry, ThirdCry, SecondCry, ThirdCry];

    /// <summary>The random field that picks which of the eight <see cref="RandomCries"/> slots a chasing Metroid plays: bits zero to two.</summary>
    internal static readonly RandomSampleField CrySlot = new(0, 3);

    /// <summary>A sixteenth of a health point, in the units of the drain counter.</summary>
    private const int SixteenthHealth = 4096;

    /// <summary>Gravity Suit Samus loses one health each time this fraction underflows the drain counter: three sixteenths.</summary>
    internal const int GravityDrainFraction = 3 * SixteenthHealth;

    /// <summary>Varia Suit Samus's drain fraction: six sixteenths.</summary>
    internal const int VariaDrainFraction = 6 * SixteenthHealth;

    /// <summary>Suitless Samus's drain fraction: twelve sixteenths.</summary>
    internal const int PowerSuitDrainFraction = 12 * SixteenthHealth;
}
