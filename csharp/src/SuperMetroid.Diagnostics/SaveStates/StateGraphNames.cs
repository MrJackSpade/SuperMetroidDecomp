using System.Collections.Concurrent;
using System.Text;
using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>How a debugger state stores a name: its UTF-8 length in seven-bit groups, low group first, then its bytes.</summary>
internal static class StateGraphNames
{
    /// <summary>Each field's stored names, encoded once: a state writes them before every value of the field.</summary>
    private static readonly ConcurrentDictionary<StateGraphField, byte[]> fields = new(ReferenceEqualityComparer.Instance);

    /// <summary>The bytes a state stores for <paramref name="name"/>.</summary>
    internal static byte[] Encode(string name)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            writer.Write(name);
        return stream.ToArray();
    }

    /// <summary>The bytes a state stores for a field: its declaring type's name, then its own.</summary>
    internal static byte[] Encoded(StateGraphField field) =>
        fields.TryGetValue(field, out byte[]? known)
            ? known
            : fields.GetOrAdd(field, static named => [.. Encode(named.DeclaringType), .. Encode(named.Name)]);
}
