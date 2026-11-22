namespace SuperMetroid.Game.Speeds;

/// <summary>The speeds by name, which the compiler lists.</summary>
internal static partial class SpeedNames
{
    /// <summary>Every speed <see cref="SpeedDefinitions"/> names.</summary>
    [NamedMembers(typeof(SpeedDefinitions))]
    internal static partial (int Value, string Owner, string Name)[] All();
}
