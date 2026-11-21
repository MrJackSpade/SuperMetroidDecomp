namespace SuperMetroid.Core;

/// <summary>
/// An object of a generic type of the game that registers its own construction with a game snapshot when the snapshot first
/// meets it (#2602): a construction made inside a generic method is named by no declaration the registration reads.
/// </summary>
public interface IStateGraphNode
{
    /// <summary>Tells the snapshot's registry how a snapshot holds objects of this object's type.</summary>
    /// <param name="registry">The snapshot's registry.</param>
    void Describe(IStateGraphRegistry registry);
}
