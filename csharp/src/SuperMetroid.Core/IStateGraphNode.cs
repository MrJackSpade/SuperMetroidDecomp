namespace SuperMetroid.Core;

/// <summary>
/// An object of the game that registers its own type with a registry when the registry first meets it (#2602): a
/// construction of a generic type made inside a generic method is named by no declaration the registration reads, and a
/// registry that learns types as it meets them, such as a comparison of two published graphs, registers nothing else.
/// </summary>
public interface IStateGraphNode
{
    /// <summary>Tells the registry how a snapshot holds objects of this object's type.</summary>
    /// <param name="registry">The registry.</param>
    void Describe(IStateGraphRegistry registry);
}
