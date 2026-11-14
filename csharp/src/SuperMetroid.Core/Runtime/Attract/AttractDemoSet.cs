namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// A set of attract demonstrations the title plays in order, one after another while no input cancels them; the set
/// ends with its last demonstration.
/// </summary>
public sealed class AttractDemoSet
{
    /// <summary>Creates the set named <paramref name="name"/> playing <paramref name="scenes"/> in order.</summary>
    internal AttractDemoSet(string name, IReadOnlyList<AttractDemoScene> scenes)
    {
        Name = name;
        Scenes = scenes;
    }

    /// <summary>The set's name.</summary>
    public string Name { get; }

    /// <summary>The demonstrations in playing order.</summary>
    public IReadOnlyList<AttractDemoScene> Scenes { get; }

    /// <inheritdoc />
    public override string ToString() => Name;
}
