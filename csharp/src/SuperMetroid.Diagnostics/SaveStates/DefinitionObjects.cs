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
/// declaring type and field; compiler-generated types are skipped, since their names are not stable between builds. Static
/// members hold definition data, fixed once initialized, so the index is built once per process, on the first capture or
/// restore that consults it, and never refreshed: a part a member loads on first use after that is written by its contents.
/// </remarks>
internal static class DefinitionObjects
{
    /// <summary>Separates a root's declaring type from its field name.</summary>
    private const string MemberSeparator = "::";

    /// <summary>Guards the index, whose path and resolution caches every capture and restore share.</summary>
    private static readonly Lock gate = new();

    /// <summary>The index of everything the game's static members reach, built on first use.</summary>
    private static readonly Lazy<BoundObjectGraph> index = new(Index);

    /// <summary>Whether <paramref name="value"/> is held by a static member, and its path when it is.</summary>
    internal static bool TryGetPath(object value, [NotNullWhen(true)] out string? path)
    {
        lock (gate)
            return index.Value.TryGetPath(value, out path);
    }

    /// <summary>The object <paramref name="path"/> names among the static members' objects.</summary>
    /// <exception cref="InvalidDataException">The path names no object this build's static members hold.</exception>
    internal static object Resolve(string path)
    {
        lock (gate)
            return index.Value.Resolve(path);
    }

    /// <summary>Indexes every object the game's static members reach.</summary>
    private static BoundObjectGraph Index() => new(Roots());

    /// <summary>
    /// The game assembly's static read-only fields holding a reference, each named by its declaring type and field, in
    /// name order so every process indexes them alike. Reading a field runs its type's initializer; a failing initializer
    /// fails the capture or restore that needed the index.
    /// </summary>
    [ReflectionAudit("debugger states name the objects the game's static members hold by the member that holds them")]
    private static (string Name, object Root)[] Roots()
    {
        var roots = new List<(string Name, object Root)>();
        foreach (Type type in typeof(Ensure).Assembly.GetTypes())
        {
            if (type.ContainsGenericParameters || type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false) ||
                type.FullName is not { } typeName || typeName.Contains('<', StringComparison.Ordinal))
                continue;
            foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (!field.IsInitOnly || field.FieldType.IsPointer || field.FieldType.IsByRef ||
                    field.FieldType.IsValueType || field.FieldType == typeof(string))
                    continue;
                if (field.GetValue(null) is { } value && value is not string)
                    roots.Add((typeName + MemberSeparator + field.Name, value));
            }
        }
        roots.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
        return [.. roots];
    }
}
