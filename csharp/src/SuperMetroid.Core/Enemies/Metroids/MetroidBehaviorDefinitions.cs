namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>Compiled fixed behavior definitions for ordinary Metroids.</summary>
internal static class MetroidBehaviorDefinitions
{
    /// <summary>Library-two cry 80, selected by random slots 0 and 3.</summary>
    private static readonly SoundEffectId FirstCry = Library2Sound.MetroidDrain;
    /// <summary>Library-two cry 88, selected by random slots 1, 4 and 6.</summary>
    private static readonly SoundEffectId SecondCry = Library2Sound.ShotMochtroid;
    /// <summary>Library-two cry 90, selected by random slots 2, 5 and 7.</summary>
    private static readonly SoundEffectId ThirdCry = Library2Sound.ShotMetroid;

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

    /// <summary>A sixteenth of a health point, in the units of the drain accumulator.</summary>
    private const int SixteenthHealth = 4096;

    /// <summary>Gravity Suit Samus loses one health each time this fraction underflows the drain accumulator: three sixteenths.</summary>
    internal const int GravityDrainFraction = 3 * SixteenthHealth;

    /// <summary>Varia Suit Samus's drain fraction: six sixteenths.</summary>
    internal const int VariaDrainFraction = 6 * SixteenthHealth;

    /// <summary>Suitless Samus's drain fraction: twelve sixteenths.</summary>
    internal const int PowerSuitDrainFraction = 12 * SixteenthHealth;
}
