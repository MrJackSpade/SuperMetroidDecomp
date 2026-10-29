namespace SuperMetroid.Core;

/// <summary>
/// Marks a verification helper that builds a whole memory image on every call (#1463): a screen's composed characters
/// and tilemaps, a VRAM image, a composed background. SME6452 rejects calling one inside a loop or a per-element LINQ
/// lambda except as the initializer of a local, so a check builds the image once per state and reads from it rather than
/// rebuilding it for every byte it compares.
/// </summary>
/// <param name="builds">What the helper builds, as the diagnostic names it.</param>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal sealed class WholeImageFixtureAttribute(string builds) : Attribute
{
    /// <summary>What the helper builds.</summary>
    public string Builds { get; } = builds;
}
