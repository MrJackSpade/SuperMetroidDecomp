using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The objects the game's static members hold, and every object reachable from them, each addressed by its path from the
/// member it was first reached from (#1623): the catalogs' identities, their behaviour objects and their definition data.
/// A captured state names such an object by its path instead of holding a copy, and a restore resolves the path to the
/// object this build's member holds, so a restored state holds the catalog's own members.
/// </summary>
/// <remarks>
/// A root is a static read-only member of a type the game's assembly declares, holding an object other than a string,
/// which the compiler registers (#2602). Each declaring type is a <see cref="DefinitionCatalog"/>, indexed only when a
/// capture or restore first needs it (#1716): a capture asks only the catalogs whose declarations can hold an object of
/// the type it meets (<see cref="DefinitionExposure"/>), and a restore only the catalog its path names. An object's path is
/// its shortest from a root, the earliest root in name order among the shortest.
/// </remarks>
/// <param name="types">The registry holding the catalogs.</param>
internal sealed class DefinitionObjects(StateGraphTypes types)
{
    /// <summary>The process's index, which every capture and restore consults unless handed another.</summary>
    internal static DefinitionObjects Process => process.Value;

    private static readonly Lazy<DefinitionObjects> process = new(static () => new DefinitionObjects(StateGraphTypes.Shared));

    /// <summary>Guards the catalogs' indexes, whose path and resolution caches every capture and restore share.</summary>
    internal readonly Lock gate = new();

    /// <summary>The catalogs that may hold an object of <paramref name="value"/>'s type where a game can reach it.</summary>
    internal IReadOnlyList<DefinitionCatalog> Candidates(object value)
    {
        Ensure.NotNull(value);
        lock (gate)
            return Declared.Exposure.Candidates(value);
    }

    /// <summary>Whether <paramref name="value"/> is held by a static member, and its path when it is.</summary>
    internal bool TryGetPath(object value, [NotNullWhen(true)] out string? path)
    {
        Ensure.NotNull(value);
        lock (gate)
        {
            path = null;
            int shortest = int.MaxValue;
            foreach (DefinitionCatalog catalog in Candidates(value))
            {
                if (!catalog.Graph.TryGetPath(value, out string? found, out int depth) || depth >= shortest)
                    continue;
                path = found;
                shortest = depth;
                // A root is the shortest path there is, and a later catalog's roots come later in name order.
                if (depth == 0)
                    break;
            }
            return path is not null;
        }
    }

    /// <summary>The object <paramref name="path"/> names among the static members' objects.</summary>
    /// <exception cref="InvalidDataException">The path names no object this build's static members hold.</exception>
    internal object Resolve(string path)
    {
        Ensure.NotNull(path);
        lock (gate)
        {
            int separator = path.IndexOf(DefinitionCatalog.MemberSeparator, StringComparison.Ordinal);
            if (separator < 0 || !Declared.ByPrefix.TryGetValue(path[..(separator + DefinitionCatalog.MemberSeparator.Length)], out DefinitionCatalog? catalog))
                throw new InvalidDataException($"Debugger state names definition '{path}', which no type of this build declares as a static member.");
            return catalog.Graph.Resolve(path);
        }
    }

    /// <summary>The catalogs and what each may hold; callers hold the lock.</summary>
    internal Declarations Declared => field ??= Declare();

    private Declarations Declare()
    {
        var catalogs = types.Catalogs
            .Select(catalog => new DefinitionCatalog(types, types.Format(catalog.Type), catalog.Members))
            .OrderBy(catalog => catalog.Prefix, StringComparer.Ordinal)
            .ToList();
        return new Declarations(catalogs.ToDictionary(catalog => catalog.Prefix, StringComparer.Ordinal), new DefinitionExposure(types, catalogs));
    }

    /// <summary>The catalogs of the game's static members and what each may hold.</summary>
    /// <param name="ByPrefix">Each catalog by the prefix its roots' names begin with.</param>
    /// <param name="Exposure">Which catalogs may hold an object of each type.</param>
    internal sealed record Declarations(IReadOnlyDictionary<string, DefinitionCatalog> ByPrefix, DefinitionExposure Exposure);
}
