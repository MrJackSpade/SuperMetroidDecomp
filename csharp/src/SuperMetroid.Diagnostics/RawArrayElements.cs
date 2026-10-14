namespace SuperMetroid.Desktop;

/// <summary>
/// Array element types written as their raw little-endian bytes: every primitive, plus the
/// 4-byte <see cref="SuperMetroid.Core.Assets.Rgba32"/> pixel, whose per-element object
/// encoding made one retained 256x224 display cost tens of megabytes.
/// </summary>
internal static class RawArrayElements
{
    /// <summary>Ensures packed RGBA pixels still occupy exactly four raw bytes.</summary>
    static RawArrayElements()
    {
        if (System.Runtime.CompilerServices.Unsafe.SizeOf<SuperMetroid.Core.Assets.Rgba32>() != 4)
            throw new InvalidOperationException("Rgba32 must remain four packed bytes to be serialized raw.");
    }

    /// <summary>Uses direct array bytes for primitives and packed RGBA pixels.</summary>
    internal static bool IsRaw(Type elementType) =>
        elementType.IsPrimitive || elementType == typeof(SuperMetroid.Core.Assets.Rgba32);

    /// <summary>The array's element storage viewed as bytes, in memory order across every dimension.</summary>
    internal static Span<byte> Bytes(Array array)
    {
        Type elementType = ReflectionBoundary.ElementType(array.GetType());
        int byteLength = elementType == typeof(SuperMetroid.Core.Assets.Rgba32)
            ? checked(array.Length * 4)
            : Buffer.ByteLength(array);
        return System.Runtime.InteropServices.MemoryMarshal.CreateSpan(
            ref System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(array), byteLength);
    }
}
