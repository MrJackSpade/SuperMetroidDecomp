using System.Runtime.InteropServices;

namespace SuperMetroid.Core.Assets;

/// <summary>Cartridge values and helpers of <see cref="ObjCharacterTablePage"/> that only verification reads.</summary>
internal static class ObjCharacterTablePageConstants
{
    /// <summary>Whether two resolutions name the same memory, so the page composed from one shows the other.</summary>
    internal static bool SameSources(ReadOnlyMemory<byte>[] composed, ReadOnlyMemory<byte>[] current)
    {
        if (composed.Length != current.Length)
            return false;
        for (int index = 0; index < composed.Length; index++)
        {
            if (!MemoryMarshal.TryGetArray(composed[index], out ArraySegment<byte> before) ||
                !MemoryMarshal.TryGetArray(current[index], out ArraySegment<byte> after) ||
                !Object.ReferenceEquals(before.Array, after.Array) || before.Offset != after.Offset || before.Count != after.Count)
                return false;
        }
        return true;
    }
}
