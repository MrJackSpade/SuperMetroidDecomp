using System.Text;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>How a debugger state stores a name: its UTF-8 length in seven-bit groups, low group first, then its bytes.</summary>
internal static class StateGraphNames
{
    /// <summary>The bytes a state stores for <paramref name="name"/>.</summary>
    internal static byte[] Encode(string name)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            writer.Write(name);
        return stream.ToArray();
    }
}
