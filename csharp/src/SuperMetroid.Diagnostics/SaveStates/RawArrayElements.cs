using SuperMetroid.Core.Assets;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Array element types written as their raw little-endian bytes: every primitive, among them an installed image's
/// recolour regions and blend map (#1633), plus the packed pixels of images, whose per-element object encoding made one
/// retained 256x224 display cost tens of megabytes (#1403).
/// </summary>
internal static class RawArrayElements
{
    /// <summary>The packed element types written raw, with the bytes each occupies.</summary>
    private static readonly FrozenDictionary<Type, int> PackedSizes = new Dictionary<Type, int>
    {
        [typeof(RgbaPixel)] = 4,
    }.ToFrozenDictionary();

    /// <summary>Ensures each packed type still occupies exactly its recorded raw bytes.</summary>
    static RawArrayElements()
    {
        if (Unsafe.SizeOf<RgbaPixel>() != PackedSizes[typeof(RgbaPixel)])
            throw new InvalidOperationException("RgbaPixel must remain four packed bytes to be serialized raw.");
    }

    /// <summary>Uses direct array bytes for primitives and the packed per-pixel types.</summary>
    internal static bool IsRaw(Type elementType) =>
        elementType.IsPrimitive || PackedSizes.ContainsKey(elementType);

    /// <summary>The array's element storage viewed as bytes, in memory order across every dimension.</summary>
    internal static Span<byte> Bytes(Array array)
    {
        Type elementType = ReflectionBoundary.ElementType(array.GetType());
        int byteLength = PackedSizes.TryGetValue(elementType, out int size)
            ? checked(array.Length * size)
            : Buffer.ByteLength(array);
        return MemoryMarshal.CreateSpan(ref MemoryMarshal.GetArrayDataReference(array), byteLength);
    }
}
