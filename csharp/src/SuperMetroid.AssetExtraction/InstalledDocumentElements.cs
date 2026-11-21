using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Rejects a JSON null element of an installed document's collection at load, naming the document and the member
/// (#2554). The serializer's nullable annotations reject a null member but never look inside the collection a member
/// holds, so without this an edited document's null element would surface only as a failure wherever the game first read
/// it. A document's collection holds present references: an element that may be absent is a nullable value, which keeps
/// its null. The check reads each document's generated contract (<see cref="InstalledDocumentJsonContext"/>): its
/// members' getters and its collections' element types.
/// </summary>
internal static class InstalledDocumentElements
{
    /// <summary>The installed documents' generated contract, with every document checking its collections' elements once read.</summary>
    internal static IJsonTypeInfoResolver Resolver { get; } = Over(InstalledDocumentJsonContext.Default);

    /// <summary><paramref name="contract"/>, with every object it describes checking its collections' elements once read.</summary>
    private static IJsonTypeInfoResolver Over(IJsonTypeInfoResolver contract) => contract.WithAddedModifier(RejectNullElements);

    /// <summary>Adds the element check to an object type, run over its members once the serializer has read it.</summary>
    private static void RejectNullElements(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;
        CheckedMember[] members = [.. typeInfo.Properties
            .Select(property => property.Get is { } get ? new CheckedMember(property.Name, get, property.PropertyType) : null)
            .OfType<CheckedMember>()];
        if (members.Length == 0)
            return;
        string document = typeInfo.Type.Name;
        JsonSerializerOptions options = typeInfo.Options;
        Action<object>? deserialized = typeInfo.OnDeserialized;
        typeInfo.OnDeserialized = value =>
        {
            foreach (CheckedMember member in members)
                if (member.Get(value) is IEnumerable collection and not string)
                    Check(document, member.Name, collection, options.GetTypeInfo(member.Type), options);
            deserialized?.Invoke(value);
        };
    }

    /// <summary>A document member the check reads: its name in the document, its getter and its declared type.</summary>
    /// <param name="Name">The member's name in the document.</param>
    /// <param name="Get">Reads the member from a read document.</param>
    /// <param name="Type">The member's declared type, whose contract names its collection's element type.</param>
    private sealed record CheckedMember(string Name, Func<object, object?> Get, Type Type);

    /// <summary>
    /// Throws at the first null element of <paramref name="collection"/>, a list's item or a dictionary's value, that is
    /// not a nullable value, descending into elements that are collections themselves.
    /// </summary>
    private static void Check(string document, string member, IEnumerable collection, JsonTypeInfo contract, JsonSerializerOptions options)
    {
        if (contract.Kind is not (JsonTypeInfoKind.Enumerable or JsonTypeInfoKind.Dictionary) || contract.ElementType is not { } elementType)
            return;
        bool mayBeAbsent = Nullable.GetUnderlyingType(elementType) is not null;
        IEnumerable values = collection is IDictionary dictionary ? dictionary.Values : collection;
        JsonTypeInfo? inner = null;
        foreach (object? element in values)
        {
            if (element is null)
            {
                if (!mayBeAbsent)
                    throw new JsonException($"{document}.{member} holds a null element; every element must be present.");
                continue;
            }
            if (element is IEnumerable nested and not string)
                Check(document, member, nested, inner ??= options.GetTypeInfo(elementType), options);
        }
    }
}
