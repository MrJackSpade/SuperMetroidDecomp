using Godot;

namespace Engine.World;

/// <summary>
/// The check, made as a game loads, of the names its placed things are remembered by: no two things anywhere in the
/// game share a name, and nothing that must be remembered is without one. It gathers every mistake and reports them
/// together, each with the room and the thing it is about.
/// </summary>
public sealed class RememberedNames
{
    /// <summary>Where each name seen so far was first given: the room and the thing.</summary>
    private readonly Dictionary<string, string> _firstGivenBy = new(StringComparer.Ordinal);

    /// <summary>The mistakes found so far.</summary>
    private readonly List<string> _mistakes = [];

    /// <summary>Looks at everything placed in <paramref name="room"/>, the room made from <paramref name="scene"/>.</summary>
    public void Add(string scene, Node room)
    {
        ArgumentNullException.ThrowIfNull(room);
        foreach (Node node in Descendants(room))
        {
            if (node is not IRemembered remembered)
                continue;
            string where = $"\"{node.Name}\" in the room \"{scene}\"";
            if (remembered.RememberedAs.Length == 0)
            {
                if (remembered.MustBeRemembered)
                    _mistakes.Add($"{where} must be remembered and has no name to be remembered by.");
            }
            else if (_firstGivenBy.TryGetValue(remembered.RememberedAs, out string? first))
            {
                _mistakes.Add($"{where} is remembered as \"{remembered.RememberedAs}\", the name {first} already has.");
            }
            else
            {
                _firstGivenBy[remembered.RememberedAs] = where;
            }
        }
    }

    /// <summary>Fails, listing every mistake, if any was found.</summary>
    /// <exception cref="InvalidOperationException">A name is shared, or something that must be remembered has none.</exception>
    public void Require()
    {
        if (_mistakes.Count > 0)
            throw new InvalidOperationException("The game's remembered names are wrong:\n" + string.Join("\n", _mistakes));
    }

    /// <summary>Every node beneath <paramref name="node"/>, in tree order.</summary>
    private static IEnumerable<Node> Descendants(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            yield return child;
            foreach (Node below in Descendants(child))
                yield return below;
        }
    }
}
