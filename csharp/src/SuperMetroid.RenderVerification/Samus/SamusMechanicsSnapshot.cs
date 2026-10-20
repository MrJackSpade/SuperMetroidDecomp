using SuperMetroid.Core;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.RenderVerification.Samus;

/// <summary>
/// Test-only field-level snapshot of the complete mutable Samus graph. New gameplay
/// fields automatically participate instead of escaping a manually maintained shortlist.
/// </summary>
internal sealed class SamusMechanicsSnapshot
{
    /// <summary>Caches ordered instance fields per type for repeatable graph traversal.</summary>
    private static readonly ConcurrentDictionary<Type, FieldInfo[]> Fields = new();
    /// <summary>Stores each mutable graph path and its invariant-culture value.</summary>
    private readonly SortedDictionary<string, string> values = new(StringComparer.Ordinal);

    /// <summary>Captures the Samus object graph, retaining reference cycles as path identities.</summary>
    internal SamusMechanicsSnapshot(object root)
    {
        var references = new Dictionary<object, string>(ReferenceEqualityComparer.Instance);
        Visit(root, "root");

        void Visit(object? value, string path)
        {
            if (value is null) { values.Add(path, "null"); return; }
            Type type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || value is string or decimal)
            {
                values.Add(path, type.FullName + ":" + Convert.ToString(value, CultureInfo.InvariantCulture));
                return;
            }
            if (!type.IsValueType)
            {
                if (references.TryGetValue(value, out string? original))
                { values.Add(path, "reference:" + original); return; }
                references.Add(value, path);
            }
            values.Add(path, (type.FullName ?? throw new InvalidOperationException("Verification requires type.FullName.")));
            if (value is IEnumerable sequence)
            {
                int index = 0;
                foreach (object? item in sequence) Visit(item, path + "[" + index++ + "]");
                values.Add(path + ".Count", index.ToString(CultureInfo.InvariantCulture));
                return;
            }
            foreach (FieldInfo field in Fields.GetOrAdd(type, SerializableFields))
            {
                Visit(field.GetValue(value), path + "." + (field.DeclaringType ?? throw new InvalidOperationException("Verification requires field.DeclaringType.")).Name + "." + field.Name);
            }
        }
    }

    /// <summary>Includes private gameplay fields while excluding nonserialized host catalogs and draw outputs.</summary>
    [ReflectionAudit("structural snapshot: every serializable field of the Samus graph participates, so new gameplay state cannot escape the comparison")]
    private static FieldInfo[] SerializableFields(Type type)
    {
        var result = new List<FieldInfo>();
        for (Type? owner = type; owner is not null; owner = owner.BaseType)
        foreach (FieldInfo field in owner.GetFields(BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .OrderBy(field => field.Name, StringComparer.Ordinal))
        {
            // Host-bound catalogs are not simulation state. The only mutable
            // exceptions are body draw outputs: OAM selectors/origin and pending
            // split DMA. Animation/frame timers, radii, pose/history and all
            // movement/special-sequence children remain compared, including private fields.
            if (field.IsDefined(typeof(NonSerializedAttribute)) ||
                field.FieldType == typeof(SamusTileTransferState) ||
                (owner == typeof(SamusPresentation) && field.Name is
                    "<TopSpritemapIndex>k__BackingField" or
                    "<BottomSpritemapIndex>k__BackingField" or
                    "<SpritemapPosition>k__BackingField")) continue;
            result.Add(field);
        }
        return result.ToArray();
    }

    /// <summary>Reports the first differing mechanics path between stock and edited Samus graphs.</summary>
    internal void RequireSame(SamusMechanicsSnapshot other, string context)
    {
        foreach (string key in values.Keys.Union(other.values.Keys).Order(StringComparer.Ordinal))
            if (!values.TryGetValue(key, out string? expected) ||
                !other.values.TryGetValue(key, out string? actual) || expected != actual)
                throw new InvalidOperationException($"{context}: mechanics differ at {key}: " +
                    $"stock={values.GetValueOrDefault(key, "<absent>")}, " +
                    $"edited={other.values.GetValueOrDefault(key, "<absent>")}.");
    }
}
