using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Rejects a JSON null element of an installed document's collection at load, naming the document and the member,
/// when the document declares that collection's elements non-nullable (#2554). The serializer's nullable annotations
/// reject a null member but never look inside the collection a member holds, so without this an edited document's
/// null element would surface only as a failure wherever the game first read it.
/// </summary>
[ReflectionAudit("A document's declared element nullability is serialization metadata, read once per document type.")]
internal static class InstalledDocumentElements
{
    /// <summary>The default resolver, with every document type checking its collections' elements once read.</summary>
    internal static IJsonTypeInfoResolver Resolver { get; } =
        new DefaultJsonTypeInfoResolver { Modifiers = { RejectNullElements } };

    /// <summary>Adds the element check to an object type whose members hold collections of declared-present elements.</summary>
    private static void RejectNullElements(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object) return;
        var context = new NullabilityInfoContext();
        var members = new List<(JsonPropertyInfo Property, NullabilityInfo Elements)>();
        foreach (JsonPropertyInfo property in typeInfo.Properties)
        {
            if (property.Get is null) continue;
            NullabilityInfo? declared = property.AttributeProvider switch
            {
                PropertyInfo member => context.Create(member),
                FieldInfo member => context.Create(member),
                _ => null,
            };
            if (declared is not null && Elements(declared) is { } elements && NeedsCheck(elements))
                members.Add((property, elements));
        }
        if (members.Count == 0) return;
        string document = typeInfo.Type.Name;
        Action<object>? deserialized = typeInfo.OnDeserialized;
        typeInfo.OnDeserialized = value =>
        {
            foreach ((JsonPropertyInfo property, NullabilityInfo elements) in members)
                if (property.Get!(value) is IEnumerable collection)
                    Check(document, property.Name, collection, elements);
            deserialized?.Invoke(value);
        };
    }

    /// <summary>
    /// The declared nullability of a collection's elements: an array's element, or a generic collection's last type
    /// argument (a list's item, a dictionary's value). A string is text, never a collection of elements.
    /// </summary>
    private static NullabilityInfo? Elements(NullabilityInfo declared)
    {
        if (declared.ElementType is { } element) return element;
        if (declared.Type == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(declared.Type)) return null;
        return declared.GenericTypeArguments.Length == 0 ? null : declared.GenericTypeArguments[^1];
    }

    /// <summary>Whether elements are declared present, or hold collections whose own elements are.</summary>
    private static bool NeedsCheck(NullabilityInfo elements) =>
        DeclaredPresent(elements) || Elements(elements) is { } inner && NeedsCheck(inner);

    /// <summary>A reference element the document declares non-nullable.</summary>
    private static bool DeclaredPresent(NullabilityInfo elements) =>
        !elements.Type.IsValueType && elements.ReadState == NullabilityState.NotNull;

    /// <summary>Throws at the first null element the declaration forbids, descending into nested collections.</summary>
    private static void Check(string document, string member, IEnumerable collection, NullabilityInfo elements)
    {
        IEnumerable values = collection is IDictionary dictionary ? dictionary.Values : collection;
        NullabilityInfo? inner = Elements(elements);
        foreach (object? element in values)
        {
            if (element is null)
            {
                if (DeclaredPresent(elements))
                    throw new JsonException($"{document}.{member} holds a null element; every element must be present.");
                continue;
            }
            if (inner is not null && NeedsCheck(inner) && element is IEnumerable nested)
                Check(document, member, nested, inner);
        }
    }
}
