using SuperMetroid.Core;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The objects the game's static members hold, and every object reachable from them, each addressed by its path from the
/// member it was first reached from (#1623): the catalogs' identities (item locations, doors, load stations, rooms, room
/// states, X-ray overlays), their behaviour objects and their definition data. A captured state names such an object by
/// its path instead of holding a copy, and a restore resolves the path to the object this build's member holds, so a
/// restored state holds the catalog's own members and every comparison and lookup against a catalog finds them. This is
/// the one place a debugger state keeps a catalog identity; identity types need no value equality of their own.
/// </summary>
/// <remarks>
/// A root is a static read-only field, an auto-property's included, of a type the game's assembly declares, named by its
/// declaring type and field; compiler-generated types are skipped, since their names are not stable between builds. Each
/// declaring type is a <see cref="DefinitionCatalog"/>, indexed only when a capture or restore first needs it (#1716): a
/// capture asks only the catalogs whose declarations can hold an object of the type it meets
/// (<see cref="DefinitionExposure"/>), and a restore only the catalog its path names, so neither runs the initializer of a
/// catalog the state never reaches. An object's path is its shortest from a root, the earliest root in name order among
/// the shortest: the path one breadth-first walk over every root in name order gives it, since every catalog holding the
/// object is asked and each catalog's roots are consecutive in that order.
/// </remarks>
internal sealed class DefinitionObjects
{
    /// <summary>The process's index, which every capture and restore consults unless handed another.</summary>
    internal static DefinitionObjects Process { get; } = new();

    /// <summary>Guards the catalogs' indexes, whose path and resolution caches every capture and restore share.</summary>
    private readonly Lock gate = new();

    /// <summary>The catalogs and what each may hold, read from the game assembly's declarations on first use.</summary>
    private Declarations? declarations;

    /// <summary>Every catalog of the game's static members, in their prefixes' ordinal order, none indexed until needed.</summary>
    internal IReadOnlyList<DefinitionCatalog> Catalogs
    {
        get
        {
            lock (gate)
                return Declared.Catalogs;
        }
    }

    /// <summary>
    /// The catalogs that may hold an object of <paramref name="type"/> where a game can reach it, in their prefixes'
    /// ordinal order, found from their declarations without indexing any (<see cref="DefinitionExposure"/>).
    /// </summary>
    internal IReadOnlyList<DefinitionCatalog> Candidates(Type type)
    {
        Ensure.NotNull(type);
        lock (gate)
            return Declared.Exposure.Candidates(type);
    }

    /// <summary>
    /// Whether <paramref name="value"/> is held by a static member, and its path when it is: asked of each catalog that
    /// may hold an object of its type, which indexes that catalog if no capture or restore has yet.
    /// </summary>
    internal bool TryGetPath(object value, [NotNullWhen(true)] out string? path)
    {
        Ensure.NotNull(value);
        lock (gate)
        {
            path = null;
            int shortest = int.MaxValue;
            foreach (DefinitionCatalog catalog in Candidates(value.GetType()))
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
    private Declarations Declared => declarations ??= Declare();

    /// <summary>
    /// Every type of the game's assembly with static read-only fields holding a reference, each as a catalog of those
    /// fields, and what each may hold. Nothing is read from a field, so no initializer runs.
    /// </summary>
    [ReflectionAudit("debugger states name the objects the game's static members hold by the member that holds them")]
    private static Declarations Declare()
    {
        Assembly game = typeof(Ensure).Assembly;
        Type[] types = game.GetTypes();
        var catalogs = new List<DefinitionCatalog>();
        foreach (Type type in types)
        {
            if (type.ContainsGenericParameters || type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false) ||
                type.FullName is not { } typeName || typeName.Contains('<', StringComparison.Ordinal))
                continue;
            FieldInfo[] members = [.. type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(field => field.IsInitOnly && !field.FieldType.IsPointer && !field.FieldType.IsByRef &&
                    !field.FieldType.IsValueType && field.FieldType != typeof(string))
                .OrderBy(field => field.Name, StringComparer.Ordinal)];
            if (members.Length != 0)
                catalogs.Add(new DefinitionCatalog(type, members));
        }
        catalogs.Sort((left, right) => string.CompareOrdinal(left.Prefix, right.Prefix));
        return new Declarations(catalogs, catalogs.ToDictionary(catalog => catalog.Prefix, StringComparer.Ordinal),
            new DefinitionExposure(game, types, catalogs));
    }

    /// <summary>The catalogs of the game's static members and what each may hold.</summary>
    /// <param name="Catalogs">Every catalog, in its prefix's ordinal order.</param>
    /// <param name="ByPrefix">Each catalog by the prefix its roots' names begin with.</param>
    /// <param name="Exposure">Which catalogs may hold an object of each type.</param>
    private sealed record Declarations(IReadOnlyList<DefinitionCatalog> Catalogs, IReadOnlyDictionary<string, DefinitionCatalog> ByPrefix,
        DefinitionExposure Exposure);
}
