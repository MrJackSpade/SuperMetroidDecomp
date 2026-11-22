namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// One class of blocks whose start contributes the same to a Wave shot's straddled read (<see cref="StraddledTerrain{TTerrain}"/>):
/// an identity the installed table names, which each lead class's outcomes are keyed by. Evidence: the original scan
/// reads the first half of the next block's stored terrain, which only that block's number decides.
/// </summary>
/// <param name="name">The class's installed name, for diagnostics.</param>
public sealed partial class StraddledTerrainTail(string name)
{
    /// <summary>The class's installed name.</summary>
    public string Name { get; } = Ensure.NotNull(name);

    /// <summary>Names the class for diagnostics.</summary>
    public override string ToString() => Name;
}
