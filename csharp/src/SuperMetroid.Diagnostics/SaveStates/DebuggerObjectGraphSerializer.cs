using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The debugger's state format (#2602): a game's whole object graph, written and restored through the members the compiler
/// gave every type of the game, so a state reaches nothing by reflection. It preserves private and read-only fields,
/// reference identity, cycles, arrays and the methods delegates call; it is not a general interchange format. A state
/// restores only this build's exact layout: a type, field or method this build lacks is refused as an unsupported version.
/// A collection keyed by hash is held as its entries and rebuilt from them (#1681); an array of value types whose fields are
/// all scalars or enums is written as one packed run (#1696); a type value is written as the name of the type it names (#2060).
/// </summary>
internal static class DebuggerObjectGraphSerializer
{
    /// <summary>The message prefix of every rejection of a state this build cannot restore exactly.</summary>
    internal const string UnsupportedVersion = "Unsupported debugger state version";

    /// <summary>
    /// Writes a debugger state of <paramref name="root"/>. An object <paramref name="bound"/> holds is written as its path
    /// among the host's bound objects (#1507), and an object the game's static members hold as its path among them (#1623).
    /// </summary>
    public static void Serialize(Stream destination, object root, BoundObjectGraph? bound = null) =>
        Serialize(destination, root, bound, DefinitionObjects.Process);

    /// <summary>Writes a debugger state, naming what the game's static members hold through <paramref name="definitions"/>.</summary>
    internal static void Serialize(Stream destination, object root, BoundObjectGraph? bound, DefinitionObjects definitions)
    {
        Ensure.NotNull(destination);
        Ensure.NotNull(root);
        Ensure.NotNull(definitions);
        using var writer = new BinaryWriter(destination, System.Text.Encoding.UTF8, leaveOpen: true);
        new GraphWriter(writer, StateGraphTypes.Shared, bound, definitions).Write(root);
    }

    /// <summary>
    /// Restores a debugger state whose root is a <typeparamref name="T"/>, resolving each bound object's path against
    /// <paramref name="bound"/> (#1507) and each static member's object's path to the object this build's member holds (#1623).
    /// </summary>
    public static T Deserialize<T>(Stream source, BoundObjectGraph? bound = null) where T : class =>
        Deserialize<T>(source, bound, DefinitionObjects.Process);

    /// <summary>Restores a debugger state, resolving what the game's static members hold through <paramref name="definitions"/>.</summary>
    internal static T Deserialize<T>(Stream source, BoundObjectGraph? bound, DefinitionObjects definitions) where T : class
    {
        Ensure.NotNull(source);
        Ensure.NotNull(definitions);
        using var reader = new BinaryReader(source, System.Text.Encoding.UTF8, leaveOpen: true);
        object? root = new GraphReader(reader, StateGraphTypes.Shared, bound, definitions).ReadState();
        return root as T ?? throw new InvalidDataException(
            $"Debugger state root is {root?.GetType().FullName ?? "null"}, expected {typeof(T).FullName}.");
    }
}
